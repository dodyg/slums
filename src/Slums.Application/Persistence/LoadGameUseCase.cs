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

    public async Task<LoadedGameSession?> ExecuteAsync(string slot, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        LogLoadingGame(_logger, slot);

        var result = await _saveGameStore.LoadAsync(slot, cancellationToken).ConfigureAwait(false);

        if (result is not null)
        {
            LogGameLoaded(_logger, slot, result.GameSession.RunId, result.GameSession.DaysSurvived);
        }
        else
        {
            LogGameLoadFailed(_logger, slot);
        }

        return result;
    }

    private static readonly Action<ILogger, string, Exception?> LogLoadingGameDelegate =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(201, "LoadingGame"),
            "Loading game from slot {Slot}.");

    private static readonly Action<ILogger, string, Guid, int, Exception?> LogGameLoadedDelegate =
        LoggerMessage.Define<string, Guid, int>(LogLevel.Information, new EventId(202, "GameLoaded"),
            "Loaded game from slot {Slot}. RunId={RunId}, Day={Day}");

    private static readonly Action<ILogger, string, Exception?> LogGameLoadFailedDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(203, "GameLoadFailed"),
            "Failed to load game from slot {Slot}.");

    private static void LogLoadingGame(ILogger logger, string slot) => LogLoadingGameDelegate(logger, slot, null);
    private static void LogGameLoaded(ILogger logger, string slot, Guid runId, int day) => LogGameLoadedDelegate(logger, slot, runId, day, null);
    private static void LogGameLoadFailed(ILogger logger, string slot) => LogGameLoadFailedDelegate(logger, slot, null);
}
