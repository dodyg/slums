using FluentAssertions;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class TerritorySnapshotTests
{
    [Test]
    public async Task TerritorySnapshot_CaptureAndRestore_PreservesTension()
    {
        using var original = new GameSession(new Random(42));
        original.Territory.ModifyTension(DistrictId.Imbaba, 30);

        var snapshot = GameSessionTerritorySnapshot.Capture(original);
        using var restored = new GameSession(new Random(42));
        snapshot.Restore(restored);

        restored.Territory.GetControl(DistrictId.Imbaba).Tension.Should().Be(original.Territory.GetControl(DistrictId.Imbaba).Tension);
    }

    [Test]
    public async Task TerritorySnapshot_CaptureAndRestore_PreservesInfluence()
    {
        using var original = new GameSession(new Random(42));
        original.Territory.ModifyInfluence(DistrictId.Imbaba, FactionId.ImbabaCrew, 15);

        var snapshot = GameSessionTerritorySnapshot.Capture(original);
        using var restored = new GameSession(new Random(42));
        snapshot.Restore(restored);

        var originalInfluence = original.Territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];
        var restoredInfluence = restored.Territory.GetControl(DistrictId.Imbaba).FactionInfluence[FactionId.ImbabaCrew];
        restoredInfluence.Should().Be(originalInfluence);
    }

    [Test]
    public async Task TerritorySnapshot_CaptureAndRestore_PreservesAllDistricts()
    {
        using var original = new GameSession(new Random(42));

        var snapshot = GameSessionTerritorySnapshot.Capture(original);
        using var restored = new GameSession(new Random(42));
        snapshot.Restore(restored);

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            var orig = original.Territory.GetControl(district);
            var rest = restored.Territory.GetControl(district);
            rest.Tension.Should().Be(orig.Tension);
            rest.FactionInfluence.Count.Should().Be(orig.FactionInfluence.Count);
        }
    }

    [Test]
    public async Task TerritorySnapshot_CaptureAndRestore_PreservesLastConflictDay()
    {
        using var original = new GameSession(new Random(42));
        original.Territory.RestoreEntry(DistrictId.Dokki, new Dictionary<FactionId, int>
        {
            [FactionId.ImbabaCrew] = 10,
            [FactionId.DokkiThugs] = 60,
            [FactionId.ExPrisonerNetwork] = 5
        }, 30, 15);

        var snapshot = GameSessionTerritorySnapshot.Capture(original);
        using var restored = new GameSession(new Random(42));
        snapshot.Restore(restored);

        restored.Territory.GetControl(DistrictId.Dokki).LastConflictDay.Should().Be(15);
    }
}
