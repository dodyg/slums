using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed class FishTankUpgradeCommand
{
    public bool Execute(GameSession gameSession, FishTankUpgradeType upgradeType)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.UpgradeFishTank(upgradeType);
    }
}
