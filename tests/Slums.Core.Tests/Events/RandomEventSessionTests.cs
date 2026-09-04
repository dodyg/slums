using FluentAssertions;
using Slums.Core.Events;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Events;

internal sealed class RandomEventSessionTests
{
    [Test]
    public void ApplyEvent_ShouldApplyEffectsAndRecordTheEvent()
    {
        var session = new GameSession();
        var randomEvent = new RandomEvent(
            "session-test-event",
            "A small test event changes the household ledger.",
            new RandomEventEffect { MoneyChange = 12, StressChange = 3, InkKnot = "session_test_event" },
            1,
            1,
            null);

        RandomEventService.ApplyEvent(session, randomEvent);

        session.Player.Stats.Money.Should().Be(112);
        session.Player.Stats.Stress.Should().Be(23);
        session.GetEventCount(randomEvent.Id).Should().Be(1);
        session.PendingNarrativeScenes.Should().Contain("session_test_event");
        session.Mutations[^1].Action.Should().Be("ApplyRandomEvent");
    }
}
