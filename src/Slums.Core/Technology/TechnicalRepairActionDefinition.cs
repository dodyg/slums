using Slums.Core.World;

namespace Slums.Core.Technology;

public sealed record TechnicalRepairActionDefinition(
    TechnicalRepairActionType Type,
    string Name,
    string Description,
    LocationId RequiredLocation,
    int RequiredSkillLevel,
    int TimeCostMinutes,
    int MoneyCost,
    int EnergyCost,
    int PartsRequired);
