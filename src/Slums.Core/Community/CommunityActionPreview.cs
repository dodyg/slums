namespace Slums.Core.Community;

public sealed record CommunityActionPreview(
    CommunityActionDefinition Action,
    bool IsAtHome,
    bool HasSkill,
    bool HasTime,
    bool CanAfford,
    bool HasEnergy,
    bool HasSupplies,
    bool HasPressureNeed,
    bool HasCommunityParticipation,
    bool CanPerform,
    string? UnavailabilityReason);
