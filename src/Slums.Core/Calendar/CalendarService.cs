using Slums.Core.Clock;
using Slums.Core.State;
using Slums.Core.Weather;

namespace Slums.Core.Calendar;

internal static class CalendarService
{
    internal static int GetCurrentWeek(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return ((session.Clock.Day - 1) / 7) + 1;
    }

    internal static GameDayOfWeek GetCurrentDayOfWeek(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Clock.DayOfWeek;
    }

    internal static DayScheduleModifiers GetCurrentSchedule(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return DayScheduleRegistry.GetModifiers(session.Clock.DayOfWeek);
    }

    internal static Season GetCurrentSeason(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return GameCalendar.GetSeason(session.Clock.Day);
    }

    internal static SeasonModifiers GetCurrentSeasonModifiers(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return SeasonModifiersRegistry.GetModifiers(GetCurrentSeason(session));
    }

    internal static ActiveHolidayState GetActiveHolidayState(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return HolidayRegistry.GetHolidayState(GameCalendar.GetDate(session.Clock.Day));
    }

    internal static void SetRamadanFasting(GameSession session, bool isFasting)
    {
        ArgumentNullException.ThrowIfNull(session);

        var holidayState = GetActiveHolidayState(session);
        if (!holidayState.IsRamadan)
        {
            return;
        }

        session.RamadanState = session.RamadanState with
        {
            IsActive = true,
            PlayerIsFasting = isFasting,
            DaysRemaining = holidayState.DaysRemaining
        };
    }

    internal static void RestoreRamadanState(GameSession session, bool isActive, bool playerIsFasting, int daysFasting, int daysRemaining)
    {
        ArgumentNullException.ThrowIfNull(session);

        session.RamadanState = new RamadanState
        {
            IsActive = isActive,
            PlayerIsFasting = playerIsFasting,
            DaysFasting = daysFasting,
            DaysRemaining = daysRemaining
        };
    }

    internal static void RestoreWeather(GameSession session, WeatherType weatherType)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.CurrentWeather = WeatherModifiers.GetModifiers(weatherType);
    }
}
