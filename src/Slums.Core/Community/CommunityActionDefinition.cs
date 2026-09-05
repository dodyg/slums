namespace Slums.Core.Community;

public sealed record CommunityActionDefinition(
    CommunityActionType Type,
    string Name,
    string Description,
    int RequiredSkillLevel,
    int TimeCostMinutes,
    int MoneyCost,
    int EnergyCost);
