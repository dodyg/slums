using FluentAssertions;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Infrastructure.Persistence;
using TUnit;

namespace Slums.Infrastructure.Tests;

internal sealed class SaveGameValidatorTests
{
    [Test]
    public void Validate_RejectsIncompleteRelationshipsAndJobTracks()
    {
        var snapshot = new GameSessionSnapshot();

        var act = () => SaveGameValidator.Validate(snapshot);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*relationships contain 0 NPC entries*job tracks contain 0 entries*");
    }

    [Test]
    public void Validate_RejectsCorruptedRelationshipTrustAndJobReliability()
    {
        var snapshot = CompleteSnapshot() with
        {
            Relationships = new GameSessionRelationshipSnapshot
            {
                Npcs = Enum.GetValues<NpcId>().ToDictionary(static npc => npc.ToString(), static _ => new GameSessionNpcRelationshipSnapshot { Trust = 101 }),
                Factions = Enum.GetValues<FactionId>().ToDictionary(static faction => faction.ToString(), static _ => 0)
            },
            JobProgress = new GameSessionJobProgressSnapshot
            {
                Tracks = Enum.GetValues<JobType>().ToDictionary(static job => job.ToString(), static _ => new GameSessionJobTrackSnapshot { Reliability = 101 })
            }
        };

        var act = () => SaveGameValidator.Validate(snapshot);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*relationship trust*job reliability*");
    }

    [Test]
    public void Restore_RejectsAnIncompleteSnapshotBeforeHydration()
    {
        var act = () => new GameSessionSnapshot().Restore();

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*relationships contain 0 NPC entries*");
    }

    private static GameSessionSnapshot CompleteSnapshot()
    {
        var snapshot = GameSessionSnapshot.Capture(new Slums.Core.State.GameSession());
        return snapshot;
    }
}
