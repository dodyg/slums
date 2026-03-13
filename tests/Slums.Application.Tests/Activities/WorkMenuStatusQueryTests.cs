using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
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
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Clinic);
        gameState.Relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 1);
        gameState.JobProgress.RestoreTrack(JobType.ClinicReception, reliability: 58, shiftsCompleted: 4, lockoutUntilDay: 0);

        var statuses = query.GetStatuses(WorkMenuContext.Create(gameState));

        statuses.Should().ContainSingle();
        statuses[0].Job.Name.Should().Be("Clinic Intake Desk");
        statuses[0].Reliability.Should().Be(58);
        statuses[0].ShiftsCompleted.Should().Be(4);
        statuses[0].CanPerform.Should().BeTrue();
        statuses[0].VariantReason.Should().Contain("Nurse Salma trust 10");
        statuses[0].NextUnlockHint.Should().Contain("Clinic Triage Support");
    }

    [Test]
    public void GetStatuses_ShouldExposeLockoutReason_WhenTrackIsBlocked()
    {
        var query = new WorkMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.CallCenter);
        gameState.JobProgress.RestoreTrack(JobType.CallCenterWork, reliability: 41, shiftsCompleted: 2, lockoutUntilDay: 3);

        var statuses = query.GetStatuses(WorkMenuContext.Create(gameState));

        statuses.Should().ContainSingle();
        statuses[0].CanPerform.Should().BeFalse();
        statuses[0].LockoutUntilDay.Should().Be(3);
        statuses[0].AvailabilityReason.Should().Contain("shut out");
    }

    [Test]
    public void GetStatuses_ShouldExposeActiveModifiers_AndRiskWarnings()
    {
        var query = new WorkMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Bakery);
        gameState.Player.Skills.SetLevel(SkillId.Physical, 3);
        gameState.Player.Stats.ModifyEnergy(-75);

        var statuses = query.GetStatuses(WorkMenuContext.Create(gameState));

        statuses.Should().ContainSingle();
        statuses[0].ActiveModifiers.Should().ContainSingle(static text => text.Contains("reduces energy cost by 5", StringComparison.Ordinal));
        statuses[0].RiskWarning.Should().Contain("low energy");
    }

    [Test]
    public void GetStatuses_ShouldExposeNarrativeSignals_ForHeatAndClinicHooks()
    {
        var query = new WorkMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Clinic);
        gameState.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        gameState.Relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 1);
        gameState.Player.Household.SetMotherHealth(60);
        gameState.SetPolicePressure(65);
        gameState.SetCrimeCounters(0, 0, lastCrimeDay: 1);
        gameState.SetWorkCounters(0, 0, lastHonestWorkDay: 0, lastPublicFacingWorkDay: 0);

        var statuses = query.GetStatuses(WorkMenuContext.Create(gameState));

        statuses.Should().ContainSingle();
        statuses[0].NarrativeSignals.Should().Contain(static text => text.Contains("police-heat suspicion", StringComparison.Ordinal));
        statuses[0].NarrativeSignals.Should().Contain(static text => text.Contains("medical-dropout reflection", StringComparison.Ordinal));
        statuses[0].NarrativeSignals.Should().Contain(static text => text.Contains("quietly help with medicine", StringComparison.Ordinal));
    }
}
