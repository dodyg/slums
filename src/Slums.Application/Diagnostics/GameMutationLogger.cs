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
        var entries = new List<string>(before.Count + after.Count + 1);
        foreach (var kvp in before)
        {
            entries.Add($"{kvp.Key}={kvp.Value}");
            if (entries.Count >= MaxSnapshotEntries)
            {
                return string.Join(", ", entries) + ", …";
            }
        }

        foreach (var kvp in after)
        {
            entries.Add($"{kvp.Key}={kvp.Value}");
            if (entries.Count >= MaxSnapshotEntries)
            {
                return string.Join(", ", entries) + ", …";
            }
        }

        return entries.Count == 0 ? "{}" : string.Join(", ", entries);
    }

    public void Dispose()
    {
        Detach();
    }
}
