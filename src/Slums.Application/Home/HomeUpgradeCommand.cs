using Slums.Core.Home;
using Slums.Core.State;

namespace Slums.Application.Home;

/// <summary>
/// Purchases a home upgrade on behalf of the player.
/// </summary>
public sealed class HomeUpgradeCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, HomeUpgrade upgrade)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.TryPurchaseHomeUpgrade(upgrade);
    }
}
