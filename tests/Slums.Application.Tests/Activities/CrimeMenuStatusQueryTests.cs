using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class CrimeMenuStatusQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposeBlockedContactRouteReason()
    {
        var query = new CrimeMenuStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Market);

        var statuses = query.GetStatuses(gameState);

        var fencing = statuses.Single(static status => status.Attempt.Type == CrimeType.MarketFencing);
        fencing.IsAvailable.Should().BeFalse();
        fencing.BlockReason.Should().Contain("Hanan trust 10");
    }

    [Test]
    public void GetStatuses_ShouldMarkDokkiDropAvailable_WhenReliableWorkUnlockApplies()
    {
        var query = new CrimeMenuStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Square);
        gameState.JobProgress.RestoreTrack(JobType.CallCenterWork, reliability: 60, shiftsCompleted: 3, lockoutUntilDay: 0);

        var statuses = query.GetStatuses(gameState);

        var dokkiDrop = statuses.Single(static status => status.Attempt.Type == CrimeType.DokkiDrop);
        dokkiDrop.IsAvailable.Should().BeTrue();
        dokkiDrop.StatusText.Should().Contain("reliable day work");
    }

    [Test]
    public void GetStatuses_ShouldMarkNetworkErrandAvailable_WhenExPrisonerUnlockApplies()
    {
        var query = new CrimeMenuStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Market);
        gameState.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        gameState.Relationships.SetFactionStanding(FactionId.ExPrisonerNetwork, 10);

        var statuses = query.GetStatuses(gameState);

        var networkErrand = statuses.Single(static status => status.Attempt.Type == CrimeType.NetworkErrand);
        networkErrand.IsAvailable.Should().BeTrue();
        networkErrand.StatusText.Should().Contain("ex-prisoner network");
    }
}