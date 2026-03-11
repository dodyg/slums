namespace Slums.Core.Characters;

public sealed record NutritionDecayResult(
    int EnergyDelta,
    int HealthDelta,
    int StressDelta);