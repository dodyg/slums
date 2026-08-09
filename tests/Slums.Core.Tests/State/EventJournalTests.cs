using FluentAssertions;
using Slums.Core.State;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class EventJournalTests
{
    [Test]
    public async Task Add_RecordsEntriesInOrder()
    {
        var journal = new EventJournal();

        journal.Add(1, EventSource.GameEvent, "first");
        journal.Add(2, EventSource.AutoTransaction, "second");

        journal.Entries.Should().HaveCount(2);
        journal.Entries[0].Should().Be(new EventJournalEntry(1, EventSource.GameEvent, "first"));
        journal.Entries[1].Should().Be(new EventJournalEntry(2, EventSource.AutoTransaction, "second"));
    }

    [Test]
    public async Task Add_CapsEntriesAtMax()
    {
        var journal = new EventJournal();

        for (var i = 0; i < EventJournal.MaxEntries + 50; i++)
        {
            journal.Add(i, EventSource.GameEvent, $"entry {i}");
        }

        journal.Entries.Should().HaveCount(EventJournal.MaxEntries);
        journal.Entries[0].Message.Should().Be("entry 50", "the oldest entries are dropped");
        journal.Entries[^1].Message.Should().Be($"entry {EventJournal.MaxEntries + 49}");
    }

    [Test]
    public async Task Clear_EmptiesTheJournal()
    {
        var journal = new EventJournal();
        journal.Add(1, EventSource.GameEvent, "entry");

        journal.Clear();

        journal.Entries.Should().BeEmpty();
    }

    [Test]
    public async Task Add_Throws_WhenMessageIsBlank()
    {
        var journal = new EventJournal();

        var act = () => journal.Add(1, EventSource.GameEvent, "  ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task GameSession_RaiseEvent_JournalizesStructuredEntry()
    {
        using var session = new GameSession();

        session.EndDay();

        session.EventJournal.Entries.Should().NotBeEmpty();
        session.EventJournal.Entries.Should().OnlyContain(entry => entry.Day >= 1 && entry.Day <= session.Clock.Day);
        session.EventJournal.Entries.Should().Contain(entry => entry.Source == EventSource.AutoTransaction);
    }
}
