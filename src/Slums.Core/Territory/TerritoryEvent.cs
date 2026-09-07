namespace Slums.Core.Territory;

public sealed record TerritoryEvent
{
    public TerritoryEventType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public int StressModifier { get; init; }
    public int HealthModifier { get; init; }
    public int MoneyModifier { get; init; }
    public int TensionModifier { get; init; }
    public int InfluenceModifier { get; init; }
    public bool BlocksMarket { get; init; }
    public bool BlocksCrime { get; init; }
    public int DurationDays { get; init; }
    public string? Narration { get; init; }
}
