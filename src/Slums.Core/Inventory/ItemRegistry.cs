namespace Slums.Core.Inventory;

public static class ItemRegistry
{
    private static IReadOnlyList<ItemDefinition> _definitions = [];

    public static IReadOnlyList<ItemDefinition> All => _definitions;

    public static void Configure(IEnumerable<ItemDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = definitions.ToArray();
    }

    public static ItemDefinition? GetById(string id) => _definitions.FirstOrDefault(item => item.Id == id);
}
