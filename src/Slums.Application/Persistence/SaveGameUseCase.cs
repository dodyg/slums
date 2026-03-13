namespace Slums.Application.Persistence;

public sealed class SaveGameUseCase
{
    private readonly ISaveGameStore _saveGameStore;

    public SaveGameUseCase(ISaveGameStore saveGameStore)
    {
        _saveGameStore = saveGameStore;
    }

    public Task ExecuteAsync(SaveGameRequest request, string slot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        return _saveGameStore.SaveAsync(request, slot, cancellationToken);
    }
}
