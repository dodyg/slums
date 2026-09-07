using Slums.Core.Home;
using Slums.Core.State;

namespace Slums.Application.Home;

/// <summary>
/// Purchases a home upgrade on behalf of the player.
/// </summary>
public sealed class HomeUpgradeCommand
{
    public bool Execute(GameSession gameSession, HomeUpgrade upgrade)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.TryPurchaseHomeUpgrade(upgrade);
    }
}
