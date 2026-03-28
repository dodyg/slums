using Microsoft.Extensions.Logging;

namespace Slums.Application.Persistence;

public sealed class SaveGameUseCase
{
    private readonly ISaveGameStore _saveGameStore;
    private readonly ILogger<SaveGameUseCase> _logger;

    public SaveGameUseCase(ISaveGameStore saveGameStore, ILogger<SaveGameUseCase> logger)
    {
        _saveGameStore = saveGameStore;
        _logger = logger;
    }

    public Task ExecuteAsync(SaveGameRequest request, string slot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        LogSavingGame(_logger, slot, request.GameSession.RunId, request.GameSession.DaysSurvived, request.GameSession.Player.Stats.Money);
        return _saveGameStore.SaveAsync(request, slot, cancellationToken);
    }

    private static readonly Action<ILogger, string, Guid, int, int, Exception?> LogSavingGameDelegate =
        LoggerMessage.Define<string, Guid, int, int>(LogLevel.Information, new EventId(200, "SavingGame"),
            "Saving game to slot {Slot}. RunId={RunId}, Day={Day}, Money={Money}");

    private static void LogSavingGame(ILogger logger, string slot, Guid runId, int day, int money) =>
        LogSavingGameDelegate(logger, slot, runId, day, money, null);
}
