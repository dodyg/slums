namespace Slums.Core.State;

/// <summary>
/// Structured journal of events and automatic transactions, owned by the session so it is
/// included in snapshots and survives save/load. The UI renders from this journal.
/// </summary>
public sealed class EventJournal
{
    /// <summary>
    /// Retained entry cap. With roughly two to four logged events per day this covers a full
    /// 100-day run; older entries are dropped (the UI viewer highlights the retained window).
    /// </summary>
    public const int MaxEntries = 200;

    private readonly List<EventJournalEntry> _entries = new(MaxEntries);

    public IReadOnlyList<EventJournalEntry> Entries => _entries;

    public void Add(int day, EventSource source, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _entries.Add(new EventJournalEntry(day, source, message));
        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveRange(0, _entries.Count - MaxEntries);
        }
    }

    public void Clear()
    {
        _entries.Clear();
    }
}
