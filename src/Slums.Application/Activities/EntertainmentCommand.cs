using Slums.Core.Entertainment;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class EntertainmentCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, EntertainmentActivity activity)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(activity);
        return gameSession.TryPerformEntertainment(activity);
    }
}
