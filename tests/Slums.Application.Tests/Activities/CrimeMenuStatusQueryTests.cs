using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
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
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Market);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var fencing = statuses.Single(static status => status.Attempt.Type == CrimeType.MarketFencing);
        fencing.IsAvailable.Should().BeFalse();
        fencing.BlockReason.Should().Contain("Hanan trust 10");
        fencing.EffectiveDetectionRisk.Should().BeGreaterThan(0);
        fencing.EffectivePressureIfDetected.Should().BeGreaterThan(fencing.EffectivePressureIfUndetected);
        fencing.AccessSignals.Should().Contain(static text => text.Contains("Hanan trust: 0/10", StringComparison.Ordinal));
        fencing.RiskNotes.Should().Contain(static text => text.Contains("Base route profile", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldMarkDokkiDropAvailable_WhenReliableWorkUnlockApplies()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Square);
        gameState.JobProgress.RestoreTrack(JobType.CallCenterWork, reliability: 60, shiftsCompleted: 3, lockoutUntilDay: 0);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var dokkiDrop = statuses.Single(static status => status.Attempt.Type == CrimeType.DokkiDrop);
        dokkiDrop.IsAvailable.Should().BeTrue();
        dokkiDrop.StatusText.Should().Contain("reliable day work");
        dokkiDrop.AccessSignals.Should().Contain(static text => text.Contains("call center 60/60", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldMarkNetworkErrandAvailable_WhenExPrisonerUnlockApplies()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Market);
        gameState.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        gameState.Relationships.SetFactionStanding(FactionId.ExPrisonerNetwork, 10);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var networkErrand = statuses.Single(static status => status.Attempt.Type == CrimeType.NetworkErrand);
        networkErrand.IsAvailable.Should().BeTrue();
        networkErrand.StatusText.Should().Contain("ex-prisoner network");
    }

    [Test]
    public void GetStatuses_ShouldMarkDepotFareSkimAvailable_WhenReliableDepotWorkUnlockApplies()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Depot);
        gameState.JobProgress.RestoreTrack(JobType.MicrobusDispatch, reliability: 60, shiftsCompleted: 3, lockoutUntilDay: 0);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var fareSkim = statuses.Single(static status => status.Attempt.Type == CrimeType.DepotFareSkim);
        fareSkim.IsAvailable.Should().BeTrue();
        fareSkim.StatusText.Should().Contain("reliable depot work");
    }

    [Test]
    public void GetStatuses_ShouldMarkShubraBundleLiftAvailable_WhenReliableLaundryWorkUnlockApplies()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Laundry);
        gameState.JobProgress.RestoreTrack(JobType.LaundryPressing, reliability: 60, shiftsCompleted: 3, lockoutUntilDay: 0);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var bundleLift = statuses.Single(static status => status.Attempt.Type == CrimeType.ShubraBundleLift);
        bundleLift.IsAvailable.Should().BeTrue();
        bundleLift.StatusText.Should().Contain("reliable laundry work");
    }

    [Test]
    public void GetStatuses_ShouldExposeEffectiveCrimeModifiers()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Square);
        gameState.SetPolicePressure(70);
        gameState.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        gameState.Player.Skills.SetLevel(SkillId.StreetSmarts, 3);
        gameState.SetWorkCounters(totalHonestWorkEarnings: 0, honestShiftsCompleted: 0, lastHonestWorkDay: 0, lastPublicFacingWorkDay: gameState.Clock.Day);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var pettyTheft = statuses.Single(static status => status.Attempt.Type == CrimeType.PettyTheft);
        pettyTheft.ActiveModifiers.Should().Contain(static text => text.Contains("thin alibi", StringComparison.Ordinal));
        pettyTheft.ActiveModifiers.Should().Contain(static text => text.Contains("Street Smarts 3", StringComparison.Ordinal));
        pettyTheft.ActiveModifiers.Should().Contain(static text => text.Contains("political prisoner", StringComparison.OrdinalIgnoreCase));
        pettyTheft.EffectiveDetectionRisk.Should().BeGreaterThanOrEqualTo(5);
        pettyTheft.EffectiveSuccessChance.Should().BeGreaterThanOrEqualTo(10);
        pettyTheft.RiskNotes.Should().Contain(static text => text.Contains("Current police pressure (70)", StringComparison.Ordinal));
        pettyTheft.RiskNotes.Should().Contain(static text => text.Contains("Modifier sources", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldExposeNarrativeSignals_ForHomeSuspicionAndMonaWarning()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Square);
        gameState.SetCrimeCounters(120, 1);
        gameState.Player.Household.SetMotherHealth(50);
        gameState.SetPolicePressure(65);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 18, 1);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var pettyTheft = statuses.Single(static status => status.Attempt.Type == CrimeType.PettyTheft);
        pettyTheft.NarrativeSignals.Should().Contain(static text => text.Contains("first successful crime", StringComparison.Ordinal));
        pettyTheft.NarrativeSignals.Should().Contain(static text => text.Contains("suspicious tonight", StringComparison.Ordinal));
        pettyTheft.NarrativeSignals.Should().Contain(static text => text.Contains("Mona", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldExposeDistrictCrimePressure()
    {
        var query = new CrimeMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Square);
        gameState.World.SetActiveDistrictConditions(
        [
            new ActiveDistrictCondition { District = DistrictId.Dokki, ConditionId = "dokki_checkpoint_sweep" }
        ]);

        var statuses = query.GetStatuses(CrimeMenuContext.Create(gameState));

        var pettyTheft = statuses.Single(static status => status.Attempt.Type == CrimeType.PettyTheft);
        pettyTheft.ActiveModifiers.Should().Contain(static text => text.Contains("Checkpoint Sweep", StringComparison.Ordinal));
        pettyTheft.EffectiveDetectionRisk.Should().BeGreaterThan(25);
    }
}
