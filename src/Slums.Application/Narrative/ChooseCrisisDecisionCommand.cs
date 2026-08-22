using Slums.Core.Narrative;
using Slums.Core.State;

namespace Slums.Application.Narrative;

public static class ChooseCrisisDecisionCommand
{
    public static bool Execute(GameSession gameSession, CityCrisisDecision decision)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.ChooseCrisisDecision(decision);
    }
}
