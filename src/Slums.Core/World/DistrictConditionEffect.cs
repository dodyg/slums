namespace Slums.Core.World;

public sealed record DistrictConditionEffect
{
    public int TravelCostModifier { get; init; }

    public int TravelTimeMinutesModifier { get; init; }

    public int TravelEnergyModifier { get; init; }

    public int FoodCostModifier { get; init; }

    public int StreetFoodCostModifier { get; init; }

    public int MedicineCostModifier { get; init; }

    public int ClinicVisitCostModifier { get; init; }

    public int WorkPayModifier { get; init; }

    public int WorkStressModifier { get; init; }

    public int CrimeDetectionRiskModifier { get; init; }

    public int CrimeRewardModifier { get; init; }

    public IReadOnlyList<string> BoostedRandomEventIds { get; init; } = [];

    public IReadOnlyList<string> SuppressedRandomEventIds { get; init; } = [];
}
