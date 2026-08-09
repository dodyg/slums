using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.State;
using TUnit;

namespace Slums.Application.Tests.Narrative;

internal sealed class NarrativeQueueCommandTests
{
    [Test]
    public void TryDequeueScene_ConsumesQueuedScene()
    {
        var command = new NarrativeQueueCommand();
        using var session = new GameSession();
        session.QueueNarrativeScene("event_test_scene");

        var found = command.TryDequeueScene(session, out var knotName);

        found.Should().BeTrue();
        knotName.Should().Be("event_test_scene");
        command.TryDequeueScene(session, out _).Should().BeFalse("the queue is consumed by dequeue");
    }

    [Test]
    public void TryTakeEndingKnot_ConsumesPendingEnding()
    {
        var command = new NarrativeQueueCommand();
        using var session = new GameSession();
        session.RestoreRunState(Guid.NewGuid(), daysSurvived: 1, isGameOver: false, gameOverReason: null, endingId: null, pendingEndingKnot: "ending_test");

        var found = command.TryTakeEndingKnot(session, out var knotName);

        found.Should().BeTrue();
        knotName.Should().Be("ending_test");
        command.TryTakeEndingKnot(session, out _).Should().BeFalse("the pending ending is consumed by take");
    }

    [Test]
    public void TryDequeueScene_ReturnsFalse_WhenQueueEmpty()
    {
        var command = new NarrativeQueueCommand();
        using var session = new GameSession();

        command.TryDequeueScene(session, out _).Should().BeFalse();
    }

    [Test]
    public void TryDequeueScene_Throws_WhenSessionIsNull()
    {
        var command = new NarrativeQueueCommand();

        var act = () => command.TryDequeueScene(null!, out _);

        act.Should().Throw<ArgumentNullException>();
    }
}
