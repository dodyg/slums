using Slums.Core.Characters;
using Slums.Core.Randomness;
using Slums.Core.State;
using TUnit;
using Slums.TestSupport;

namespace Slums.Core.Tests.State;

internal sealed class EndOfDayPipelineTests
{
    [Test]
    public async Task Run_ResolvesOneCompleteDayThroughTheSessionBoundary()
    {
        var session = TestSessions.Create(new GameRandom(20260904));
        session.Player.ApplyBackground(TestContent.Catalog.GetBackground(BackgroundType.SudaneseRefugee));

        EndOfDayPipeline.Run(session, session.SharedRandom);

        await Assert.That(session.Clock.Day).IsEqualTo(2);
        await Assert.That(session.DaysSurvived).IsEqualTo(1);
        await Assert.That(session.EventJournal.Entries).IsNotEmpty();
    }
}
