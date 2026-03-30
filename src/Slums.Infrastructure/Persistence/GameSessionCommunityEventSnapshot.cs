namespace Slums.Infrastructure.Persistence;

public sealed class GameSessionCommunityEventSnapshot
{
    public int ConsecutiveSkips { get; init; }
    public int TotalAttended { get; init; }
    public int LastAttendanceDay { get; init; }
    public IReadOnlyList<string> AttendedThisWeek { get; init; } = [];
    public int LastWeekResetDay { get; init; }
    public bool HasTeaCircleInvitation { get; init; }

    public static GameSessionCommunityEventSnapshot Capture(Slums.Core.State.GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        var attendance = gameSession.EventAttendance;
        return new GameSessionCommunityEventSnapshot
        {
            ConsecutiveSkips = attendance.ConsecutiveSkips,
            TotalAttended = attendance.TotalAttended,
            LastAttendanceDay = attendance.LastAttendanceDay,
            AttendedThisWeek = attendance.AttendedThisWeek.Select(static e => e.ToString()).ToArray(),
            LastWeekResetDay = attendance.LastWeekResetDay,
            HasTeaCircleInvitation = attendance.HasTeaCircleInvitation
        };
    }
}
