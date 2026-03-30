using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class AttendCommunityEventCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, CommunityEventId eventId)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.AttendCommunityEvent(eventId);
    }
}
