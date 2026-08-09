using Microsoft.Extensions.Logging;

namespace Slums.Application.Persistence;

public sealed class LoadGameUseCase
{
    private readonly ISaveGameStore _saveGameStore;
    private readonly ILogger<LoadGameUseCase> _logger;

    public LoadGameUseCase(ISaveGameStore saveGameStore, ILogger<LoadGameUseCase> logger)
    {
        _saveGameStore = saveGameStore;
        _logger = logger;
    }

    public async Task<LoadGameResult> ExecuteAsync(string slot, CancellationToken cancellationToken = default)
    {
        SaveSlotRules.EnsureValidSlot(slot);

        LogLoadingGame(_logger, slot);

        var result = await _saveGameStore.LoadAsync(slot, cancellationToken).ConfigureAwait(false);

        if (result.Kind == LoadGameResultKind.Loaded && result.Session is not null)
        {
            LogGameLoaded(_logger, slot, result.Session.GameSession.RunId, result.Session.GameSession.DaysSurvived);
        }
        else
        {
            LogGameLoadFailed(_logger, slot, result.Kind, result.Detail);
        }

        return result;
    }

    private static readonly Action<ILogger, string, Exception?> LogLoadingGameDelegate =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(201, "LoadingGame"),
            "Loading game from slot {Slot}.");

    private static readonly Action<ILogger, string, Guid, int, Exception?> LogGameLoadedDelegate =
        LoggerMessage.Define<string, Guid, int>(LogLevel.Information, new EventId(202, "GameLoaded"),
            "Loaded game from slot {Slot}. RunId={RunId}, Day={Day}");

    private static readonly Action<ILogger, string, LoadGameResultKind, string?, Exception?> LogGameLoadFailedDetailDelegate =
        LoggerMessage.Define<string, LoadGameResultKind, string?>(LogLevel.Warning, new EventId(204, "GameLoadFailedDetail"),
            "Failed to load game from slot {Slot}: {Kind} ({Detail}).");

    private static void LogLoadingGame(ILogger logger, string slot) => LogLoadingGameDelegate(logger, slot, null);
    private static void LogGameLoaded(ILogger logger, string slot, Guid runId, int day) => LogGameLoadedDelegate(logger, slot, runId, day, null);
    private static void LogGameLoadFailed(ILogger logger, string slot, LoadGameResultKind kind, string? detail) => LogGameLoadFailedDetailDelegate(logger, slot, kind, detail, null);
}
