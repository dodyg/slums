namespace Slums.Core.Clock;

public enum GameDayOfWeek
{
    Saturday = 0,
    Sunday = 1,
    Monday = 2,
    Tuesday = 3,
    Wednesday = 4,
    Thursday = 5,
    Friday = 6
}

public static class GameDayOfWeekExtensions
{
    public static System.DayOfWeek ToSystemDayOfWeek(this GameDayOfWeek day) => day switch
    {
        GameDayOfWeek.Saturday => System.DayOfWeek.Saturday,
        GameDayOfWeek.Sunday => System.DayOfWeek.Sunday,
        GameDayOfWeek.Monday => System.DayOfWeek.Monday,
        GameDayOfWeek.Tuesday => System.DayOfWeek.Tuesday,
        GameDayOfWeek.Wednesday => System.DayOfWeek.Wednesday,
        GameDayOfWeek.Thursday => System.DayOfWeek.Thursday,
        GameDayOfWeek.Friday => System.DayOfWeek.Friday,
        _ => throw new ArgumentOutOfRangeException(nameof(day), day, null)
    };
}
