using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed class ClinicTravelCommand
{
    public TravelAndClinicVisitResult Execute(GameSession gameSession, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.TravelAndTakeMotherToClinic(locationId);
    }
}
