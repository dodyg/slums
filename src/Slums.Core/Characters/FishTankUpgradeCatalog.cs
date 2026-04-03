namespace Slums.Core.Characters;

public static class FishTankUpgradeCatalog
{
    public static int GetCost(FishTankUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            FishTankUpgradeType.BetterFilter => 15,
            FishTankUpgradeType.Heater => 20,
            FishTankUpgradeType.Decorations => 12,
            FishTankUpgradeType.WaterConditioner => 18,
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public static string GetName(FishTankUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            FishTankUpgradeType.BetterFilter => "Better Filter",
            FishTankUpgradeType.Heater => "Heater",
            FishTankUpgradeType.Decorations => "Decorations",
            FishTankUpgradeType.WaterConditioner => "Water Conditioner",
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public static int GetMotherHealthBonusPerUpgrade => 1;
}
