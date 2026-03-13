using Slums.Core.State;

namespace Slums.Application.Persistence;

public sealed class LoadedGameSession : IDisposable
{
    private GameSession? _gameSession;
    private bool _disposed;

    public LoadedGameSession(
        string slot,
        string checkpointName,
        DateTimeOffset createdUtc,
        DateTimeOffset lastPlayedUtc,
        string? lastKnot,
        GameSession gameSession)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpointName);
        ArgumentNullException.ThrowIfNull(gameSession);

        Slot = slot;
        CheckpointName = checkpointName;
        CreatedUtc = createdUtc;
        LastPlayedUtc = lastPlayedUtc;
        LastKnot = lastKnot;
        _gameSession = gameSession;
    }

    public static LoadedGameSession Create(
        string slot,
        string checkpointName,
        DateTimeOffset createdUtc,
        DateTimeOffset lastPlayedUtc,
        string? lastKnot,
        Func<GameSession> gameSessionFactory)
    {
        ArgumentNullException.ThrowIfNull(gameSessionFactory);

        GameSession? gameSession = gameSessionFactory();
        ArgumentNullException.ThrowIfNull(gameSession);

        try
        {
            var loadedGameSession = new LoadedGameSession(slot, checkpointName, createdUtc, lastPlayedUtc, lastKnot, gameSession);
            gameSession = null;
            return loadedGameSession;
        }
        finally
        {
            gameSession?.Dispose();
        }
    }

    public string Slot { get; }

    public string CheckpointName { get; }

    public DateTimeOffset CreatedUtc { get; }

    public DateTimeOffset LastPlayedUtc { get; }

    public string? LastKnot { get; }

    public GameSession GameSession
    {
        get
        {
            ThrowIfDisposed();
            return _gameSession ?? throw new InvalidOperationException("Game session ownership has already been transferred.");
        }
    }

    public GameSession TakeGameSession()
    {
        ThrowIfDisposed();

        if (_gameSession is null)
        {
            throw new InvalidOperationException("Game session ownership has already been transferred.");
        }

        var gameSession = _gameSession;
        _gameSession = null;
        return gameSession;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _gameSession?.Dispose();
        _gameSession = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
