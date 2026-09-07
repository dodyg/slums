using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed class PlantUpgradeCommand
{
    public bool Execute(GameSession gameSession, Guid plantId, PlantUpgradeType upgradeType)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.UpgradePlant(plantId, upgradeType);
    }
}
