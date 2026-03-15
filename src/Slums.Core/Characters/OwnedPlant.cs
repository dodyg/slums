namespace Slums.Core.Characters;

public sealed class OwnedPlant
{
    public Guid Id { get; init; }

    public PlantType Type { get; init; }

    public int AcquiredOnDay { get; init; }

    public int LastBaseCarePaidWeek { get; private set; }

    public int LastHarvestDay { get; private set; }

    public bool HasBiggerPot { get; private set; }

    public bool HasWindowPlacement { get; private set; }

    public int FertilizerPaidWeek { get; private set; }

    public int IrrigationPaidWeek { get; private set; }

    public bool IsBaseCarePaidForWeek(int currentWeek)
    {
        return LastBaseCarePaidWeek >= currentWeek;
    }

    public void PayBaseCare(int currentWeek)
    {
        LastBaseCarePaidWeek = Math.Max(LastBaseCarePaidWeek, currentWeek);
    }

    public bool HasActiveUpgrade(PlantUpgradeType upgradeType, int currentWeek)
    {
        return upgradeType switch
        {
            PlantUpgradeType.BiggerPot => HasBiggerPot,
            PlantUpgradeType.WindowPlacement => HasWindowPlacement,
            PlantUpgradeType.Fertilizer => FertilizerPaidWeek >= currentWeek,
            PlantUpgradeType.Irrigation => IrrigationPaidWeek >= currentWeek,
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public bool CanPurchaseUpgrade(PlantUpgradeType upgradeType, int currentWeek)
    {
        return !HasActiveUpgrade(upgradeType, currentWeek);
    }

    public void PurchaseUpgrade(PlantUpgradeType upgradeType, int currentWeek)
    {
        switch (upgradeType)
        {
            case PlantUpgradeType.BiggerPot:
                HasBiggerPot = true;
                break;
            case PlantUpgradeType.WindowPlacement:
                HasWindowPlacement = true;
                break;
            case PlantUpgradeType.Fertilizer:
                FertilizerPaidWeek = Math.Max(FertilizerPaidWeek, currentWeek);
                break;
            case PlantUpgradeType.Irrigation:
                IrrigationPaidWeek = Math.Max(IrrigationPaidWeek, currentWeek);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null);
        }
    }

    public int GetActiveUpgradeCount(int currentWeek)
    {
        var count = 0;
        if (HasBiggerPot)
        {
            count++;
        }

        if (HasWindowPlacement)
        {
            count++;
        }

        if (FertilizerPaidWeek >= currentWeek)
        {
            count++;
        }

        if (IrrigationPaidWeek >= currentWeek)
        {
            count++;
        }

        return count;
    }

    public bool TryResolveHarvest(int currentDay, int currentWeek, PlantDefinition definition, out int income)
    {
        ArgumentNullException.ThrowIfNull(definition);

        income = 0;
        if (!definition.IsSellable || definition.HarvestCycleDays <= 0)
        {
            return false;
        }

        if (currentDay - LastHarvestDay < definition.HarvestCycleDays)
        {
            return false;
        }

        income = definition.HarvestSalePrice + (definition.HarvestPriceBonusPerUpgrade * GetActiveUpgradeCount(currentWeek));
        LastHarvestDay = currentDay;
        return income > 0;
    }

    public static OwnedPlant Create(PlantType type, int currentDay, int currentWeek)
    {
        return new OwnedPlant
        {
            Id = Guid.NewGuid(),
            Type = type,
            AcquiredOnDay = currentDay,
            LastBaseCarePaidWeek = currentWeek,
            LastHarvestDay = currentDay
        };
    }

    public static OwnedPlant Restore(
        Guid id,
        PlantType type,
        int acquiredOnDay,
        int lastBaseCarePaidWeek,
        int lastHarvestDay,
        bool hasBiggerPot,
        bool hasWindowPlacement,
        int fertilizerPaidWeek,
        int irrigationPaidWeek)
    {
        return new OwnedPlant
        {
            Id = id,
            Type = type,
            AcquiredOnDay = acquiredOnDay,
            LastBaseCarePaidWeek = lastBaseCarePaidWeek,
            LastHarvestDay = lastHarvestDay,
            HasBiggerPot = hasBiggerPot,
            HasWindowPlacement = hasWindowPlacement,
            FertilizerPaidWeek = fertilizerPaidWeek,
            IrrigationPaidWeek = irrigationPaidWeek
        };
    }
}
