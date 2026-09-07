using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class CommunityActionCommand
{
    public bool Execute(GameSession gameSession, CommunityActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.PerformCommunityAction(actionType);
    }
}
