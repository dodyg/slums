using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionInventorySnapshot
{
    public Dictionary<string, int> Quantities { get; init; } = [];

    public static GameSessionInventorySnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new GameSessionInventorySnapshot { Quantities = gameSession.Inventory.Quantities.ToDictionary(static item => item.Key, static item => item.Value, StringComparer.Ordinal) };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        gameSession.Inventory.Restore(Quantities);
    }
}
