namespace Slums.Core.Home;

public static class HomeUpgradeDefinitions
{
    public const int WindowScreenStressReduction = 2;
    public const int CurtainPrivacyBonus = 1;

    public static int GetCost(HomeUpgrade upgrade) => upgrade switch
    {
        HomeUpgrade.CleanBedding => 25,
        HomeUpgrade.Fan => 40,
        HomeUpgrade.WindowScreen => 20,
        HomeUpgrade.Curtain => 15,
        _ => throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, null)
    };

    public static int GetEnergyBonus(HomeUpgrade upgrade) => upgrade switch
    {
        HomeUpgrade.CleanBedding => 2,
        HomeUpgrade.WindowScreen => 1,
        HomeUpgrade.Curtain => 1,
        _ => 0
    };

    public static int GetFanEnergyBonus(bool isSummer) => isSummer ? 3 : 1;

    public static string GetDescription(HomeUpgrade upgrade) => upgrade switch
    {
        HomeUpgrade.CleanBedding => "Clean Bedding: +2 energy recovery permanently",
        HomeUpgrade.Fan => "Fan: +3 energy recovery in summer, +1 otherwise",
        HomeUpgrade.WindowScreen => "Window Screen: -2 stress daily, +1 energy recovery, blocks insect events",
        HomeUpgrade.Curtain => "Curtain: +1 energy recovery, +1 privacy (reduces negative rumor spread)",
        _ => throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, null)
    };

    public static IReadOnlyList<HomeUpgrade> AllUpgrades { get; } =
        [HomeUpgrade.CleanBedding, HomeUpgrade.Fan, HomeUpgrade.WindowScreen, HomeUpgrade.Curtain];
}
