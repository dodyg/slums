using Slums.Core.Characters;
using Slums.Core.Endings;
using Slums.Core.Narrative;
using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit;

namespace Slums.Infrastructure.Tests;

internal sealed class UpgradeStateSnapshotTests
{
    [Test]
    public async Task Snapshot_ShouldPreserveTechnologyArcsAndPendingEnding()
    {
        var original = new GameSession();
        original.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
        original.Technology.RecordHandsetUse(4);
        original.Technology.RecordMicrogridRepair(7, 3);
        original.Technology.RecordBiometricAppeal();
        original.CentralCharacterArcs.RecordDecision(CentralCharacterId.NeighborMona, CentralArcDecision.MonaShareRota);
        original.SetDaysSurvived(30);
        original.Clock.SetTime(30, 8, 0);
        original.SetWorkCounters(180, 6, 30, 30);
        original.SetCrimeCounters(0, 0, 0);
        original.SetPolicePressure(10);
        original.TryChooseEnding(EndingId.StabilityHonestWork);

        var restored = GameSessionSnapshot.Capture(original).Restore();

        await Assert.That(restored.Technology.HandsetDataExposure).IsEqualTo(4);
        await Assert.That(restored.Technology.MicrogridRepairDebt).IsEqualTo(7);
        await Assert.That(restored.Technology.BiometricAppealPending).IsTrue();
        await Assert.That(restored.CentralCharacterArcs.GetDecision(CentralCharacterId.NeighborMona)).IsEqualTo(CentralArcDecision.MonaShareRota);
        await Assert.That(restored.PendingEndingId).IsEqualTo(EndingId.StabilityHonestWork);
        await Assert.That(restored.PendingEndingKnot).IsEqualTo(EndingKnotCatalog.Commitment);
    }
}
