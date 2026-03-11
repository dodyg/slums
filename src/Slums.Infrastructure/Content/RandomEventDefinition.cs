namespace Slums.Infrastructure.Content;

public sealed record RandomEventDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int MinDay { get; init; }
    public int Weight { get; init; }
    public string? ConditionId { get; init; }
    public int MoneyChange { get; init; }
    public int HealthChange { get; init; }
    public int EnergyChange { get; init; }
    public int HungerChange { get; init; }
    public int StressChange { get; init; }
    public int PolicePressureChange { get; init; }
    public int MotherHealthChange { get; init; }
    public int FoodChange { get; init; }
    public string? InkKnot { get; init; }
}