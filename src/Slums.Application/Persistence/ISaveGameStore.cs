using Slums.Application.Narrative;
using Slums.Core.State;

namespace Slums.Application.Persistence;

public interface ISaveGameStore
{
    public Task SaveAsync(GameState gameState, INarrativeService narrativeService, string slot, CancellationToken cancellationToken = default);

    public Task<LoadedGameState?> LoadAsync(string slot, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<SaveSlotMetadata>> ListSlotsAsync(CancellationToken cancellationToken = default);
}