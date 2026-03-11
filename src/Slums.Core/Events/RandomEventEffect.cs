namespace Slums.Core.Events;

public sealed record RandomEventEffect
{
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