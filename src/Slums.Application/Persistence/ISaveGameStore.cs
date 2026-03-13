namespace Slums.Application.Persistence;

public interface ISaveGameStore
{
    public Task SaveAsync(SaveGameRequest request, string slot, CancellationToken cancellationToken = default);

    public Task<LoadedGameSession?> LoadAsync(string slot, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<SaveSlotMetadata>> ListSlotsAsync(CancellationToken cancellationToken = default);
}
