using Slums.Core.State;

namespace Slums.Application.Narrative;

public static class CollectCrisisEvidenceCommand
{
    public static bool Execute(GameSession gameSession, int amount = 1)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.CollectCrisisEvidence(amount);
    }
}
