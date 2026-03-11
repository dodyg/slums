using System.Text.Json;
using Microsoft.Extensions.Logging;
using Slums.Application.Narrative;
using Slums.Application.Persistence;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed class JsonSaveGameStore : ISaveGameStore
{
    private const int CurrentSaveVersion = 1;
    private readonly ILogger<JsonSaveGameStore> _logger;
    private readonly string _saveDirectory;

    public JsonSaveGameStore(ILogger<JsonSaveGameStore> logger, string? saveDirectory = null)
    {
        _logger = logger;
        _saveDirectory = saveDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Slums", "saves");
    }

    public async Task SaveAsync(GameState gameState, INarrativeService narrativeService, string slot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(narrativeService);
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        Directory.CreateDirectory(_saveDirectory);
        var path = GetSlotPath(slot);
        var now = DateTimeOffset.UtcNow;
        var existingEnvelope = await ReadEnvelopeAsync(path, cancellationToken).ConfigureAwait(false);

        var envelope = new SaveEnvelope(
            CurrentSaveVersion,
            gameState.RunId,
            existingEnvelope?.CreatedUtc ?? now,
            now,
            BuildCheckpointName(gameState),
            GameStateDto.FromGameState(gameState),
            new NarrativeStateDto { LastKnot = narrativeService.LastKnot });

        using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, envelope, SaveGameJsonContext.Default.SaveEnvelope, cancellationToken).ConfigureAwait(false);
    }

    public async Task<LoadedGameState?> LoadAsync(string slot, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        var path = GetSlotPath(slot);
        var envelope = await ReadEnvelopeAsync(path, cancellationToken).ConfigureAwait(false);
        if (envelope is null)
        {
            return null;
        }

        if (envelope.SaveVersion != CurrentSaveVersion)
        {
            LogVersionMismatch(_logger, slot, envelope.SaveVersion, CurrentSaveVersion);
            return null;
        }

        return new LoadedGameState(
            slot,
            envelope.CheckpointName,
            envelope.CreatedUtc,
            envelope.LastPlayedUtc,
            envelope.NarrativeState.LastKnot,
            envelope.GameState.ToGameState(envelope.RunId));
    }

    public async Task<IReadOnlyList<SaveSlotMetadata>> ListSlotsAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_saveDirectory))
        {
            return [];
        }

        var slots = new List<SaveSlotMetadata>();
        foreach (var filePath in Directory.EnumerateFiles(_saveDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            var envelope = await ReadEnvelopeAsync(filePath, cancellationToken).ConfigureAwait(false);
            if (envelope is null || envelope.SaveVersion != CurrentSaveVersion)
            {
                continue;
            }

            slots.Add(new SaveSlotMetadata(Path.GetFileNameWithoutExtension(filePath), envelope.CheckpointName, envelope.LastPlayedUtc));
        }

        return slots
            .OrderByDescending(static slot => slot.LastPlayedUtc)
            .ToArray();
    }

    private async Task<SaveEnvelope?> ReadEnvelopeAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync(stream, SaveGameJsonContext.Default.SaveEnvelope, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            LogSaveReadJsonFailure(_logger, path, exception);
            return null;
        }
        catch (IOException exception)
        {
            LogSaveReadIoFailure(_logger, path, exception);
            return null;
        }
    }

    private string GetSlotPath(string slot)
    {
        return Path.Combine(_saveDirectory, $"{slot}.json");
    }

    private static string BuildCheckpointName(GameState gameState)
    {
        var backgroundName = gameState.Player.Background?.Name ?? gameState.Player.BackgroundType.ToString();
        return $"{backgroundName} - Day {gameState.Clock.Day}";
    }

    private static readonly Action<ILogger, string, int, int, Exception?> LogVersionMismatchDelegate =
        LoggerMessage.Define<string, int, int>(LogLevel.Warning, new EventId(1, "SaveVersionMismatch"), "Rejecting save slot {Slot} due to version mismatch. Found {FoundVersion}, expected {ExpectedVersion}.");

    private static readonly Action<ILogger, string, Exception?> LogSaveReadJsonFailureDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, "SaveReadJsonFailure"), "Failed to parse save file {Path}.");

    private static readonly Action<ILogger, string, Exception?> LogSaveReadIoFailureDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3, "SaveReadIoFailure"), "Failed to read save file {Path}.");

    private static void LogVersionMismatch(ILogger logger, string slot, int foundVersion, int expectedVersion) =>
        LogVersionMismatchDelegate(logger, slot, foundVersion, expectedVersion, null);

    private static void LogSaveReadJsonFailure(ILogger logger, string path, Exception exception) =>
        LogSaveReadJsonFailureDelegate(logger, path, exception);

    private static void LogSaveReadIoFailure(ILogger logger, string path, Exception exception) =>
        LogSaveReadIoFailureDelegate(logger, path, exception);
}