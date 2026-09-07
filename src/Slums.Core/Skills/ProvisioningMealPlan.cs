using Slums.Core.Characters;

namespace Slums.Core.Skills;

/// <summary>The shared result used by food previews and meal commitment.</summary>
public sealed record ProvisioningMealPlan(
    MealQuality Quality,
    int FoodUnitsRequired,
    int StressReduction,
    bool UsesHouseholdHerb,
    bool UsesPreservedFood = false);
