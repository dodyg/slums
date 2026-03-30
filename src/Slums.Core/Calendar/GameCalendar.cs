namespace Slums.Core.Calendar;

public sealed class GameCalendar
{
    private static readonly DateOnly StartDate = new(2024, 10, 1);

    public static DateOnly GetDate(int gameDay)
    {
        return StartDate.AddDays(gameDay - 1);
    }

    public static Season GetSeason(int gameDay)
    {
        var date = GetDate(gameDay);
        return GetSeasonFromDate(date);
    }

    public static Season GetSeasonFromDate(DateOnly date)
    {
        return date.Month switch
        {
            >= 3 and <= 5 => Season.Spring,
            >= 6 and <= 9 => Season.Summer,
            >= 10 and <= 11 => Season.Autumn,
            12 => Season.Winter,
            1 or 2 => Season.Winter,
            _ => Season.Autumn
        };
    }

    public static string GetSeasonName(Season season) => season switch
    {
        Season.Autumn => "Autumn",
        Season.Winter => "Winter",
        Season.Spring => "Spring",
        Season.Summer => "Summer",
        _ => throw new ArgumentOutOfRangeException(nameof(season), season, null)
    };

    public static int GetMonth(int gameDay) => GetDate(gameDay).Month;

    public static int GetDayOfMonth(int gameDay) => GetDate(gameDay).Day;
}
