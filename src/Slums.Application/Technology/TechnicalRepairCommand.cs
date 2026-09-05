using Slums.Core.State;
using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed class TechnicalRepairCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, TechnicalRepairActionType actionType)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.PerformTechnicalRepair(actionType);
    }
}
