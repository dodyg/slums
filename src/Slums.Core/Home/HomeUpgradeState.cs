namespace Slums.Core.Home;

public sealed class HomeUpgradeState
{
    private readonly HashSet<HomeUpgrade> _purchased = [];

    public IReadOnlySet<HomeUpgrade> PurchasedUpgrades => _purchased;

    public bool HasUpgrade(HomeUpgrade upgrade) => _purchased.Contains(upgrade);

    public bool Purchase(HomeUpgrade upgrade)
    {
        if (_purchased.Contains(upgrade))
        {
            return false;
        }

        _purchased.Add(upgrade);
        return true;
    }

    public int GetEnergyRecoveryBonus()
    {
        var bonus = 0;

        if (HasUpgrade(HomeUpgrade.CleanBedding))
        {
            bonus += HomeUpgradeDefinitions.GetEnergyBonus(HomeUpgrade.CleanBedding);
        }

        if (HasUpgrade(HomeUpgrade.Fan))
        {
            bonus += HomeUpgradeDefinitions.GetFanEnergyBonus(isSummer: false);
        }

        if (HasUpgrade(HomeUpgrade.WindowScreen))
        {
            bonus += HomeUpgradeDefinitions.GetEnergyBonus(HomeUpgrade.WindowScreen);
        }

        if (HasUpgrade(HomeUpgrade.Curtain))
        {
            bonus += HomeUpgradeDefinitions.GetEnergyBonus(HomeUpgrade.Curtain);
        }

        return bonus;
    }

    public int GetStressBonus()
    {
        return HasUpgrade(HomeUpgrade.WindowScreen)
            ? HomeUpgradeDefinitions.WindowScreenStressReduction
            : 0;
    }

    public int GetPrivacyBonus()
    {
        return HasUpgrade(HomeUpgrade.Curtain)
            ? HomeUpgradeDefinitions.CurtainPrivacyBonus
            : 0;
    }

    public void Restore(IEnumerable<HomeUpgrade> upgrades)
    {
        ArgumentNullException.ThrowIfNull(upgrades);
        _purchased.Clear();
        foreach (var upgrade in upgrades)
        {
            _purchased.Add(upgrade);
        }
    }
}
