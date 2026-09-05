namespace Slums.Core.Community;

public static class CommunityActionRegistry
{
    private static readonly IReadOnlyList<CommunityActionDefinition> Definitions =
    [
        new(CommunityActionType.CoordinateCoolingRoom, "Coordinate Cooling Room", "Contribute money and time to keep a shared shaded room open through the hot nights.", 4, 120, 8, 8),
        new(CommunityActionType.OrganizeWaterRationing, "Organize Water Rationing", "Use one food staple and a small contribution to coordinate rooftop water access.", 4, 150, 5, 10),
        new(CommunityActionType.NeighborhoodPressureResponse, "Neighborhood Pressure Response", "Coordinate a guarded neighborhood response when heat or territory pressure rises.", 8, 180, 10, 12)
    ];

    public static IReadOnlyList<CommunityActionDefinition> All => Definitions;

    public static CommunityActionDefinition Get(CommunityActionType actionType)
    {
        return Definitions.First(definition => definition.Type == actionType);
    }
}
