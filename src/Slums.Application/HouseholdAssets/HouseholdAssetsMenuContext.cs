using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.HouseholdAssets;

public sealed record HouseholdAssetsMenuContext(
    LocationId CurrentLocationId,
    string? LocationName,
    int CurrentWeek,
    int Money,
    Slums.Core.Characters.HouseholdAssetsState Assets)
{
    public static HouseholdAssetsMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new HouseholdAssetsMenuContext(
            gameSession.World.CurrentLocationId,
            gameSession.World.GetCurrentLocation()?.Name,
            gameSession.CurrentWeek,
            gameSession.Player.Stats.Money,
            gameSession.Player.HouseholdAssets);
    }
}
