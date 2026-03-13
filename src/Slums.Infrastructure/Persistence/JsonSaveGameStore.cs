using System.Text.Json;
using Microsoft.Extensions.Logging;
using Slums.Application.Persistence;

namespace Slums.Infrastructure.Persistence;

public sealed class JsonSaveGameStore : ISaveGameStore
{
    private const int CurrentSaveVersion = 2;
    private const int StreamBufferSize = 4096;
    private readonly ILogger<JsonSaveGameStore> _logger;
    private readonly string _saveDirectory;

    public JsonSaveGameStore(ILogger<JsonSaveGameStore> logger, string? saveDirectory = null)
    {
        _logger = logger;
        _saveDirectory = saveDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Slums", "saves");
    }

    public async Task SaveAsync(SaveGameRequest request, string slot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        Directory.CreateDirectory(_saveDirectory);
        var path = GetSlotPath(slot);
        var now = DateTimeOffset.UtcNow;
        var existingDocument = await ReadDocumentAsync(path, cancellationToken).ConfigureAwait(false);

        var document = new GameSessionSaveDocument(
            CurrentSaveVersion,
            existingDocument?.CreatedUtc ?? now,
            now,
            request.CheckpointName,
            GameSessionSnapshot.Capture(request.GameSession),
            new NarrativeProgressSnapshot { LastKnot = request.LastKnot });

        var stream = OpenWriteStream(path);
        await using (stream.ConfigureAwait(false))
        {
            await JsonSerializer.SerializeAsync(stream, document, SaveGameJsonContext.Default.GameSessionSaveDocument, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<LoadedGameSession?> LoadAsync(string slot, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        var path = GetSlotPath(slot);
        var document = await ReadDocumentAsync(path, cancellationToken).ConfigureAwait(false);
        if (document is null)
        {
            return null;
        }

        if (document.SaveVersion != CurrentSaveVersion)
        {
            LogVersionMismatch(_logger, slot, document.SaveVersion, CurrentSaveVersion);
            return null;
        }

        return LoadedGameSession.Create(
            slot,
            document.CheckpointName,
            document.CreatedUtc,
            document.LastPlayedUtc,
            document.NarrativeProgress.LastKnot,
            document.SessionSnapshot.Restore);
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
            var document = await ReadDocumentAsync(filePath, cancellationToken).ConfigureAwait(false);
            if (document is null || document.SaveVersion != CurrentSaveVersion)
            {
                continue;
            }

            slots.Add(new SaveSlotMetadata(Path.GetFileNameWithoutExtension(filePath), document.CheckpointName, document.LastPlayedUtc));
        }

        return slots
            .OrderByDescending(static slot => slot.LastPlayedUtc)
            .ToArray();
    }

    private async Task<GameSessionSaveDocument?> ReadDocumentAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var stream = OpenReadStream(path);
            await using (stream.ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync(stream, SaveGameJsonContext.Default.GameSessionSaveDocument, cancellationToken).ConfigureAwait(false);
            }
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

    private static FileStream OpenReadStream(string path)
    {
        return new FileStream(path, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            BufferSize = StreamBufferSize,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
    }

    private static FileStream OpenWriteStream(string path)
    {
        return new FileStream(path, new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            BufferSize = StreamBufferSize,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
    }

    private string GetSlotPath(string slot)
    {
        return Path.Combine(_saveDirectory, $"{slot}.json");
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
