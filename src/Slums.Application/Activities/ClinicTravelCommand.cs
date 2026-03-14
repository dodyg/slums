using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed class ClinicTravelCommand
{
#pragma warning disable CA1822
    public TravelAndClinicVisitResult Execute(GameSession gameSession, LocationId locationId)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.TravelAndTakeMotherToClinic(locationId);
    }
}
