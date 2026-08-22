using Slums.Core.State;

namespace Slums.Application.Narrative;

public static class CommitCrisisResourcesCommand
{
    public static bool Execute(GameSession gameSession, int amount)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.CommitCrisisResources(amount);
    }
}
