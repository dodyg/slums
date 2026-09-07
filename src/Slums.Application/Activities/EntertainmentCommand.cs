using Slums.Core.Entertainment;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class EntertainmentCommand
{
    public bool Execute(GameSession gameSession, EntertainmentActivity activity)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(activity);
        return gameSession.TryPerformEntertainment(activity);
    }
}
