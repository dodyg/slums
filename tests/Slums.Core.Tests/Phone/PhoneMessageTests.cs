using Slums.Core.Phone;
using TUnit.Core;

namespace Slums.Core.Tests.Phone;

internal sealed class PhoneMessageTests
{
    [Test]
    public async Task PhoneMessage_IsExpired_ReturnsTrueWhenExpired()
    {
        var message = new PhoneMessage { DayReceived = 1, ExpiresAfterDay = 3 };
        await Assert.That(message.IsExpired(4)).IsTrue();
    }

    [Test]
    public async Task PhoneMessage_IsExpired_ReturnsFalseWhenNotExpired()
    {
        var message = new PhoneMessage { DayReceived = 1, ExpiresAfterDay = 3 };
        await Assert.That(message.IsExpired(2)).IsFalse();
    }

    [Test]
    public async Task PhoneMessage_IsExpired_ReturnsFalseOnExactDay()
    {
        var message = new PhoneMessage { DayReceived = 1, ExpiresAfterDay = 3 };
        await Assert.That(message.IsExpired(3)).IsFalse();
    }

    [Test]
    public async Task PhoneMessage_IsExpired_ReturnsFalseWhenNoExpiry()
    {
        var message = new PhoneMessage { DayReceived = 1 };
        await Assert.That(message.IsExpired(100)).IsFalse();
    }

    [Test]
    public async Task PhoneMessageState_AddMessage_IncreasesInbox()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Content = "Test" });

        await Assert.That(state.Inbox).Count().IsEqualTo(1);
    }

    [Test]
    public async Task PhoneMessageState_GetActiveMessages_ExcludesExpired()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Content = "Active", DayReceived = 1, ExpiresAfterDay = 5 });
        state.AddMessage(new PhoneMessage { Content = "Expired", DayReceived = 1, ExpiresAfterDay = 2 });

        var active = state.GetActiveMessages(3);
        await Assert.That(active).Count().IsEqualTo(1);
        await Assert.That(active[0].Content).IsEqualTo("Active");
    }

    [Test]
    public async Task PhoneMessageState_RespondToMessage_MarksResponded()
    {
        var state = new PhoneMessageState();
        var msg = new PhoneMessage { Id = "test-1", Content = "Test" };
        state.AddMessage(msg);

        var result = state.RespondToMessage("test-1");

        await Assert.That(result).IsTrue();
        await Assert.That(state.GetMessage("test-1")!.Responded).IsTrue();
    }

    [Test]
    public async Task PhoneMessageState_RespondToMessage_FailsForUnknownId()
    {
        var state = new PhoneMessageState();
        var result = state.RespondToMessage("nonexistent");
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PhoneMessageState_RespondToMessage_FailsIfAlreadyResponded()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "test-1" });
        state.RespondToMessage("test-1");

        var result = state.RespondToMessage("test-1");
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PhoneMessageState_IgnoreMessage_MarksIgnored()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "test-1", Sender = "Mona" });

        var count = state.IgnoreMessage("test-1");

        await Assert.That(count).IsEqualTo(1);
        await Assert.That(state.GetMessage("test-1")!.Ignored).IsTrue();
    }

    [Test]
    public async Task PhoneMessageState_IgnoreMessage_TracksIgnoredCount()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "msg-1", Sender = "Mona" });
        state.AddMessage(new PhoneMessage { Id = "msg-2", Sender = "Mona" });
        state.AddMessage(new PhoneMessage { Id = "msg-3", Sender = "Mona" });

        state.IgnoreMessage("msg-1");
        state.IgnoreMessage("msg-2");
        var third = state.IgnoreMessage("msg-3");

        await Assert.That(third).IsEqualTo(3);
        await Assert.That(state.GetIgnoredCount("Mona")).IsEqualTo(3);
    }

    [Test]
    public async Task PhoneMessageState_RemoveExpired_ClearsExpiredMessages()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "active", DayReceived = 1, ExpiresAfterDay = 10 });
        state.AddMessage(new PhoneMessage { Id = "expired", DayReceived = 1, ExpiresAfterDay = 2 });

        state.RemoveExpired(3);

        await Assert.That(state.Inbox).Count().IsEqualTo(1);
        await Assert.That(state.Inbox[0].Id).IsEqualTo("active");
    }

    [Test]
    public async Task PhoneMessageState_GetUnrespondedCount_ExcludesRespondedAndIgnored()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "a" });
        state.AddMessage(new PhoneMessage { Id = "b" });
        state.AddMessage(new PhoneMessage { Id = "c" });

        state.RespondToMessage("a");
        state.IgnoreMessage("b");

        await Assert.That(state.GetUnrespondedCount(1)).IsEqualTo(1);
    }

    [Test]
    public async Task PhoneMessageState_MarkPendingAsMissed_MarksUnresponded()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "a", Responded = false });
        state.AddMessage(new PhoneMessage { Id = "b", Responded = true });

        state.MarkPendingAsMissed();

        await Assert.That(state.GetMessage("a")!.WasMissed).IsTrue();
        await Assert.That(state.GetMessage("b")!.WasMissed).IsFalse();
    }

    [Test]
    public async Task PhoneMessageState_DeliverMissedMessages_ClearsMissedFlag()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "a", WasMissed = true });
        state.AddMessage(new PhoneMessage { Id = "b", WasMissed = false });

        state.DeliverMissedMessages();

        await Assert.That(state.GetMessage("a")!.WasMissed).IsFalse();
        await Assert.That(state.GetMessage("b")!.WasMissed).IsFalse();
    }

    [Test]
    public async Task PhoneMessageState_RestoreMessages_ClearsAndReplaces()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "old" });

        state.RestoreMessages([new PhoneMessage { Id = "new" }]);

        await Assert.That(state.Inbox).Count().IsEqualTo(1);
        await Assert.That(state.Inbox[0].Id).IsEqualTo("new");
    }

    [Test]
    public async Task PhoneMessageState_IgnoreMessage_FailsForAlreadyResponded()
    {
        var state = new PhoneMessageState();
        state.AddMessage(new PhoneMessage { Id = "test-1", Sender = "Mona" });
        state.RespondToMessage("test-1");

        var count = state.IgnoreMessage("test-1");
        await Assert.That(count).IsEqualTo(0);
    }
}
