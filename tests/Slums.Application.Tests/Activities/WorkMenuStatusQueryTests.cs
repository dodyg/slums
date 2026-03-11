using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class WorkMenuStatusQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposeResolvedVariant_AndTrackProgress()
    {
        var query = new WorkMenuStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Clinic);
        gameState.Relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 1);
        gameState.JobProgress.RestoreTrack(JobType.ClinicReception, reliability: 58, shiftsCompleted: 4, lockoutUntilDay: 0);

        var statuses = query.GetStatuses(gameState);

        statuses.Should().ContainSingle();
        statuses[0].Job.Name.Should().Be("Clinic Intake Desk");
        statuses[0].Reliability.Should().Be(58);
        statuses[0].ShiftsCompleted.Should().Be(4);
        statuses[0].CanPerform.Should().BeTrue();
    }

    [Test]
    public void GetStatuses_ShouldExposeLockoutReason_WhenTrackIsBlocked()
    {
        var query = new WorkMenuStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.CallCenter);
        gameState.JobProgress.RestoreTrack(JobType.CallCenterWork, reliability: 41, shiftsCompleted: 2, lockoutUntilDay: 3);

        var statuses = query.GetStatuses(gameState);

        statuses.Should().ContainSingle();
        statuses[0].CanPerform.Should().BeFalse();
        statuses[0].LockoutUntilDay.Should().Be(3);
        statuses[0].AvailabilityReason.Should().Contain("shut out");
    }
}