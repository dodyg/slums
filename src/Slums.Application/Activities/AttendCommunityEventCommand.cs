using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class AttendCommunityEventCommand
{
    public bool Execute(GameSession gameSession, CommunityEventId eventId)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.AttendCommunityEvent(eventId);
    }
}
