namespace Slums.Core.Characters;

public sealed class OwnedPet
{
    public PetType Type { get; init; }

    public int AcquiredOnDay { get; init; }

    public int LastUpkeepPaidWeek { get; private set; }

    public bool HasBetterFilter { get; private set; }

    public bool HasHeater { get; private set; }

    public int DecorationsPaidWeek { get; private set; }

    public int WaterConditionerPaidWeek { get; private set; }

    public bool IsUpkeepPaidForWeek(int currentWeek)
    {
        return LastUpkeepPaidWeek >= currentWeek;
    }

    public void PayUpkeep(int currentWeek)
    {
        LastUpkeepPaidWeek = Math.Max(LastUpkeepPaidWeek, currentWeek);
    }

    public bool HasActiveUpgrade(FishTankUpgradeType upgradeType, int currentWeek)
    {
        return upgradeType switch
        {
            FishTankUpgradeType.BetterFilter => HasBetterFilter,
            FishTankUpgradeType.Heater => HasHeater,
            FishTankUpgradeType.Decorations => DecorationsPaidWeek >= currentWeek,
            FishTankUpgradeType.WaterConditioner => WaterConditionerPaidWeek >= currentWeek,
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public bool CanPurchaseUpgrade(FishTankUpgradeType upgradeType, int currentWeek)
    {
        return !HasActiveUpgrade(upgradeType, currentWeek);
    }

    public void PurchaseUpgrade(FishTankUpgradeType upgradeType, int currentWeek)
    {
        switch (upgradeType)
        {
            case FishTankUpgradeType.BetterFilter:
                HasBetterFilter = true;
                break;
            case FishTankUpgradeType.Heater:
                HasHeater = true;
                break;
            case FishTankUpgradeType.Decorations:
                DecorationsPaidWeek = Math.Max(DecorationsPaidWeek, currentWeek);
                break;
            case FishTankUpgradeType.WaterConditioner:
                WaterConditionerPaidWeek = Math.Max(WaterConditionerPaidWeek, currentWeek);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null);
        }
    }

    public int GetActiveUpgradeCount(int currentWeek)
    {
        var count = 0;
        if (HasBetterFilter)
        {
            count++;
        }

        if (HasHeater)
        {
            count++;
        }

        if (DecorationsPaidWeek >= currentWeek)
        {
            count++;
        }

        if (WaterConditionerPaidWeek >= currentWeek)
        {
            count++;
        }

        return count;
    }

    public static OwnedPet Create(PetType type, int currentDay, int currentWeek)
    {
        return new OwnedPet
        {
            Type = type,
            AcquiredOnDay = currentDay,
            LastUpkeepPaidWeek = currentWeek
        };
    }

    public static OwnedPet Restore(
        PetType type,
        int acquiredOnDay,
        int lastUpkeepPaidWeek,
        bool hasBetterFilter,
        bool hasHeater,
        int decorationsPaidWeek,
        int waterConditionerPaidWeek)
    {
        return new OwnedPet
        {
            Type = type,
            AcquiredOnDay = acquiredOnDay,
            LastUpkeepPaidWeek = lastUpkeepPaidWeek,
            HasBetterFilter = hasBetterFilter,
            HasHeater = hasHeater,
            DecorationsPaidWeek = decorationsPaidWeek,
            WaterConditionerPaidWeek = waterConditionerPaidWeek
        };
    }
}
