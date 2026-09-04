using Slums.Core.Jobs;
using Slums.Core.Randomness;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit;

namespace Slums.Core.Tests.State;

internal sealed class RestoreBoundaryTests
{
    [Test]
    public async Task RestoreFromSnapshot_ReturnsTheSameHydratedSession()
    {
        var session = new GameSession(new GameRandom(20260904));

        var restored = session.RestoreFromSnapshot(target =>
        {
            target.Clock.SetTime(4, 12, 30);
        });

        await Assert.That(restored).IsSameReferenceAs(session);
        await Assert.That(restored.Clock.Day).IsEqualTo(4);
    }

    [Test]
    public async Task RestoreFromSnapshot_RejectsANullRestoreDelegate()
    {
        var session = new GameSession(new GameRandom(20260904));

        await Assert.That(() => session.RestoreFromSnapshot(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RestoreFromSnapshot_KeepsCompleteRegistriesAfterRestore()
    {
        var session = new GameSession(new GameRandom(20260904));

        var restored = session.RestoreFromSnapshot(_ =>
        {
        });

        await Assert.That(restored.Relationships.NpcRelationships.Count).IsEqualTo(Enum.GetValues<NpcId>().Length);
        await Assert.That(restored.JobProgress.Tracks.Count).IsEqualTo(Enum.GetValues<JobType>().Length);
    }
}
