using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record CommunityEventMenuContext(
    int PlayerMoney,
    int CurrentHour,
    int EndOfDayHour,
    IReadOnlyList<CommunityEventDefinition> AvailableEvents,
    CommunityEventAttendance Attendance)
{
    public static CommunityEventMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new CommunityEventMenuContext(
            gameSession.Player.Stats.Money,
            gameSession.Clock.Hour,
            22,
            gameSession.GetAvailableCommunityEvents(),
            gameSession.EventAttendance);
    }
}
