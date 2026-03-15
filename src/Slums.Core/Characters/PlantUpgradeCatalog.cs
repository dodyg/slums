namespace Slums.Core.Characters;

public static class PlantUpgradeCatalog
{
    public static int GetCost(PlantUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            PlantUpgradeType.BiggerPot => 15,
            PlantUpgradeType.WindowPlacement => 20,
            PlantUpgradeType.Fertilizer => 18,
            PlantUpgradeType.Irrigation => 25,
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public static string GetName(PlantUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            PlantUpgradeType.BiggerPot => "Bigger Pot",
            PlantUpgradeType.WindowPlacement => "Window Placement / Sunlamp",
            PlantUpgradeType.Fertilizer => "Fertilizer / Soil Amendments",
            PlantUpgradeType.Irrigation => "Irrigation / Drip System",
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }
}
