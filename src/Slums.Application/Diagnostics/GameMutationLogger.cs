using Microsoft.Extensions.Logging;
using Slums.Core.Diagnostics;
using Slums.Core.State;

namespace Slums.Application.Diagnostics;

public sealed class GameMutationLogger : IDisposable
{
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

#pragma warning disable CA1848
    private void OnMutationRecorded(object? sender, GameMutationEventArgs e)
    {
        var record = e.Record;
        var logLevel = record.Category switch
        {
            MutationCategories.EndingTriggered => LogLevel.Warning,
            MutationCategories.GuardRejected => LogLevel.Debug,
            MutationCategories.DayTransition => LogLevel.Information,
            _ => LogLevel.Debug
        };

        if (!_logger.IsEnabled(logLevel))
        {
            return;
        }

        _logger.Log(
            logLevel,
            new EventId(100, "GameMutation"),
            "[Mutation] RunId={RunId} Category={Category} Action={Action} Reason={Reason} Before={Before} After={After}",
            record.RunId,
            record.Category,
            record.Action,
            record.Reason,
            FormatSnapshot(record.Before),
            FormatSnapshot(record.After));
    }
#pragma warning restore CA1848

    private static string FormatSnapshot(IReadOnlyDictionary<string, object?> snapshot)
    {
        if (snapshot.Count == 0)
        {
            return "{}";
        }

        return string.Join(", ", snapshot.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    }

    public void Dispose()
    {
        Detach();
    }
}
