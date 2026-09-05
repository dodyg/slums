namespace Slums.Core.Technology;

public sealed record TechnicalRepairPreview(
    TechnicalRepairActionDefinition Action,
    bool AtRequiredLocation,
    bool HasSkill,
    bool HasTime,
    bool CanAfford,
    bool HasEnergy,
    bool HasParts,
    bool NeedsRepair,
    int CurrentCondition,
    int ConditionGain,
    int Income,
    bool CanPerform,
    string? UnavailabilityReason);
