using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Randomness;
using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit;

namespace Slums.Infrastructure.Tests;

internal sealed class EventJournalSnapshotTests
{
    [Test]
    public async Task CaptureAndRestore_PreservesJournalEntries()
    {
        using var session = new GameSession(new GameRandom(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.EndDay();
        session.EndDay();

        var snapshot = GameSessionSnapshot.Capture(session);
        using var restored = snapshot.Restore();

        restored.EventJournal.Entries.Should().HaveCount(session.EventJournal.Entries.Count);
        restored.EventJournal.Entries.Should().Equal(session.EventJournal.Entries,
            "the journal must survive save/load so the UI log is not empty after loading");
    }
}
