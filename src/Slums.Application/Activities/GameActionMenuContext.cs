using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed record GameActionMenuContext(
    Location? CurrentLocation,
    bool IsAtHome,
    bool HasReachableNpcs,
    bool HasInvestmentOpportunities,
    bool HasHouseholdAssetsAccess)
{
    public static GameActionMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameActionMenuContext(
            gameSession.World.GetCurrentLocation(),
            gameSession.World.CurrentLocationId == LocationId.Home,
            gameSession.GetReachableNpcs().Count > 0,
            gameSession.GetCurrentInvestmentOpportunities().Count > 0,
            gameSession.CanUseHouseholdAssets());
    }
}
