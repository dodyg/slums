using Slums.Application.Narrative;
using Slums.Core.State;

namespace Slums.Application.Persistence;

public sealed class SaveGameUseCase
{
    private readonly ISaveGameStore _saveGameStore;

    public SaveGameUseCase(ISaveGameStore saveGameStore)
    {
        _saveGameStore = saveGameStore;
    }

    public Task ExecuteAsync(GameState gameState, INarrativeService narrativeService, string slot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(narrativeService);
        ArgumentException.ThrowIfNullOrWhiteSpace(slot);

        return _saveGameStore.SaveAsync(gameState, narrativeService, slot, cancellationToken);
    }
}