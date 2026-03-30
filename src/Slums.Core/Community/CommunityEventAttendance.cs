namespace Slums.Core.Community;

public sealed class CommunityEventAttendance
{
    public int ConsecutiveSkips { get; set; }
    public int TotalAttended { get; set; }
    public int LastAttendanceDay { get; set; }
    public HashSet<CommunityEventId> AttendedThisWeek { get; } = [];
    public int LastWeekResetDay { get; set; }
    public bool HasTeaCircleInvitation { get; set; }

    public void RecordAttendance(CommunityEventId eventId, int currentDay)
    {
        ConsecutiveSkips = 0;
        TotalAttended++;
        LastAttendanceDay = currentDay;
        AttendedThisWeek.Add(eventId);
    }

    public void RecordSkip()
    {
        ConsecutiveSkips++;
    }

    public void ResetWeeklyIfNeeded(int currentDay)
    {
        var currentWeek = (currentDay - 1) / 7;
        var lastWeek = (LastWeekResetDay - 1) / 7;
        if (currentWeek > lastWeek)
        {
            AttendedThisWeek.Clear();
            HasTeaCircleInvitation = false;
            LastWeekResetDay = currentDay;
        }
    }
}
