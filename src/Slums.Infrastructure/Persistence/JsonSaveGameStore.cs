using System.Text.Json;
using Microsoft.Extensions.Logging;
using Slums.Application.Persistence;

namespace Slums.Infrastructure.Persistence;

/// <summary>
/// Stores saves as JSON files. Slots are restricted to a safe identifier format, writes are
/// atomic (temporary file + replace, retaining a backup), and loads return typed results so
/// missing, corrupt, and incompatible saves are distinguishable.
/// </summary>
public sealed class JsonSaveGameStore : ISaveGameStore
{
    /// <summary>
    /// Save compatibility policy: no migrations exist yet, so a save must carry exactly the
    /// current version to load. Older or newer saves are reported as incompatible.
    /// </summary>
    private const int CurrentSaveVersion = 2;

    private const int StreamBufferSize = 4096;
    private readonly ILogger<JsonSaveGameStore> _logger;
    private readonly string _saveDirectory;

    public JsonSaveGameStore(ILogger<JsonSaveGameStore> logger, string? saveDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _saveDirectory = saveDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Slums", "saves");
    }

    public async Task SaveAsync(SaveGameRequest request, string slot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        SaveSlotRules.EnsureValidSlot(slot);

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

        await WriteAtomicAsync(path, document, cancellationToken).ConfigureAwait(false);

        LogSaveCompleted(_logger, slot);
    }

    public async Task<LoadGameResult> LoadAsync(string slot, CancellationToken cancellationToken = default)
    {
        SaveSlotRules.EnsureValidSlot(slot);

        var path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            return LoadGameResult.Missing();
        }

        GameSessionSaveDocument? document;
        try
        {
            document = await ReadDocumentAsync(path, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            LogSaveReadJsonFailure(_logger, path, exception);
            return LoadGameResult.Corrupt($"Save file is not valid JSON: {exception.Message}");
        }
        catch (IOException exception)
        {
            LogSaveReadIoFailure(_logger, path, exception);
            return LoadGameResult.Corrupt($"Save file could not be read: {exception.Message}");
        }

        if (document is null)
        {
            return LoadGameResult.Corrupt("Save file is empty or could not be deserialized.");
        }

        if (document.SaveVersion != CurrentSaveVersion)
        {
            LogVersionMismatch(_logger, slot, document.SaveVersion, CurrentSaveVersion);
            return LoadGameResult.Incompatible(document.SaveVersion, CurrentSaveVersion);
        }

        try
        {
            SaveGameValidator.Validate(document.SessionSnapshot);
        }
        catch (InvalidDataException exception)
        {
            LogInvalidSaveData(_logger, path, exception);
            return LoadGameResult.Corrupt(exception.Message);
        }

        // Ownership of the session transfers to the caller through LoadGameResult.
#pragma warning disable CA2000
        var loadedSession = LoadedGameSession.Create(
            slot,
            document.CheckpointName,
            document.CreatedUtc,
            document.LastPlayedUtc,
            document.NarrativeProgress.LastKnot,
            document.SessionSnapshot.Restore);
#pragma warning restore CA2000
        return LoadGameResult.Loaded(loadedSession);
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
            GameSessionSaveDocument? document;
            try
            {
                document = await ReadDocumentAsync(filePath, cancellationToken).ConfigureAwait(false);
            }
            catch (JsonException exception)
            {
                LogSaveReadJsonFailure(_logger, filePath, exception);
                continue;
            }
            catch (IOException exception)
            {
                LogSaveReadIoFailure(_logger, filePath, exception);
                continue;
            }

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

    private static async Task<GameSessionSaveDocument?> ReadDocumentAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var stream = OpenReadStream(path);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer.DeserializeAsync(stream, SaveGameJsonContext.Default.GameSessionSaveDocument, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteAtomicAsync(string path, GameSessionSaveDocument document, CancellationToken cancellationToken)
    {
        var temporaryPath = path + ".tmp";
        try
        {
            var stream = OpenWriteStream(temporaryPath);
            await using (stream.ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync(stream, document, SaveGameJsonContext.Default.GameSessionSaveDocument, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(path))
            {
                // Atomically replace the existing save, retaining the previous version as a backup.
                var backupPath = path + ".bak";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                File.Replace(temporaryPath, path, backupPath);
            }
            else
            {
                File.Move(temporaryPath, path);
            }
        }
        catch
        {
            TryDelete(temporaryPath);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup of the temporary file; the original save is untouched.
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

    private static readonly Action<ILogger, string, Exception?> LogInvalidSaveDataDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(5, "InvalidSaveData"), "Rejecting save file {Path} because it failed validation.");

    private static readonly Action<ILogger, string, Exception?> LogSaveCompletedDelegate =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4, "SaveCompleted"), "Save completed for slot {Slot}.");

    private static void LogVersionMismatch(ILogger logger, string slot, int foundVersion, int expectedVersion) =>
        LogVersionMismatchDelegate(logger, slot, foundVersion, expectedVersion, null);

    private static void LogSaveReadJsonFailure(ILogger logger, string path, Exception exception) =>
        LogSaveReadJsonFailureDelegate(logger, path, exception);

    private static void LogSaveReadIoFailure(ILogger logger, string path, Exception exception) =>
        LogSaveReadIoFailureDelegate(logger, path, exception);

    private static void LogInvalidSaveData(ILogger logger, string path, Exception exception) =>
        LogInvalidSaveDataDelegate(logger, path, exception);

    private static void LogSaveCompleted(ILogger logger, string slot) =>
        LogSaveCompletedDelegate(logger, slot, null);
}
