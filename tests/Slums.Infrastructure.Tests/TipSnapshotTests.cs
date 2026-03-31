using Slums.Core.Characters;
using Slums.Core.Heat;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class TipSnapshotTests
{
    [Test]
    public async Task TipSnapshot_Capture_CapturesTips()
    {
        using var session = new GameSession(new Random(1));
        session.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Test tip",
            DayGenerated = 1,
            ExpiresAfterDay = 3,
            RelevantDistrict = DistrictId.Imbaba,
            IsEmergency = true
        });

        var snapshot = GameSessionTipSnapshot.Capture(session);

        await Assert.That(snapshot.Tips).Count().IsEqualTo(1);
        await Assert.That(snapshot.Tips[0].Content).IsEqualTo("Test tip");
        await Assert.That(snapshot.Tips[0].IsEmergency).IsTrue();
        await Assert.That(snapshot.Tips[0].RelevantDistrict).IsEqualTo("Imbaba");
    }

    [Test]
    public async Task TipSnapshot_Restore_RestoresTips()
    {
        using var session = new GameSession(new Random(1));
        session.Tips.AddTip(new Tip
        {
            Type = TipType.PoliceTip,
            Source = NpcId.OfficerKhalid,
            Content = "Original",
            DayGenerated = 1,
            ExpiresAfterDay = 3
        });
        session.Tips.IgnoreTip(session.Tips.AllTips[0].Id);

        var snapshot = GameSessionTipSnapshot.Capture(session);

        using var restored = new GameSession(new Random(2));
        snapshot.Restore(restored);

        await Assert.That(restored.Tips.AllTips).Count().IsEqualTo(1);
        await Assert.That(restored.Tips.AllTips[0].Content).IsEqualTo("Original");
        await Assert.That(restored.Tips.GetIgnoredCount(NpcId.OfficerKhalid)).IsEqualTo(1);
    }

    [Test]
    public async Task TipSnapshot_Roundtrip_PreservesState()
    {
        using var session = new GameSession(new Random(1));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.MedicalSchoolDropout));
        session.Tips.AddTip(new Tip
        {
            Type = TipType.JobLead,
            Source = NpcId.NurseSalma,
            Content = "Extra shifts",
            DayGenerated = 5,
            ExpiresAfterDay = 7,
            Delivered = true
        });
        session.Tips.AddTip(new Tip
        {
            Type = TipType.CrimeWarning,
            Source = NpcId.FenceHanan,
            Content = "Heads up",
            DayGenerated = 5,
            ExpiresAfterDay = 6,
            RelevantDistrict = DistrictId.Dokki
        });
        session.Tips.AcknowledgeTip(session.Tips.AllTips[0].Id);

        var snapshot = GameSessionTipSnapshot.Capture(session);

        using var restored = new GameSession(new Random(2));
        snapshot.Restore(restored);

        await Assert.That(restored.Tips.AllTips).Count().IsEqualTo(2);
        await Assert.That(restored.Tips.AllTips[0].Delivered).IsTrue();
        await Assert.That(restored.Tips.AllTips[1].RelevantDistrict).IsEqualTo(DistrictId.Dokki);
    }

    [Test]
    public async Task TipSnapshot_Empty_CapturesAndRestoresCleanly()
    {
        using var session = new GameSession(new Random(1));
        var snapshot = GameSessionTipSnapshot.Capture(session);

        await Assert.That(snapshot.Tips).Count().IsEqualTo(0);

        using var restored = new GameSession(new Random(2));
        snapshot.Restore(restored);
        await Assert.That(restored.Tips.AllTips).Count().IsEqualTo(0);
    }
}
