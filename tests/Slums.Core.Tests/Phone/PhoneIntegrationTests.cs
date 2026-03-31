using Slums.Core.Characters;
using Slums.Core.Phone;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Phone;

internal sealed class PhoneIntegrationTests
{
    [Test]
    public async Task GameSession_Initial_HasPhoneWithCredit()
    {
        using var session = new GameSession();
        await Assert.That(session.Phone.IsOperational()).IsTrue();
        await Assert.That(session.Phone.CreditRemaining).IsEqualTo(7);
    }

    [Test]
    public async Task GameSession_Initial_HasEmptyInbox()
    {
        using var session = new GameSession();
        await Assert.That(session.PhoneMessages.Inbox).Count().IsEqualTo(0);
    }

    [Test]
    public async Task GameSession_RefillPhoneCredit_DeductsMoney()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(20);

        for (var i = 0; i < 7; i++)
        {
            session.Phone.DailyCreditDrain();
        }

        await Assert.That(session.Phone.IsOperational()).IsFalse();

        var (success, message) = session.RefillPhoneCredit();

        await Assert.That(success).IsTrue();
        await Assert.That(session.Phone.IsOperational()).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(15);
    }

    [Test]
    public async Task GameSession_RefillPhoneCredit_FailsWhenNoMoney()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(2);

        for (var i = 0; i < 7; i++)
        {
            session.Phone.DailyCreditDrain();
        }

        var (success, _) = session.RefillPhoneCredit();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_RefillPhoneCredit_FailsWhenPhoneLost()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(100);
        session.Phone.LosePhone(1);

        var (success, _) = session.RefillPhoneCredit();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_RespondToMessage_Works()
    {
        using var session = new GameSession();
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "test-1",
            Sender = "Hanan",
            SenderNpcId = "FenceHanan",
            Type = PhoneMessageType.Opportunity,
            Content = "Meet me",
            DayReceived = 1,
            ExpiresAfterDay = 5,
            RequiresResponse = true,
            ResponseTimeCost = 1,
            ResponseMoneyCost = 0
        });

        var (success, _) = session.RespondToMessage("test-1");

        await Assert.That(success).IsTrue();
        await Assert.That(session.PhoneMessages.GetMessage("test-1")!.Responded).IsTrue();
    }

    [Test]
    public async Task GameSession_RespondToMessage_DeductsMoneyCost()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(20);
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "test-1",
            Sender = "Umm Karim",
            SenderNpcId = "FixerUmmKarim",
            Type = PhoneMessageType.NetworkRequest,
            Content = "Favor",
            DayReceived = 1,
            ExpiresAfterDay = 5,
            RequiresResponse = true,
            ResponseTimeCost = 1,
            ResponseMoneyCost = 5
        });

        var (success, _) = session.RespondToMessage("test-1");

        await Assert.That(success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(15);
    }

    [Test]
    public async Task GameSession_RespondToMessage_FailsWhenNotOperational()
    {
        using var session = new GameSession();
        session.Phone.LosePhone(1);
        session.PhoneMessages.AddMessage(new PhoneMessage { Id = "test-1", Content = "Test", DayReceived = 1 });

        var (success, _) = session.RespondToMessage("test-1");
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_IgnoreMessage_Works()
    {
        using var session = new GameSession();
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "test-1",
            Sender = "Mona",
            SenderNpcId = "NeighborMona",
            Content = "Check on mom",
            DayReceived = 1
        });

        var (success, _, _) = session.IgnoreMessage("test-1");

        await Assert.That(success).IsTrue();
        await Assert.That(session.PhoneMessages.GetMessage("test-1")!.Ignored).IsTrue();
    }

    [Test]
    public async Task GameSession_IgnoreMessage_TrustErosionAfterThree()
    {
        using var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.FixerUmmKarim, 12, 0);

        for (var i = 0; i < 4; i++)
        {
            session.PhoneMessages.AddMessage(new PhoneMessage
            {
                Id = $"msg-{i}",
                Sender = "Umm Karim",
                SenderNpcId = "FixerUmmKarim",
                Content = "Test",
                DayReceived = 1
            });

            session.IgnoreMessage($"msg-{i}");
        }

        var trust = session.Relationships.GetNpcRelationship(NpcId.FixerUmmKarim).Trust;
        await Assert.That(trust).IsEqualTo(11);
    }

    [Test]
    public async Task GameSession_IgnoreMessage_NoTrustErosionForLowTrust()
    {
        using var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 5, 0);

        for (var i = 0; i < 4; i++)
        {
            session.PhoneMessages.AddMessage(new PhoneMessage
            {
                Id = $"msg-{i}",
                Sender = "Mona",
                SenderNpcId = "NeighborMona",
                Content = "Test",
                DayReceived = 1
            });

            session.IgnoreMessage($"msg-{i}");
        }

        var trust = session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust;
        await Assert.That(trust).IsEqualTo(5);
    }

    [Test]
    public async Task GameSession_ReplacePhone_RestoresFunctionality()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(100);
        session.Phone.LosePhone(3);

        var (success, _) = session.ReplacePhone();

        await Assert.That(success).IsTrue();
        await Assert.That(session.Phone.IsOperational()).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(70);
    }

    [Test]
    public async Task GameSession_ReplacePhone_FailsWhenNotLost()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(100);

        var (success, _) = session.ReplacePhone();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_ReplacePhone_FailsWhenNotEnoughMoney()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(20);
        session.Phone.LosePhone(3);

        var (success, _) = session.ReplacePhone();
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task GameSession_EndDay_GeneratesMessages()
    {
        var found = false;
        for (var seed = 0; seed < 50; seed++)
        {
            using var session = new GameSession(new Random(seed));
            session.Relationships.SetNpcRelationship(NpcId.FenceHanan, 12, 0);
            session.EndDay();

            if (session.Mutations.Any(m => m.Category == "Phone"))
            {
                found = true;
                break;
            }
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    public async Task GameSession_EndDay_DrainsCredit()
    {
        using var session = new GameSession();
        session.EndDay();

        await Assert.That(session.Phone.DaysSinceCreditRefill).IsEqualTo(1);
        await Assert.That(session.Phone.CreditRemaining).IsEqualTo(6);
    }

    [Test]
    public async Task GameSession_EndDay_MarksMessagesAsMissedWhenNoCredit()
    {
        using var session = new GameSession();
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "pending",
            Content = "Test",
            DayReceived = 1,
            Responded = false
        });

        for (var i = 0; i < 7; i++)
        {
            session.Phone.DailyCreditDrain();
        }

        await Assert.That(session.Phone.IsOperational()).IsFalse();

        session.EndDay();

        await Assert.That(session.PhoneMessages.GetMessage("pending")!.WasMissed).IsTrue();
    }

    [Test]
    public async Task GameSession_RefillCredit_DeliversMissedMessages()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(100);
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "pending",
            Content = "Test",
            DayReceived = 1,
            Responded = false
        });

        for (var i = 0; i < 7; i++)
        {
            session.Phone.DailyCreditDrain();
        }

        session.PhoneMessages.MarkPendingAsMissed();
        await Assert.That(session.PhoneMessages.GetMessage("pending")!.WasMissed).IsTrue();

        session.RefillPhoneCredit();

        await Assert.That(session.PhoneMessages.GetMessage("pending")!.WasMissed).IsFalse();
    }

    [Test]
    public async Task GameSession_RespondToMissedCall_DeductsExtra1LE()
    {
        using var session = new GameSession();
        session.Player.Stats.SetMoney(20);
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "missed-1",
            Sender = "Hanan",
            SenderNpcId = "FenceHanan",
            Type = PhoneMessageType.Opportunity,
            Content = "Meet me",
            DayReceived = 1,
            ExpiresAfterDay = 10,
            RequiresResponse = true,
            ResponseTimeCost = 1,
            ResponseMoneyCost = 0,
            WasMissed = true
        });

        var (success, _) = session.RespondToMessage("missed-1");

        await Assert.That(success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(19);
    }

    [Test]
    public async Task GameSession_RestorePhone_PreservesState()
    {
        using var session = new GameSession();
        session.Phone.LosePhone(5);
        session.PhoneMessages.AddMessage(new PhoneMessage
        {
            Id = "saved-msg",
            Content = "Test",
            DayReceived = 3,
            Type = PhoneMessageType.Warning,
            Sender = "Abu Samir",
            SenderNpcId = "WorkshopBossAbuSamir"
        });

        session.RestorePhoneState(false, 3, 4, true, 5, false);
        session.RestorePhoneMessages([new PhoneMessage
        {
            Id = "restored-msg",
            Content = "Restored",
            DayReceived = 1,
            Type = PhoneMessageType.Tip,
            Sender = "Youssef",
            SenderNpcId = "RunnerYoussef"
        }]);

        await Assert.That(session.Phone.HasPhone).IsFalse();
        await Assert.That(session.Phone.CreditRemaining).IsEqualTo(3);
        await Assert.That(session.PhoneMessages.Inbox).Count().IsEqualTo(1);
        await Assert.That(session.PhoneMessages.Inbox[0].Id).IsEqualTo("restored-msg");
    }
}
