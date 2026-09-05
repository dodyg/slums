namespace Slums.Core.Technology;

public sealed record DigitalServicePreview(
    DigitalServiceActionDefinition Action,
    bool AtRequiredLocation,
    bool HasSkill,
    bool HasOperationalPhone,
    bool HasTime,
    bool CanAfford,
    bool HasEnergy,
    bool NoPendingAppeal,
    int SuccessChance,
    bool CreatesObligation,
    bool CanPerform,
    string? UnavailabilityReason);
