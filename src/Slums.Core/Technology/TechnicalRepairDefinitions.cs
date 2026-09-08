using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Core.Technology;

/// <summary>Code-owned default technical repair definitions used when a catalog is built without JSON content.</summary>
public static class TechnicalRepairDefinitions
{
    /// <summary>Gets the default technical repair action definitions.</summary>
    public static IReadOnlyList<TechnicalRepairActionDefinition> Defaults { get; } =
    [
        new(TechnicalRepairActionType.RepairHandset, "Repair Smart Handset", "Reseal the cracked handset and restore a little reliability to its repairable battery and wallet board.", LocationId.Home, SkillThresholds.AdvancedLevel, 90, 6, 8, 1),
        new(TechnicalRepairActionType.RestoreSolarStorage, "Restore Solar Storage", "Use salvaged cells and a patient bench session to keep the neighborhood storage bank useful through an outage.", LocationId.Workshop, SkillThresholds.HighLevel, 120, 10, 10, 2),
        new(TechnicalRepairActionType.RestoreWaterPump, "Restore Water Pump", "Rebuild a worn rooftop pump relay so the block can ration water through the next interruption.", LocationId.Workshop, SkillThresholds.HighLevel, 120, 8, 10, 2),
        new(TechnicalRepairActionType.TakeRepairBenchContract, "Take Repair Bench Contract", "Repair a courier relay for a local cooperative. The job pays, but your own spare parts leave the shelf.", LocationId.Workshop, SkillThresholds.MasteryLevel, 180, 0, 15, 2)
    ];
}
