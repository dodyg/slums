using Slums.Core.Inventory;

namespace Slums.Application.Inventory;

public sealed record InventoryEntryDisplay(string Id, string Name, string Description, int Quantity);

public sealed class InventoryMenuQuery
{
    #pragma warning disable CA1822
    public IReadOnlyList<InventoryEntryDisplay> GetEntries(InventoryMenuContext context)
    #pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Quantities
            .Where(static item => item.Value > 0)
            .Select(item =>
            {
                var definition = ItemRegistry.GetById(item.Key);
                return new InventoryEntryDisplay(item.Key, definition?.Name ?? item.Key, definition?.Description ?? "Unknown item.", item.Value);
            })
            .OrderBy(static item => item.Name, StringComparer.Ordinal)
            .ToArray();
    }
}
