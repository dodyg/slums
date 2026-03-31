using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class PhoneSnapshotTests
{
    [Test]
    public async Task PhoneSnapshot_CaptureAndRestore_PreservesState()
    {
        using var session = new Slums.Core.State.GameSession();
        session.Phone.LosePhone(5);
        session.PhoneMessages.AddMessage(new Slums.Core.Phone.PhoneMessage
        {
            Id = "snap-1",
            Type = Slums.Core.Phone.PhoneMessageType.Warning,
            Sender = "Abu Samir",
            SenderNpcId = "WorkshopBossAbuSamir",
            Content = "Don't come to Dokki",
            DayReceived = 3,
            ExpiresAfterDay = 6,
            RequiresResponse = false,
            WasMissed = true
        });

        var snapshot = GameSessionSnapshot.Capture(session);

        await Assert.That(snapshot.Phone.PhoneLost).IsTrue();
        await Assert.That(snapshot.Phone.PhoneLostDay).IsEqualTo(5);
        await Assert.That(snapshot.Phone.Messages).Count().IsEqualTo(1);
        await Assert.That(snapshot.Phone.Messages[0].Id).IsEqualTo("snap-1");
        await Assert.That(snapshot.Phone.Messages[0].Type).IsEqualTo("Warning");
        await Assert.That(snapshot.Phone.Messages[0].WasMissed).IsTrue();
    }

    [Test]
    public async Task PhoneSnapshot_Restore_PreservesState()
    {
        using var original = new Slums.Core.State.GameSession(new Random(42));
        original.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.GetByType(Slums.Core.Characters.BackgroundType.MedicalSchoolDropout));
        original.Player.Stats.SetMoney(100);
        original.Phone.LosePhone(3);
        original.PhoneMessages.AddMessage(new Slums.Core.Phone.PhoneMessage
        {
            Id = "test-msg",
            Type = Slums.Core.Phone.PhoneMessageType.Opportunity,
            Sender = "Hanan",
            SenderNpcId = "FenceHanan",
            Content = "Meet me",
            DayReceived = 2,
            ExpiresAfterDay = 5,
            RequiresResponse = true,
            ResponseTimeCost = 2,
            ResponseMoneyCost = 5
        });

        var snapshot = GameSessionSnapshot.Capture(original);
        using var restored = snapshot.Restore();

        await Assert.That(restored.Phone.PhoneLost).IsTrue();
        await Assert.That(restored.Phone.PhoneLostDay).IsEqualTo(3);
        await Assert.That(restored.PhoneMessages.Inbox).Count().IsEqualTo(1);
        await Assert.That(restored.PhoneMessages.Inbox[0].Id).IsEqualTo("test-msg");
        await Assert.That(restored.PhoneMessages.Inbox[0].Type).IsEqualTo(Slums.Core.Phone.PhoneMessageType.Opportunity);
        await Assert.That(restored.PhoneMessages.Inbox[0].WasMissed).IsFalse();
    }

    [Test]
    public async Task PhoneSnapshot_FullRoundtrip_PreservesAllFields()
    {
        using var original = new Slums.Core.State.GameSession(new Random(42));
        original.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.GetByType(Slums.Core.Characters.BackgroundType.SudaneseRefugee));
        original.Player.Stats.SetMoney(200);

        for (var i = 0; i < 4; i++)
        {
            original.Phone.DailyCreditDrain();
        }

        original.PhoneMessages.AddMessage(new Slums.Core.Phone.PhoneMessage
        {
            Id = "roundtrip-msg",
            Type = Slums.Core.Phone.PhoneMessageType.Tip,
            Sender = "Youssef",
            SenderNpcId = "RunnerYoussef",
            Content = "Heads up",
            DayReceived = 1,
            ExpiresAfterDay = 4,
            Responded = true,
            Ignored = false,
            WasMissed = false
        });

        var snapshot = GameSessionSnapshot.Capture(original);
        using var restored = snapshot.Restore();

        await Assert.That(restored.Phone.CreditRemaining).IsEqualTo(original.Phone.CreditRemaining);
        await Assert.That(restored.Phone.DaysSinceCreditRefill).IsEqualTo(original.Phone.DaysSinceCreditRefill);
        await Assert.That(restored.Phone.HasPhone).IsTrue();
        await Assert.That(restored.PhoneMessages.Inbox[0].Responded).IsTrue();
        await Assert.That(restored.PhoneMessages.Inbox[0].Content).IsEqualTo("Heads up");
    }

    [Test]
    public async Task PhoneSnapshot_EmptySnapshot_RestoresDefaults()
    {
        using var original = new Slums.Core.State.GameSession(new Random(42));
        original.Player.ApplyBackground(Slums.Core.Characters.BackgroundRegistry.GetByType(Slums.Core.Characters.BackgroundType.MedicalSchoolDropout));
        original.Player.Stats.SetMoney(100);

        var snapshot = GameSessionSnapshot.Capture(original);
        using var restored = snapshot.Restore();

        await Assert.That(restored.Phone.HasPhone).IsTrue();
        await Assert.That(restored.Phone.PhoneLost).IsFalse();
        await Assert.That(restored.PhoneMessages.Inbox).Count().IsEqualTo(0);
    }
}
