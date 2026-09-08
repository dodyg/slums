using Slums.Core.Content;
using Slums.Core.Inventory;
using Slums.Core.State;

namespace Slums.Application.Inventory;

public sealed record InventoryMenuContext(IReadOnlyDictionary<string, int> Quantities, GameContentCatalog Catalog)
{
    public static InventoryMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new InventoryMenuContext(gameSession.Inventory.Quantities, gameSession.ContentCatalog);
    }
}
