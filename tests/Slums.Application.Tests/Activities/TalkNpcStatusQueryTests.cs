using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class TalkNpcStatusQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposeReachableNpcMemoryFlags()
    {
        var query = new TalkNpcStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Home);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 16, 3);
        gameState.Relationships.SetNpcRelationshipMemory(
            NpcId.NeighborMona,
            lastFavorDay: 2,
            lastRefusalDay: 0,
            hasUnpaidDebt: false,
            wasEmbarrassed: false,
            wasHelped: true,
            recentContactCount: 3);

        var statuses = query.GetStatuses(gameState);

        var mona = statuses.Single(static npc => npc.NpcId == NpcId.NeighborMona);
        mona.Summary.Should().Contain("stairwell");
        mona.MemoryFlags.Should().Contain("Remembers help");
        mona.MemoryFlags.Should().Contain("Last favor: day 2");
        mona.MemoryFlags.Should().Contain("Recent contact: 3");
        mona.TriggerSignals.Should().Contain(static text => text.Contains("Past mutual help", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldExposeFactionAndHeatSummaries()
    {
        var query = new TalkNpcStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Square);
        gameState.SetPolicePressure(75);
        gameState.Relationships.SetFactionStanding(FactionId.DokkiThugs, 18);

        var statuses = query.GetStatuses(gameState);

        var khalid = statuses.Single(static npc => npc.NpcId == NpcId.OfficerKhalid);
        var youssef = statuses.Single(static npc => npc.NpcId == NpcId.RunnerYoussef);
        khalid.Summary.Should().Contain("Checkpoint mood");
        khalid.FactionLink.Should().Contain("Police pressure: 75");
        youssef.FactionLink.Should().Contain("Dokki Thugs: 18");
        youssef.Summary.Should().Contain("route is too hot");
    }

    [Test]
    public void GetStatuses_ShouldExposeDoubleLifeSuspicion()
    {
        var query = new TalkNpcStatusQuery();
        var gameState = new GameState();
        gameState.World.TravelTo(LocationId.Clinic);
        gameState.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        gameState.SetCrimeCounters(totalCrimeEarnings: 90, crimesCommitted: 2);
        gameState.SetWorkCounters(totalHonestWorkEarnings: 120, honestShiftsCompleted: 3, lastCrimeDay: 1, lastHonestWorkDay: 2, lastPublicFacingWorkDay: 2);

        var statuses = query.GetStatuses(gameState);

        var salma = statuses.Single(static npc => npc.NpcId == NpcId.NurseSalma);
        salma.Summary.Should().Contain("stories and your days stop matching");
        salma.TriggerSignals.Should().Contain(static text => text.Contains("double life", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void GetStatuses_ShouldExposeLowMoneyAndUrgentCareSummaries()
    {
        var query = new TalkNpcStatusQuery();

        var homeState = new GameState();
        homeState.World.TravelTo(LocationId.Home);
        homeState.Player.Stats.ModifyMoney(-85);
        var homeStatuses = query.GetStatuses(homeState);

        homeStatuses.Single(static npc => npc.NpcId == NpcId.LandlordHajjMahmoud).Summary.Should().Contain("visibly short");
        homeStatuses.Single(static npc => npc.NpcId == NpcId.NeighborMona).Summary.Should().Contain("week tightening");

        var clinicState = new GameState();
        clinicState.World.TravelTo(LocationId.Clinic);
        clinicState.Player.Household.SetMotherHealth(30);
        var clinicStatuses = query.GetStatuses(clinicState);

        clinicStatuses.Single(static npc => npc.NpcId == NpcId.NurseSalma).Summary.Should().Contain("mother's condition");
    }

    [Test]
    public void GetStatuses_ShouldExposeConversationTriggers_ForDebtAndHeat()
    {
        var query = new TalkNpcStatusQuery();

        var clinicState = new GameState();
        clinicState.World.TravelTo(LocationId.Clinic);
        clinicState.Relationships.SetNpcRelationship(NpcId.NurseSalma, 18, 1);
        clinicState.Relationships.SetNpcRelationshipMemory(
            NpcId.NurseSalma,
            lastFavorDay: 2,
            lastRefusalDay: 0,
            hasUnpaidDebt: true,
            wasEmbarrassed: false,
            wasHelped: false,
            recentContactCount: 2);

        var homeState = new GameState();
        homeState.World.TravelTo(LocationId.Home);
        homeState.SetPolicePressure(75);
        homeState.SetCrimeCounters(120, 2);

        var clinicStatuses = query.GetStatuses(clinicState);
        var homeStatuses = query.GetStatuses(homeState);

        clinicStatuses.Single(static npc => npc.NpcId == NpcId.NurseSalma).TriggerSignals.Should().Contain(static text => text.Contains("owe Salma", StringComparison.Ordinal));
        clinicStatuses.Single(static npc => npc.NpcId == NpcId.NurseSalma).TriggerSignals.Should().Contain(static text => text.Contains("High trust", StringComparison.Ordinal));
        homeStatuses.Single(static npc => npc.NpcId == NpcId.NeighborMona).TriggerSignals.Should().Contain(static text => text.Contains("Police heat", StringComparison.Ordinal));
    }
}