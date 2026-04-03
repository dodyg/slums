using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed class FishTankUpgradeCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, FishTankUpgradeType upgradeType)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.UpgradeFishTank(upgradeType);
    }
}
