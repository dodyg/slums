using System.Linq;

namespace Slums.Core.Phone;

public sealed class PhoneMessageState
{
    private readonly List<PhoneMessage> _inbox = [];
    private readonly Dictionary<string, int> _ignoredBySender = [];

    public IReadOnlyList<PhoneMessage> Inbox => _inbox;

    public void AddMessage(PhoneMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _inbox.Add(message);
    }

    public IReadOnlyList<PhoneMessage> GetActiveMessages(int currentDay)
    {
        return _inbox
            .Where(m => !m.IsExpired(currentDay))
            .OrderByDescending(m => m.DayReceived)
            .ToArray();
    }

    public IReadOnlyList<PhoneMessage> GetUnrespondedMessages(int currentDay)
    {
        return _inbox
            .Where(m => !m.Responded && !m.Ignored && !m.IsExpired(currentDay))
            .ToArray();
    }

    public PhoneMessage? GetMessage(string id)
    {
        return _inbox.FirstOrDefault(m => m.Id == id);
    }

    public bool RespondToMessage(string id)
    {
        var index = _inbox.FindIndex(m => m.Id == id);
        if (index < 0)
        {
            return false;
        }

        var message = _inbox[index];
        if (message.Responded || message.Ignored)
        {
            return false;
        }

        _inbox[index] = message.WithResponded();
        return true;
    }

    public int IgnoreMessage(string id)
    {
        var index = _inbox.FindIndex(m => m.Id == id);
        if (index < 0)
        {
            return 0;
        }

        var message = _inbox[index];
        if (message.Responded || message.Ignored)
        {
            return 0;
        }

        _inbox[index] = message.WithIgnored();

        var sender = message.Sender;
        _ignoredBySender[sender] = _ignoredBySender.TryGetValue(sender, out var count) ? count + 1 : 1;

        return _ignoredBySender[sender];
    }

    public int GetIgnoredCount(string sender)
    {
        return _ignoredBySender.TryGetValue(sender, out var count) ? count : 0;
    }

    public void RemoveExpired(int currentDay)
    {
        _inbox.RemoveAll(m => m.IsExpired(currentDay));
    }

    public int GetUnrespondedCount(int currentDay)
    {
        return _inbox.Count(m => !m.Responded && !m.Ignored && !m.IsExpired(currentDay));
    }

    public void MarkPendingAsMissed()
    {
        for (var i = 0; i < _inbox.Count; i++)
        {
            if (!_inbox[i].Responded && !_inbox[i].Ignored && !_inbox[i].WasMissed)
            {
                _inbox[i] = _inbox[i].WithMissed();
            }
        }
    }

    public void DeliverMissedMessages()
    {
        for (var i = 0; i < _inbox.Count; i++)
        {
            if (_inbox[i].WasMissed)
            {
                _inbox[i] = _inbox[i].WithDelivered();
            }
        }
    }

    public void RestoreMessages(IEnumerable<PhoneMessage> messages)
    {
        _inbox.Clear();
        _inbox.AddRange(messages);
    }
}
