using Slums.Core.World;

namespace Slums.Core.Robotics;

public static class RobotRegistry
{
    private static readonly IReadOnlyList<RobotDefinition> DefaultDefinitions =
    [
        new RobotDefinition
        {
            Type = RobotType.SalvageCrawler,
            Name = "Salvage Crawler",
            Description = "A second-hand tracked crawler that can pull useful boards and actuator parts from dead infrastructure.",
            PurchaseCost = 125,
            RepairCost = 18,
            RepairCondition = 45,
            PurchaseLocationId = LocationId.Workshop
        },
        new RobotDefinition
        {
            Type = RobotType.RepairDrone,
            Name = "Repair Drone",
            Description = "A battered quad-rotor with a solder arm; its flight controller remembers every hard landing.",
            PurchaseCost = 165,
            RepairCost = 24,
            RepairCondition = 40,
            PurchaseLocationId = LocationId.Workshop
        },
        new RobotDefinition
        {
            Type = RobotType.CargoMule,
            Name = "Cargo Mule",
            Description = "A squat autonomous hauler built for alleys where delivery firms stopped sending drivers.",
            PurchaseCost = 210,
            RepairCost = 30,
            RepairCondition = 35,
            PurchaseLocationId = LocationId.Workshop
        }
    ];

    private static IReadOnlyList<RobotDefinition> _definitions = DefaultDefinitions;

    public const int MaxOwnedRobots = 3;
    public const int PartsPurchaseCost = 8;
    public const int MaxParts = 20;

    public static IReadOnlyList<RobotDefinition> AllDefinitions => _definitions;

    public static void Configure(IEnumerable<RobotDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var configuredDefinitions = definitions.Where(static definition => definition is not null).ToArray();
        if (configuredDefinitions.Length == 0)
        {
            throw new InvalidOperationException("At least one robot definition must be configured.");
        }

        _definitions = configuredDefinitions;
    }

    public static RobotDefinition GetByType(RobotType type)
    {
        return _definitions.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No robot definition configured for {type}.");
    }
}
