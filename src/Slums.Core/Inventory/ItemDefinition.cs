namespace Slums.Core.Inventory;

public sealed record ItemDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int MaximumQuantity { get; init; } = 10;
    public bool Perishable { get; init; }
}
