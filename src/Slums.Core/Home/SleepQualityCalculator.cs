using Slums.Core.Characters;

namespace Slums.Core.Home;

public static class SleepQualityCalculator
{
    public const int BaseRecovery = 30;
    public const int MinimumRecovery = 10;
    public const int OvernightBaseRecovery = 15;
    public const int OvernightMinimumRecovery = 5;

    public static int CalculateRecovery(
        SurvivalStats stats,
        NutritionState nutrition,
        HouseholdCareState household,
        int unpaidRentDays,
        HomeUpgradeState upgrades,
        int seasonRestBonus = 0)
    {
        ArgumentNullException.ThrowIfNull(stats);
        ArgumentNullException.ThrowIfNull(nutrition);
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(upgrades);

        var recovery = BaseRecovery;

        if (stats.Stress > 80)
        {
            recovery -= 10;
        }
        else if (stats.Stress > 60)
        {
            recovery -= 5;
        }

        if (!nutrition.AteToday)
        {
            recovery -= 5;
        }

        if (nutrition.DaysUndereating > 2)
        {
            recovery -= 5;
        }

        if (household.MotherCondition == MotherCondition.Crisis)
        {
            recovery -= 5;
        }

        if (unpaidRentDays > 3)
        {
            recovery -= 3;
        }

        recovery += upgrades.GetEnergyRecoveryBonus();
        recovery += seasonRestBonus;

        return Math.Max(MinimumRecovery, recovery);
    }

    public static int CalculateOvernightRecovery(
        SurvivalStats stats,
        NutritionState nutrition,
        HouseholdCareState household,
        int unpaidRentDays,
        HomeUpgradeState upgrades,
        int seasonRestBonus = 0)
    {
        ArgumentNullException.ThrowIfNull(stats);
        ArgumentNullException.ThrowIfNull(nutrition);
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(upgrades);

        var recovery = OvernightBaseRecovery;

        if (stats.Stress > 80)
        {
            recovery -= 5;
        }
        else if (stats.Stress > 60)
        {
            recovery -= 3;
        }

        if (!nutrition.AteToday)
        {
            recovery -= 3;
        }

        if (nutrition.DaysUndereating > 2)
        {
            recovery -= 3;
        }

        if (household.MotherCondition == MotherCondition.Crisis)
        {
            recovery -= 3;
        }

        if (unpaidRentDays > 3)
        {
            recovery -= 2;
        }

        recovery += upgrades.GetEnergyRecoveryBonus() / 2;
        recovery += seasonRestBonus;

        return Math.Max(OvernightMinimumRecovery, recovery);
    }

    public static string BuildRecoveryBreakdown(
        int recovery,
        SurvivalStats stats,
        NutritionState nutrition,
        HouseholdCareState household,
        int unpaidRentDays,
        HomeUpgradeState upgrades,
        int seasonRestBonus = 0)
    {
        ArgumentNullException.ThrowIfNull(stats);
        ArgumentNullException.ThrowIfNull(nutrition);
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(upgrades);

        var factors = new List<string> { $"Base: {BaseRecovery}" };

        if (stats.Stress > 80)
        {
            factors.Add("High stress: -10");
        }
        else if (stats.Stress > 60)
        {
            factors.Add("Stress: -5");
        }

        if (!nutrition.AteToday)
        {
            factors.Add("No meal today: -5");
        }

        if (nutrition.DaysUndereating > 2)
        {
            factors.Add("Undereating: -5");
        }

        if (household.MotherCondition == MotherCondition.Crisis)
        {
            factors.Add("Mother in crisis: -5");
        }

        if (unpaidRentDays > 3)
        {
            factors.Add("Rent anxiety: -3");
        }

        if (upgrades.GetEnergyRecoveryBonus() > 0)
        {
            factors.Add($"Home upgrades: +{upgrades.GetEnergyRecoveryBonus()}");
        }

        if (seasonRestBonus > 0)
        {
            factors.Add($"Season bonus: +{seasonRestBonus}");
        }

        factors.Add($"Recovery: {recovery}");
        return string.Join(" | ", factors);
    }
}
