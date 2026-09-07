using FluentAssertions;
using Slums.Core.Jobs;
using Slums.Core.Randomness;
using Slums.Core.Relationships;
using Slums.Core.Rumors;
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

    [Test]
    public void Validate_RejectsInvalidWeatherAndRandomState()
    {
        var snapshot = CompleteSnapshot() with
        {
            CurrentWeather = "NotAWeather",
            RandomState = new GameRandomState(0, 0, 0, 0)
        };

        var act = () => SaveGameValidator.Validate(snapshot);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*weather*random state*");
    }

    [Test]
    public void Validate_RejectsMalformedNestedCollections()
    {
        var snapshot = CompleteSnapshot() with
        {
            Rumors =
            [
                new RumorSnapshot(
                    "NotARumor",
                    "test",
                    "NotADistrict",
                    1,
                    7,
                    false,
                    ["NotANpc"],
                    -2,
                    [],
                    7,
                    0)
            ],
            Inventory = new GameSessionInventorySnapshot
            {
                Quantities = new Dictionary<string, int> { ["unknown_item"] = 1 }
            }
        };

        var act = () => SaveGameValidator.Validate(snapshot);

        act.Should().Throw<InvalidDataException>()
            .WithMessage("*rumor id*rumor district*rumor NPC*inventory item*");
    }

    private static GameSessionSnapshot CompleteSnapshot()
    {
        var snapshot = GameSessionSnapshot.Capture(new Slums.Core.State.GameSession());
        return snapshot;
    }
}
