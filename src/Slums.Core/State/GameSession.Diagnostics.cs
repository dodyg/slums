using Slums.Core.Diagnostics;

namespace Slums.Core.State;

public sealed partial class GameSession
{
    internal Dictionary<string, object?> CaptureStats() => new()
    {
        ["Money"] = Player.Stats.Money,
        ["Hunger"] = Player.Stats.Hunger,
        ["Energy"] = Player.Stats.Energy,
        ["Health"] = Player.Stats.Health,
        ["Stress"] = Player.Stats.Stress,
        ["MotherHealth"] = Player.Household.MotherHealth,
        ["PolicePressure"] = PolicePressure,
        ["Day"] = CurrentDay,
        ["Location"] = World.CurrentLocationId.ToString(),
        ["FoodStockpile"] = Player.Household.FoodStockpile,
        ["RentDaysUnpaid"] = UnpaidRentDays,
    };

    internal void RecordMutation(string category, string action, Dictionary<string, object?> before, Dictionary<string, object?> after, string reason)
    {
        var record = new GameMutationRecord(RunId, DateTimeOffset.UtcNow, category, action, before, after, reason);
        _mutations.Add(record);
        MutationRecorded?.Invoke(this, new GameMutationEventArgs(record));
    }

    internal void RaiseEvent(string message)
    {
        RaiseEvent(message, EventSource.GameEvent);
    }

    internal void RaiseEvent(string message, EventSource source)
    {
        EventJournal.Add(Clock.Day, source, message);
        GameEvent?.Invoke(this, new GameEventArgs(message));
    }

    internal void RaiseAutoTransaction(string message)
    {
        RaiseEvent($"[Day {CurrentDay}] {message}", EventSource.AutoTransaction);
    }

    /// <summary>Replaces the event journal contents (used when restoring a save).</summary>
    internal void RestoreEventJournal(IEnumerable<EventJournalEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        EventJournal.Clear();
        foreach (var entry in entries)
        {
            EventJournal.Add(entry.Day, entry.Source, entry.Message);
        }
    }
}
