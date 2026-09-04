using Slums.Core.Characters;
using Slums.Core.Randomness;
using Slums.Core.State;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class EndOfDayPipelineTests
{
    [Test]
    public async Task Run_ResolvesOneCompleteDayThroughTheSessionBoundary()
    {
        var session = new GameSession(new GameRandom(20260904));
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);

        EndOfDayPipeline.Run(session, session.SharedRandom);

        await Assert.That(session.Clock.Day).IsEqualTo(2);
        await Assert.That(session.DaysSurvived).IsEqualTo(1);
        await Assert.That(session.EventJournal.Entries).IsNotEmpty();
    }
}
