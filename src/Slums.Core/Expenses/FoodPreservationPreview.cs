namespace Slums.Core.Expenses;

/// <summary>Describes the current bounded home food-preservation opportunity.</summary>
public sealed record FoodPreservationPreview(
    int RequiredSkillLevel,
    int InputFoodUnits,
    int OutputMealUnits,
    int TimeCostMinutes,
    int EnergyCost,
    int CurrentFoodStockpile,
    int CurrentPreservedMealUnits,
    bool AtHome,
    bool HasSkill,
    bool HasFood,
    bool HasTime,
    bool HasEnergy,
    bool CanPerform,
    string? UnavailabilityReason);
