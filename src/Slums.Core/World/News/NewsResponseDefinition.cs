namespace Slums.Core.World.News;

public sealed record NewsResponseDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public NewsResponseType Type { get; init; }
    public int MoneyCost { get; init; }
    public int TimeCostMinutes { get; init; }
    public string? RequiredItemId { get; init; }
    public int RequiredItemQuantity { get; init; }
    public int TrustChange { get; init; }
    public string? OutcomeMessage { get; init; }
}
