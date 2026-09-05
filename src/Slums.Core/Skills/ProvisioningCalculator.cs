using Slums.Core.Characters;

namespace Slums.Core.Skills;

/// <summary>Calculates bounded food and care benefits from Provisioning.</summary>
public static class ProvisioningCalculator
{
    public static int GetFoodBundleUnits(int skillLevel)
    {
        ValidateLevel(skillLevel);
        return skillLevel >= SkillThresholds.FirstMeaningfulLevel ? 4 : 3;
    }

    public static int GetFoodPriceReduction(int skillLevel, int foodPriceShock)
    {
        ValidateLevel(skillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(foodPriceShock);
        return skillLevel >= SkillThresholds.HighLevel && foodPriceShock >= 3 ? 1 : 0;
    }

    public static ProvisioningMealPlan GetMealPlan(int skillLevel, int cookingBonus)
    {
        ValidateLevel(skillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(cookingBonus);

        var usesHerb = cookingBonus > 0 && skillLevel >= SkillThresholds.AdvancedLevel;
        var quality = usesHerb ? MealQuality.HotMeal : MealQuality.Basic;
        var stressReduction = Math.Min(3, cookingBonus);
        if (skillLevel >= SkillThresholds.MasteryLevel && usesHerb)
        {
            stressReduction++;
        }

        return new ProvisioningMealPlan(quality, 1, stressReduction, usesHerb);
    }

    public static int GetMotherCareMealBonus(int skillLevel, MealQuality mealQuality)
    {
        ValidateLevel(skillLevel);
        return skillLevel >= SkillThresholds.MasteryLevel && mealQuality == MealQuality.HotMeal ? 1 : 0;
    }

    private static void ValidateLevel(int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(skillLevel, SkillThresholds.MaximumLevel);
    }
}

/// <summary>The shared result used by food previews and meal commitment.</summary>
public sealed record ProvisioningMealPlan(
    MealQuality Quality,
    int FoodUnitsRequired,
    int StressReduction,
    bool UsesHouseholdHerb);
