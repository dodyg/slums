using Slums.Core.Characters;
using Slums.Core.Entertainment;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record EntertainmentMenuContext(
    PlayerCharacter Player,
    IReadOnlyList<EntertainmentActivity> Activities,
    string? LocationName)
{
    public static EntertainmentMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var location = gameSession.World.GetCurrentLocation();
        var activities = gameSession.GetAvailableEntertainmentActivities();

        return new EntertainmentMenuContext(
            gameSession.Player,
            activities,
            location?.Name);
    }
}
