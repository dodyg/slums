using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Narrative;
using Slums.Core.Weather;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class NarrativeReachabilityPlannerTests
{
    [Test]
    public async Task WeatherPlanner_SelectsEveryAuthoredWeatherEntry()
    {
        var cases = new Dictionary<string, NarrativeReachabilityContext>
        {
            ["event_khamsin"] = Context(WeatherType.Khamsin),
            ["event_khamsin_prisoner"] = Context(WeatherType.Khamsin, background: BackgroundType.ReleasedPoliticalPrisoner),
            ["event_khamsin_sudanese"] = Context(WeatherType.Khamsin, background: BackgroundType.SudaneseRefugee),
            ["event_rain_leak"] = Context(WeatherType.Rain),
            ["event_rain_leak_with_curtain"] = Context(WeatherType.Rain, hasCurtain: true),
            ["event_rain_outside"] = Context(WeatherType.Rain, isAtHome: false),
            ["event_rain_prisoner"] = Context(WeatherType.Rain, background: BackgroundType.ReleasedPoliticalPrisoner),
            ["event_heatwave"] = Context(WeatherType.Heatwave, background: BackgroundType.ReleasedPoliticalPrisoner),
            ["event_heatwave_medical"] = Context(WeatherType.Heatwave, background: BackgroundType.MedicalSchoolDropout),
            ["event_cool_day"] = Context(WeatherType.CoolOvercast),
            ["event_windy_day"] = Context(WeatherType.Windy),
            ["event_winter_chill"] = Context(WeatherType.Clear, season: Season.Winter, motherHealth: 40)
        };

        foreach (var (expectedKnot, context) in cases)
        {
            var trigger = NarrativeReachabilityPlanner.GetWeatherTrigger(context, new HashSet<string>());
            await Assert.That(trigger?.KnotName).IsEqualTo(expectedKnot);
        }
    }

    [Test]
    public async Task WeatherPlanner_DoesNotReplayASeenScene()
    {
        var flags = new HashSet<string> { StoryFlags.WeatherCoolDaySeen };
        var trigger = NarrativeReachabilityPlanner.GetWeatherTrigger(Context(WeatherType.CoolOvercast), flags);

        await Assert.That(trigger).IsNull();
    }

    [Test]
    public async Task SeasonalPlanner_SelectsHolidayAndSeasonEntries()
    {
        var cases = new Dictionary<string, NarrativeReachabilityContext>
        {
            ["event_ramadan_start"] = Context(holiday: HolidayId.Ramadan, holidayDay: 1),
            ["event_ramadan_iftar"] = Context(holiday: HolidayId.Ramadan, holidayDay: 2),
            ["event_ramadan_iftar_sudanese"] = Context(holiday: HolidayId.Ramadan, holidayDay: 2, background: BackgroundType.SudaneseRefugee),
            ["event_eid_al_fitr"] = Context(holiday: HolidayId.EidAlFitr, holidayDay: 1),
            ["event_eid_al_adha"] = Context(holiday: HolidayId.EidAlAdha, holidayDay: 1),
            ["event_coptic_christmas"] = Context(holiday: HolidayId.CopticChristmas, holidayDay: 1),
            ["event_sham_el_nessim"] = Context(holiday: HolidayId.ShamElNessim, holidayDay: 1),
            ["event_summer_solstice"] = Context(day: 244, season: Season.Summer),
            ["event_autumn_first"] = Context(day: 1, season: Season.Autumn),
            ["event_winter_first_rain"] = Context(day: 62, season: Season.Winter, weather: WeatherType.Rain),
            ["event_spring_khamsin_warning"] = Context(day: 152, season: Season.Spring)
        };

        foreach (var (expectedKnot, context) in cases)
        {
            var trigger = NarrativeReachabilityPlanner.GetSeasonalTrigger(context, new HashSet<string>());
            await Assert.That(trigger?.KnotName).IsEqualTo(expectedKnot);
        }
    }

    [Test]
    public async Task SeasonalPlanner_DoesNotReplayASeenHolidayEntry()
    {
        var flags = new HashSet<string> { StoryFlags.SeasonRamadanStartSeen };
        var context = Context(day: 70, season: Season.Winter, holiday: HolidayId.Ramadan, holidayDay: 1);

        var trigger = NarrativeReachabilityPlanner.GetSeasonalTrigger(context, flags);

        await Assert.That(trigger).IsNull();
    }

    private static NarrativeReachabilityContext Context(
        WeatherType weather = WeatherType.Clear,
        Season season = Season.Autumn,
        HolidayId holiday = HolidayId.None,
        int holidayDay = 0,
        BackgroundType background = BackgroundType.MedicalSchoolDropout,
        GameDayOfWeek dayOfWeek = GameDayOfWeek.Saturday,
        bool isAtHome = true,
        bool hasCurtain = false,
        int motherHealth = 80,
        int day = 1)
    {
        return new NarrativeReachabilityContext(
            day,
            weather,
            season,
            holiday,
            holidayDay,
            background,
            dayOfWeek,
            isAtHome,
            hasCurtain,
            motherHealth);
    }
}
