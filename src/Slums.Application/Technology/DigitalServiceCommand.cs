using Slums.Core.State;
using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed class DigitalServiceCommand
{
    public bool Execute(GameSession gameSession, DigitalServiceActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.PerformDigitalService(actionType);
    }
}
