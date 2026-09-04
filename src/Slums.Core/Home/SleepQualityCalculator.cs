using System.Globalization;
using Slums.Core.Characters;

namespace Slums.Core.Home;

public static class SleepQualityCalculator
{
    public const int BaseRecovery = 30;
    public const int MinimumRecovery = 10;
    public const int OvernightBaseRecovery = 15;
    public const int OvernightMinimumRecovery = 5;

    private static readonly SleepModifierDefinition[] ModifierDefinitions =
    [
        new(static context => context.Stats.Stress > 80, -10, -5, "High stress"),
        new(static context => context.Stats.Stress > 60 && context.Stats.Stress <= 80, -5, -3, "Stress"),
        new(static context => !context.Nutrition.AteToday, -5, -3, "No meal today"),
        new(static context => context.Nutrition.DaysUndereating > 2, -5, -3, "Undereating"),
        new(static context => context.Household.MotherCondition == MotherCondition.Crisis, -5, -3, "Mother in crisis"),
        new(static context => context.UnpaidRentDays > 3, -3, -2, "Rent anxiety")
    ];

    public static int CalculateRecovery(
        SurvivalStats stats,
        NutritionState nutrition,
        HouseholdCareState household,
        int unpaidRentDays,
        HomeUpgradeState upgrades,
        int seasonRestBonus = 0)
    {
        var context = CreateContext(stats, nutrition, household, unpaidRentDays, upgrades);
        var recovery = BaseRecovery + GetAppliedModifiers(context).Sum(static modifier => modifier.RecoveryPenalty);
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
        var context = CreateContext(stats, nutrition, household, unpaidRentDays, upgrades);
        var recovery = OvernightBaseRecovery + GetAppliedModifiers(context).Sum(static modifier => modifier.OvernightPenalty);
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
        var context = CreateContext(stats, nutrition, household, unpaidRentDays, upgrades);
        var factors = new List<string> { $"Base: {BaseRecovery}" };

        foreach (var modifier in GetAppliedModifiers(context))
        {
            factors.Add($"{modifier.Label}: {FormatSigned(modifier.RecoveryPenalty)}");
        }

        var upgradeBonus = upgrades.GetEnergyRecoveryBonus();
        if (upgradeBonus > 0)
        {
            factors.Add($"Home upgrades: +{upgradeBonus}");
        }

        if (seasonRestBonus > 0)
        {
            factors.Add($"Season bonus: +{seasonRestBonus}");
        }

        factors.Add($"Recovery: {recovery}");
        return string.Join(" | ", factors);
    }

    private static SleepContext CreateContext(
        SurvivalStats stats,
        NutritionState nutrition,
        HouseholdCareState household,
        int unpaidRentDays,
        HomeUpgradeState upgrades)
    {
        ArgumentNullException.ThrowIfNull(stats);
        ArgumentNullException.ThrowIfNull(nutrition);
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(upgrades);
        return new SleepContext(stats, nutrition, household, unpaidRentDays);
    }

    private static IEnumerable<SleepModifierDefinition> GetAppliedModifiers(SleepContext context)
    {
        return ModifierDefinitions.Where(modifier => modifier.Applies(context));
    }

    private static string FormatSigned(int value)
    {
        return value.ToString("+#;-#;0", CultureInfo.InvariantCulture);
    }

    private sealed record SleepContext(
        SurvivalStats Stats,
        NutritionState Nutrition,
        HouseholdCareState Household,
        int UnpaidRentDays);

    private sealed record SleepModifierDefinition(
        Func<SleepContext, bool> Applies,
        int RecoveryPenalty,
        int OvernightPenalty,
        string Label);
}
