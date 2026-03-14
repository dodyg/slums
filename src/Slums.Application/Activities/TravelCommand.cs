using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed class TravelCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, LocationId locationId, TravelMode mode)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return mode switch
        {
            TravelMode.Transport => gameSession.TryTravelTo(locationId),
            TravelMode.Walk => gameSession.TryWalkTo(locationId),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}
