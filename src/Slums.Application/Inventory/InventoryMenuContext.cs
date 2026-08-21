using Slums.Core.State;

namespace Slums.Application.Inventory;

public sealed record InventoryMenuContext(IReadOnlyDictionary<string, int> Quantities)
{
    public static InventoryMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new InventoryMenuContext(gameSession.Inventory.Quantities);
    }
}
