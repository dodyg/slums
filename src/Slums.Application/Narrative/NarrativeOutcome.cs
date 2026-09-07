namespace Slums.Application.Narrative;

public sealed record NarrativeOutcome
{
    public int MoneyChange { get; init; }
    public int HealthChange { get; init; }
    public int EnergyChange { get; init; }
    public int HungerChange { get; init; }
    public int StressChange { get; init; }
    public int MotherHealthChange { get; init; }
    public int FoodChange { get; init; }
    /// <summary>All story flags emitted by the scene, in authored order.</summary>
    public IReadOnlyList<string> SetFlags { get; init; } = [];

    /// <summary>Compatibility accessor for callers that only expect one flag.</summary>
    public string? SetFlag { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>NPC/faction-targeted effects in the order they were produced.</summary>
    public IReadOnlyList<NarrativeEffect> Effects { get; init; } = [];
}
