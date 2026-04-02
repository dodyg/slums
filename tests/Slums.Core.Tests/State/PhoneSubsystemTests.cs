using Slums.Core.Phone;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class PhoneSubsystemTests
{
    private static GameSession CreateSession(int money = 100)
    {
        var session = new GameSession();
        session.Player.Stats.SetMoney(money);
        return session;
    }

    private static PhoneMessage MakeMessage(
        string id = "test_msg",
        PhoneMessageType type = PhoneMessageType.Opportunity,
        string sender = "Mona",
        string senderNpcId = "NeighborMona",
        int dayReceived = 1,
        int? expiresAfterDay = 10,
        int responseTimeCost = 0,
        int responseMoneyCost = 0)
    {
        return new PhoneMessage
        {
            Id = id,
            Type = type,
            Sender = sender,
            SenderNpcId = senderNpcId,
            Content = "Test message",
            DayReceived = dayReceived,
            ExpiresAfterDay = expiresAfterDay,
            RequiresResponse = true,
            ResponseTimeCost = responseTimeCost,
            ResponseMoneyCost = responseMoneyCost
        };
    }

    [Test]
    public async Task RefillPhoneCredit_SucceedsWhenAffordable()
    {
        using var session = CreateSession(money: 10);
        session.Phone.DailyCreditDrain();
        session.Phone.DailyCreditDrain();
        session.Phone.DailyCreditDrain();
        session.Phone.DailyCreditDrain();
        session.Phone.DailyCreditDrain();
        session.Phone.DailyCreditDrain();
        session.Phone.DailyCreditDrain();
        var (success, _) = session.RefillPhoneCredit();
        await Assert.That(success).IsTrue();
        await Assert.That(session.Phone.CreditRemaining).IsGreaterThan(0);
    }

    [Test]
    public async Task RefillPhoneCredit_RejectsWhenNoPhone()
    {
        using var session = CreateSession();
        session.Phone.Restore(false, 0, 0, false, null, false);
        var (success, _) = session.RefillPhoneCredit();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RefillPhoneCredit_RejectsWhenPhoneLost()
    {
        using var session = CreateSession();
        session.Phone.LosePhone(1);
        var (success, _) = session.RefillPhoneCredit();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RefillPhoneCredit_RejectsWhenNotEnoughMoney()
    {
        using var session = CreateSession(money: 2);
        var (success, _) = session.RefillPhoneCredit();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_SucceedsForValidMessage()
    {
        using var session = CreateSession();
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsTrue();
    }

    [Test]
    public async Task RespondToMessage_RejectsWhenPhoneNotOperational()
    {
        using var session = CreateSession();
        session.Phone.Restore(true, 0, 10, false, null, false);
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_RejectsAlreadyResponded()
    {
        using var session = CreateSession();
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        session.RespondToMessage(msg.Id);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_RejectsExpired()
    {
        using var session = CreateSession();
        var msg = MakeMessage(expiresAfterDay: 5);
        session.PhoneMessages.AddMessage(msg);
        session.Clock.SetTime(10, 8, 0);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_RejectsIgnoredMessage()
    {
        using var session = CreateSession();
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        session.PhoneMessages.IgnoreMessage(msg.Id);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_ChargesForMissedCall()
    {
        using var session = CreateSession(money: 5);
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        session.PhoneMessages.MarkPendingAsMissed();
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(4);
    }

    [Test]
    public async Task RespondToMessage_RejectsMissedCallWithNoMoney()
    {
        using var session = CreateSession(money: 0);
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        session.PhoneMessages.MarkPendingAsMissed();
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_ChargesResponseMoneyCost()
    {
        using var session = CreateSession(money: 20);
        var msg = MakeMessage(responseMoneyCost: 10);
        session.PhoneMessages.AddMessage(msg);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(10);
    }

    [Test]
    public async Task RespondToMessage_RejectsWhenInsufficientMoneyCost()
    {
        using var session = CreateSession(money: 5);
        var msg = MakeMessage(responseMoneyCost: 10);
        session.PhoneMessages.AddMessage(msg);
        var (success, _) = session.RespondToMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RespondToMessage_OpportunityRecordsFavor()
    {
        using var session = CreateSession();
        var msg = MakeMessage(type: PhoneMessageType.Opportunity, senderNpcId: "NeighborMona");
        session.PhoneMessages.AddMessage(msg);
        session.RespondToMessage(msg.Id);
        var rel = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.WasHelped).IsTrue();
    }

    [Test]
    public async Task RespondToMessage_WarningReducesStress()
    {
        using var session = CreateSession();
        session.Player.Stats.SetStress(30);
        var msg = MakeMessage(type: PhoneMessageType.Warning, senderNpcId: "OfficerKhalid");
        session.PhoneMessages.AddMessage(msg);
        session.RespondToMessage(msg.Id);
        await Assert.That(session.Player.Stats.Stress).IsEqualTo(27);
    }

    [Test]
    public async Task RespondToMessage_BackgroundGrantsTrust()
    {
        using var session = CreateSession();
        var trustBefore = session.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust;
        var msg = MakeMessage(type: PhoneMessageType.Background, senderNpcId: "NurseSalma");
        session.PhoneMessages.AddMessage(msg);
        session.RespondToMessage(msg.Id);
        var trustAfter = session.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust;
        await Assert.That(trustAfter).IsEqualTo(trustBefore + 1);
    }

    [Test]
    public async Task IgnoreMessage_SucceedsForValidMessage()
    {
        using var session = CreateSession();
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        var (success, _, _) = session.IgnoreMessage(msg.Id);
        await Assert.That(success).IsTrue();
    }

    [Test]
    public async Task IgnoreMessage_RejectsWhenPhoneNotOperational()
    {
        using var session = CreateSession();
        session.Phone.Restore(true, 0, 10, false, null, false);
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        var (success, _, _) = session.IgnoreMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task IgnoreMessage_RejectsAlreadyHandled()
    {
        using var session = CreateSession();
        var msg = MakeMessage();
        session.PhoneMessages.AddMessage(msg);
        session.IgnoreMessage(msg.Id);
        var (success, _, _) = session.IgnoreMessage(msg.Id);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task IgnoreMessage_CausesTrustLossAfterThreshold()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);

        for (var i = 0; i < 5; i++)
        {
            var msg = MakeMessage(id: $"msg_{i}", senderNpcId: "NeighborMona");
            session.PhoneMessages.AddMessage(msg);
            session.IgnoreMessage(msg.Id);
        }

        var trust = session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust;
        await Assert.That(trust).IsLessThan(15);
    }

    [Test]
    public async Task IgnoreMessage_NoTrustLossBeforeThreshold()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);

        for (var i = 0; i < 3; i++)
        {
            var msg = MakeMessage(id: $"msg_{i}", senderNpcId: "NeighborMona");
            session.PhoneMessages.AddMessage(msg);
            session.IgnoreMessage(msg.Id);
        }

        var trust = session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust;
        await Assert.That(trust).IsEqualTo(15);
    }

    [Test]
    public async Task ReplacePhone_SucceedsWhenLost()
    {
        using var session = CreateSession(money: 50);
        session.Phone.LosePhone(1);
        var (success, _) = session.ReplacePhone();
        await Assert.That(success).IsTrue();
        await Assert.That(session.Phone.PhoneLost).IsFalse();
        await Assert.That(session.Phone.CreditRemaining).IsGreaterThan(0);
    }

    [Test]
    public async Task ReplacePhone_RejectsWhenNotLost()
    {
        using var session = CreateSession();
        var (success, _) = session.ReplacePhone();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task ReplacePhone_RejectsWhenNotEnoughMoney()
    {
        using var session = CreateSession(money: 20);
        session.Phone.LosePhone(1);
        var (success, _) = session.ReplacePhone();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task ReplacePhone_Charges30LE()
    {
        using var session = CreateSession(money: 50);
        session.Phone.LosePhone(1);
        session.ReplacePhone();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(20);
    }
}
