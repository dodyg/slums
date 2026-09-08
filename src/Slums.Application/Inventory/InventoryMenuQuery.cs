using Slums.Core.Inventory;

namespace Slums.Application.Inventory;

public sealed class InventoryMenuQuery
{
    public IReadOnlyList<InventoryEntryDisplay> GetEntries(InventoryMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Quantities
            .Where(static item => item.Value > 0)
            .Select(item =>
            {
                var definition = context.Catalog.GetItem(item.Key);
                return new InventoryEntryDisplay(item.Key, definition?.Name ?? item.Key, definition?.Description ?? "Unknown item.", item.Value);
            })
            .OrderBy(static item => item.Name, StringComparer.Ordinal)
            .ToArray();
    }
}
