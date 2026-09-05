using Slums.Core.State;
using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed class DigitalServiceCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, DigitalServiceActionType actionType)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.PerformDigitalService(actionType);
    }
}
