using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class CommunityActionCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, CommunityActionType actionType)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.PerformCommunityAction(actionType);
    }
}
