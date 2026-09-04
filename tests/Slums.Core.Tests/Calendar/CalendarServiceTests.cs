using FluentAssertions;
using Slums.Core.Calendar;
using Slums.Core.State;
using Slums.Core.Weather;
using TUnit.Core;

namespace Slums.Core.Tests.Calendar;

internal sealed class CalendarServiceTests
{
    [Test]
    public void CalendarQueries_ShouldReadTheSessionClock()
    {
        var session = new GameSession();
        session.Clock.SetTime(8, 13, 30);

        CalendarService.GetCurrentWeek(session).Should().Be(2);
        CalendarService.GetCurrentDayOfWeek(session).Should().Be(session.Clock.DayOfWeek);
        CalendarService.GetCurrentSeason(session).Should().Be(GameCalendar.GetSeason(8));
    }

    [Test]
    public void RestoreWeather_ShouldReplaceTheSessionWeather()
    {
        var session = new GameSession();

        CalendarService.RestoreWeather(session, WeatherType.Khamsin);

        session.CurrentWeather.Type.Should().Be(WeatherType.Khamsin);
        session.CurrentWeather.BlocksCrime.Should().BeTrue();
    }
}
