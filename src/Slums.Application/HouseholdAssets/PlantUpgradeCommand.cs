using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed class PlantUpgradeCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, Guid plantId, PlantUpgradeType upgradeType)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.UpgradePlant(plantId, upgradeType);
    }
}
