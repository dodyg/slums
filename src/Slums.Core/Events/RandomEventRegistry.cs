namespace Slums.Core.Events;

/// <summary>Provides configured random-event definitions to legacy callers.</summary>
public static class RandomEventRegistry
{
    private static IReadOnlyList<RandomEvent>? _events;

    public static IReadOnlyList<RandomEvent> AllEvents => _events
        ?? throw new InvalidOperationException("Random-event content is not configured. Configure GameContentCatalog before querying random events.");

    public static void Configure(IEnumerable<RandomEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var configuredEvents = events.Where(static item => item is not null).ToArray();
        if (configuredEvents.Length == 0)
        {
            throw new InvalidOperationException("At least one random event must be configured.");
        }

        _events = configuredEvents;
    }
}
