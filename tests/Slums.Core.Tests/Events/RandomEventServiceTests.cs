using FluentAssertions;
using Slums.Core.Events;
using Slums.Core.State;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Events;

internal sealed class RandomEventServiceTests
{
    [Test]
    public void RollDailyEvents_ShouldBeDeterministic_WithSeededRandom()
    {
        var service = new RandomEventService();
        var firstState = TestSessions.Create();
        firstState.Clock.SetTime(5, 6, 0);
        firstState.SetPolicePressure(70);
        firstState.Player.Household.SetMotherHealth(40);

        var secondState = TestSessions.Create();
        secondState.Clock.SetTime(5, 6, 0);
        secondState.SetPolicePressure(70);
        secondState.Player.Household.SetMotherHealth(40);

        var firstRoll = RandomEventService.RollDailyEvents(firstState, new Random(99)).Select(static randomEvent => randomEvent.Id).ToArray();
        var secondRoll = RandomEventService.RollDailyEvents(secondState, new Random(99)).Select(static randomEvent => randomEvent.Id).ToArray();

        secondRoll.Should().Equal(firstRoll);
    }

    [Test]
    public void RollDailyEvents_ShouldFilterMotherHealthScare_WhenMotherIsHealthy()
    {
        var service = new RandomEventService();
        var state = TestSessions.Create();
        state.Clock.SetTime(5, 6, 0);
        state.SetPolicePressure(70);
        state.Player.Household.SetMotherHealth(80);

        var events = RandomEventService.RollDailyEvents(state, new Random(15));

        events.Select(static randomEvent => randomEvent.Id).Should().NotContain("MotherHealthScare");
    }

    [Test]
    public void AllEvents_ShouldIncludeNewLocationSpecificEvents()
    {
        var eventIds = TestContent.Catalog.RandomEvents.Select(static randomEvent => randomEvent.Id);

        eventIds.Should().Contain("HomeWaterCutCollection");
        eventIds.Should().Contain("BakeryFlourShortage");
        eventIds.Should().Contain("ClinicOverflow");
        eventIds.Should().Contain("CallCenterScriptChange");
        eventIds.Should().Contain("WorkshopRushOrder");
        eventIds.Should().Contain("CafeSpill");
        eventIds.Should().Contain("NeighborhoodSolidarity");
        eventIds.Should().Contain("DokkiCheckpointSweep");
        eventIds.Should().Contain("DokkiTransportFriction");
        eventIds.Should().Contain("ClinicSupplyShortage");
        eventIds.Should().Contain("ArdAlLiwaWorkshopSolidarity");
        eventIds.Should().Contain("BulaqMedicineQueue");
        eventIds.Should().Contain("DepotFareShakeup");
        eventIds.Should().Contain("ShubraSteamBreak");
        eventIds.Should().Contain("ShubraBlockSolidarity");
    }

    [Test]
    public void RollDailyEvents_ShouldAllowDokkiCheckpointSweep_WhenInDokki()
    {
        var service = new RandomEventService();
        var state = TestSessions.Create();
        state.Clock.SetTime(6, 6, 0);
        state.World.TravelTo(Slums.Core.World.LocationId.CallCenter);
        state.SetPolicePressure(50);

        var events = RandomEventService.RollDailyEvents(state, new Random(1));

        TestContent.Catalog.RandomEvents.Single(static current => current.Id == "DokkiCheckpointSweep").Condition!(state).Should().BeTrue();
        events.Should().NotBeNull();
    }

    [Test]
    public void RollDailyEvents_ShouldAllowBulaqMedicineQueue_WhenAtPharmacy()
    {
        var service = new RandomEventService();
        var state = TestSessions.Create();
        state.Clock.SetTime(6, 6, 0);
        state.World.TravelTo(Slums.Core.World.LocationId.Pharmacy);

        var events = RandomEventService.RollDailyEvents(state, new Random(4));

        TestContent.Catalog.RandomEvents.Single(static current => current.Id == "BulaqMedicineQueue").Condition!(state).Should().BeTrue();
        events.Should().NotBeNull();
    }

    [Test]
    public void Registry_ShouldKeepBroaderDistrictEventsBalanced()
    {
        var checkpoint = TestContent.Catalog.RandomEvents.Single(static current => current.Id == "DokkiCheckpointSweep");
        var solidarity = TestContent.Catalog.RandomEvents.Single(static current => current.Id == "NeighborhoodSolidarity");
        var unexpectedWork = TestContent.Catalog.RandomEvents.Single(static current => current.Id == "UnexpectedWork");
        var homeWaterCut = TestContent.Catalog.RandomEvents.Single(static current => current.Id == "HomeWaterCutCollection");
        var bakeryFlourShortage = TestContent.Catalog.RandomEvents.Single(static current => current.Id == "BakeryFlourShortage");
        var callCenterScriptChange = TestContent.Catalog.RandomEvents.Single(static current => current.Id == "CallCenterScriptChange");

        checkpoint.Weight.Should().Be(8);
        solidarity.Weight.Should().Be(8);
        unexpectedWork.Effect.MoneyChange.Should().Be(22);
        homeWaterCut.Effect.EnergyChange.Should().Be(-4);
        bakeryFlourShortage.Effect.MoneyChange.Should().Be(8);
        callCenterScriptChange.Effect.StressChange.Should().Be(6);
    }

    [Test]
    public void RollDailyEvents_ShouldAllowBakeryFlourShortage_WhenAtBakery()
    {
        var service = new RandomEventService();
        var state = TestSessions.Create();
        state.Clock.SetTime(5, 6, 0);
        state.World.TravelTo(Slums.Core.World.LocationId.Bakery);

        var events = RandomEventService.RollDailyEvents(state, new Random(8));

        TestContent.Catalog.RandomEvents.Single(static current => current.Id == "BakeryFlourShortage").Condition!(state).Should().BeTrue();
        events.Should().NotBeNull();
    }
}
