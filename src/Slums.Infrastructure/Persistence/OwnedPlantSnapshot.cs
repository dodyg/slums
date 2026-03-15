using Slums.Core.Characters;

namespace Slums.Infrastructure.Persistence;

public sealed record OwnedPlantSnapshot(
    Guid Id,
    PlantType Type,
    int AcquiredOnDay,
    int LastBaseCarePaidWeek,
    int LastHarvestDay,
    bool HasBiggerPot,
    bool HasWindowPlacement,
    int FertilizerPaidWeek,
    int IrrigationPaidWeek)
{
    public static OwnedPlantSnapshot Capture(OwnedPlant plant)
    {
        ArgumentNullException.ThrowIfNull(plant);
        return new OwnedPlantSnapshot(
            plant.Id,
            plant.Type,
            plant.AcquiredOnDay,
            plant.LastBaseCarePaidWeek,
            plant.LastHarvestDay,
            plant.HasActiveUpgrade(PlantUpgradeType.BiggerPot, int.MaxValue),
            plant.HasActiveUpgrade(PlantUpgradeType.WindowPlacement, int.MaxValue),
            plant.FertilizerPaidWeek,
            plant.IrrigationPaidWeek);
    }

    public OwnedPlant Restore()
    {
        return OwnedPlant.Restore(
            Id,
            Type,
            AcquiredOnDay,
            LastBaseCarePaidWeek,
            LastHarvestDay,
            HasBiggerPot,
            HasWindowPlacement,
            FertilizerPaidWeek,
            IrrigationPaidWeek);
    }
}
