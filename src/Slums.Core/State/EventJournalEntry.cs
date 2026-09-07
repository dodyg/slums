namespace Slums.Core.State;

/// <summary>A single structured event journal entry.</summary>
public sealed record EventJournalEntry(int Day, EventSource Source, string Message);
