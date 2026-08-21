using Slums.Core.World;

namespace Slums.Core.Robotics;

public sealed record RobotDefinition
{
    public RobotType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int PurchaseCost { get; init; }

    public int RepairCost { get; init; }

    public int RepairCondition { get; init; } = 40;

    public LocationId PurchaseLocationId { get; init; } = LocationId.Workshop;
}
