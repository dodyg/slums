using Slums.Core.World;

namespace Slums.Core.Technology;

public static class TechnicalRepairRegistry
{
    private static readonly IReadOnlyList<TechnicalRepairActionDefinition> Definitions =
    [
        new(TechnicalRepairActionType.RepairHandset, "Repair Smart Handset", "Reseal the cracked handset and restore a little reliability to its repairable battery and wallet board.", LocationId.Home, 4, 90, 6, 8, 1),
        new(TechnicalRepairActionType.RestoreSolarStorage, "Restore Solar Storage", "Use salvaged cells and a patient bench session to keep the neighborhood storage bank useful through an outage.", LocationId.Workshop, 6, 120, 10, 10, 2),
        new(TechnicalRepairActionType.TakeRepairBenchContract, "Take Repair Bench Contract", "Repair a courier relay for a local cooperative. The job pays, but your own spare parts leave the shelf.", LocationId.Workshop, 8, 180, 0, 15, 2)
    ];

    public static IReadOnlyList<TechnicalRepairActionDefinition> All => Definitions;

    public static TechnicalRepairActionDefinition Get(TechnicalRepairActionType actionType)
    {
        return Definitions.First(definition => definition.Type == actionType);
    }
}
