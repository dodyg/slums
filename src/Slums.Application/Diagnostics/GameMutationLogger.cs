using Microsoft.Extensions.Logging;
using Slums.Core.Diagnostics;
using Slums.Core.State;

namespace Slums.Application.Diagnostics;

public sealed class GameMutationLogger : IDisposable
{
    /// <summary>Upper bound for the formatted snapshot payload so one malformed mutation cannot flood a log line.</summary>
    private const int MaxSnapshotEntries = 20;

    private static readonly Action<ILogger, Guid, string, string, string, string, Exception?> MutationLogged =
        LoggerMessage.Define<Guid, string, string, string, string>(
            LogLevel.Debug,
            new EventId(LogEvents.MutationGameMutation, "GameMutation"),
            "[Mutation] RunId={RunId} Category={Category} Action={Action} Reason={Reason} {Snapshot}");

    private static readonly Action<ILogger, Guid, string, string, string, string, Exception?> MutationTransitioned =
        LoggerMessage.Define<Guid, string, string, string, string>(
            LogLevel.Information,
            new EventId(LogEvents.MutationGameMutation, "GameMutation"),
            "[Mutation] RunId={RunId} Category={Category} Action={Action} Reason={Reason} {Snapshot}");

    private static readonly Action<ILogger, Guid, string, string, string, string, Exception?> MutationEndingTriggered =
        LoggerMessage.Define<Guid, string, string, string, string>(
            LogLevel.Warning,
            new EventId(LogEvents.MutationGameMutation, "GameMutation"),
            "[Mutation] RunId={RunId} Category={Category} Action={Action} Reason={Reason} {Snapshot}");

    private readonly ILogger<GameMutationLogger> _logger;
    private GameSession? _session;

    public GameMutationLogger(ILogger<GameMutationLogger> logger)
    {
        _logger = logger;
    }

    public void Attach(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        Detach();
        _session = session;
        session.MutationRecorded += OnMutationRecorded;
    }

    public void Detach()
    {
        if (_session is not null)
        {
            _session.MutationRecorded -= OnMutationRecorded;
            _session = null;
        }
    }

    private void OnMutationRecorded(object? sender, GameMutationEventArgs e)
    {
        var record = e.Record;
        var snapshot = FormatSnapshot(record.Before, record.After);

        switch (record.Category)
        {
            case MutationCategories.EndingTriggered:
                MutationEndingTriggered(_logger, record.RunId, record.Category, record.Action, record.Reason, snapshot, null);
                break;
            case MutationCategories.DayTransition:
                MutationTransitioned(_logger, record.RunId, record.Category, record.Action, record.Reason, snapshot, null);
                break;
            default:
                MutationLogged(_logger, record.RunId, record.Category, record.Action, record.Reason, snapshot, null);
                break;
        }
    }

    private static string FormatSnapshot(IReadOnlyDictionary<string, object?> before, IReadOnlyDictionary<string, object?> after)
    {
        var entries = new List<string>(Math.Min(MaxSnapshotEntries, before.Count + after.Count));
        var beforeEntries = FormatSnapshotPart(before, entries);
        var afterEntries = FormatSnapshotPart(after, entries);
        return $"Before={{{beforeEntries}}} After={{{afterEntries}}}";
    }

    private static string FormatSnapshotPart(IReadOnlyDictionary<string, object?> values, List<string> entries)
    {
        var partEntries = new List<string>(Math.Min(values.Count, MaxSnapshotEntries));
        foreach (var kvp in values)
        {
            if (entries.Count >= MaxSnapshotEntries)
            {
                partEntries.Add("…");
                break;
            }

            var entry = $"{kvp.Key}={kvp.Value}";
            entries.Add(entry);
            partEntries.Add(entry);
        }

        return string.Join(", ", partEntries);
    }

    public void Dispose()
    {
        Detach();
    }
}
