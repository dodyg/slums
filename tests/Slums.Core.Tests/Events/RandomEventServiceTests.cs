using FluentAssertions;
using Slums.Core.Events;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Events;

internal sealed class RandomEventServiceTests
{
    [Test]
    public void RollDailyEvents_ShouldBeDeterministic_WithSeededRandom()
    {
        var service = new RandomEventService();
        var firstState = new GameState();
        firstState.Clock.SetTime(5, 6, 0);
        firstState.SetPolicePressure(70);
        firstState.Player.Household.SetMotherHealth(40);

        var secondState = new GameState();
        secondState.Clock.SetTime(5, 6, 0);
        secondState.SetPolicePressure(70);
        secondState.Player.Household.SetMotherHealth(40);

        var firstRoll = service.RollDailyEvents(firstState, new Random(99)).Select(static randomEvent => randomEvent.Id).ToArray();
        var secondRoll = service.RollDailyEvents(secondState, new Random(99)).Select(static randomEvent => randomEvent.Id).ToArray();

        secondRoll.Should().Equal(firstRoll);
    }

    [Test]
    public void RollDailyEvents_ShouldFilterMotherHealthScare_WhenMotherIsHealthy()
    {
        var service = new RandomEventService();
        var state = new GameState();
        state.Clock.SetTime(5, 6, 0);
        state.SetPolicePressure(70);
        state.Player.Household.SetMotherHealth(80);

        var events = service.RollDailyEvents(state, new Random(15));

        events.Select(static randomEvent => randomEvent.Id).Should().NotContain("MotherHealthScare");
    }

    [Test]
    public void AllEvents_ShouldIncludeNewLocationSpecificEvents()
    {
        var eventIds = RandomEventRegistry.AllEvents.Select(static randomEvent => randomEvent.Id);

        eventIds.Should().Contain("ClinicOverflow");
        eventIds.Should().Contain("WorkshopRushOrder");
        eventIds.Should().Contain("CafeSpill");
    }
}