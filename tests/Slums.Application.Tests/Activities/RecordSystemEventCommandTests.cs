using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class RecordSystemEventCommandTests
{
    [Test]
    public void Execute_ShouldJournalSystemEntryAndRaiseGameEvent()
    {
        var session = new GameSession();
        var command = new RecordSystemEventCommand();
        string? raisedMessage = null;
        session.GameEvent += (_, args) => raisedMessage = args.Message;

        command.Execute(session, "Saved game to slot1.");

        raisedMessage.Should().Be("Saved game to slot1.");
        session.EventJournal.Entries.Should().ContainSingle(entry =>
            entry.Source == EventSource.System &&
            entry.Day == session.Clock.Day &&
            entry.Message == "Saved game to slot1.");
    }

    [Test]
    public void Execute_ShouldThrow_WhenMessageIsNullOrWhitespace()
    {
        var session = new GameSession();
        var command = new RecordSystemEventCommand();

        var nullMessage = () => command.Execute(session, null!);
        var whitespaceMessage = () => command.Execute(session, "   ");

        Assert.Throws<ArgumentNullException>(nullMessage);
        Assert.Throws<ArgumentException>(whitespaceMessage);
        session.EventJournal.Entries.Should().BeEmpty();
    }

    [Test]
    public void Execute_ShouldThrow_WhenSessionIsNull()
    {
        var command = new RecordSystemEventCommand();

        var act = () => command.Execute(null!, "message");

        Assert.Throws<ArgumentNullException>(act);
    }
}
