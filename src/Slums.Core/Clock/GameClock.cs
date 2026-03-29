namespace Slums.Core.Clock;

public sealed record GameClock
{
    public int Day { get; private set; } = 1;
    public int Hour { get; private set; } = 6;
    public int Minute { get; private set; }

    public TimeOfDay TimeOfDay => Hour switch
    {
        >= 5 and < 12 => TimeOfDay.Morning,
        >= 12 and < 17 => TimeOfDay.Afternoon,
        >= 17 and < 21 => TimeOfDay.Evening,
        _ => TimeOfDay.Night
    };

    public void AdvanceMinutes(int minutes)
    {
        Minute += minutes;
        while (Minute >= 60)
        {
            Minute -= 60;
            Hour++;
        }

        if (Hour >= 24)
        {
            Hour -= 24;
            Day++;
        }
    }

    public void AdvanceHours(int hours)
    {
        Hour += hours;
        while (Hour >= 24)
        {
            Hour -= 24;
            Day++;
        }
    }

    public void AdvanceToNextDay()
    {
        Day++;
        Hour = 6;
        Minute = 0;
    }

    public void SetTime(int day, int hour, int minute)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(day, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(hour, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hour, 23);
        ArgumentOutOfRangeException.ThrowIfLessThan(minute, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minute, 59);

        Day = day;
        Hour = hour;
        Minute = minute;
    }

    public bool IsEndOfDay => Hour >= 22;

    public GameDayOfWeek DayOfWeek => (GameDayOfWeek)((Day - 1) % 7);
}

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening,
    Night
}
