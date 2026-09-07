using Slums.Core.State;
using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed class TechnicalRepairCommand
{
    public bool Execute(GameSession gameSession, TechnicalRepairActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.PerformTechnicalRepair(actionType);
    }
}
