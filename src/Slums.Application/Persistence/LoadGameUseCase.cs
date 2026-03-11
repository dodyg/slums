namespace Slums.Application.Persistence;

public sealed class LoadGameUseCase
{
    private readonly ISaveGameStore _saveGameStore;

    public LoadGameUseCase(ISaveGameStore saveGameStore)
    {
        _saveGameStore = saveGameStore;
    }

    public Task<LoadedGameState?> ExecuteAsync(string slot, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);
        return _saveGameStore.LoadAsync(slot, cancellationToken);
    }
}