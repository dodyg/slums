namespace Slums.Core.Clock;

public static class DailyActivityWindow
{
    public static int GetRemainingMinutes(GameClock clock, int endOfDayHour)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentOutOfRangeException.ThrowIfLessThan(endOfDayHour, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(endOfDayHour, 23);

        var currentMinutes = (clock.Hour * 60) + clock.Minute;
        return Math.Max(0, (endOfDayHour * 60) - currentMinutes);
    }

    public static bool CanComplete(GameClock clock, int durationMinutes, int endOfDayHour)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentOutOfRangeException.ThrowIfNegative(durationMinutes);

        return durationMinutes <= GetRemainingMinutes(clock, endOfDayHour);
    }
}
