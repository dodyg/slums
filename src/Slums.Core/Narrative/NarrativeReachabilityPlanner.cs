using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Weather;

namespace Slums.Core.Narrative;

/// <summary>
/// Selects deterministic entry scenes for authored climate and seasonal content.
/// </summary>
public static class NarrativeReachabilityPlanner
{
    /// <summary>Returns the eligible weather scene, if its one-time cooldown has not fired.</summary>
    public static NarrativeSceneTrigger? GetWeatherTrigger(
        NarrativeReachabilityContext context,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(storyFlags);

        if (context.Season == Season.Winter && context.MotherHealth < 60)
        {
            var winterChill = CreateTrigger(StoryFlags.WeatherWinterChillSeen, "event_winter_chill", storyFlags);
            if (winterChill is not null)
            {
                return winterChill;
            }
        }

        return context.Weather switch
        {
            WeatherType.Khamsin => GetKhamsinTrigger(context.Background, storyFlags),
            WeatherType.Rain => GetRainTrigger(context, storyFlags),
            WeatherType.Heatwave => context.Background == BackgroundType.MedicalSchoolDropout
                ? CreateTrigger(StoryFlags.WeatherHeatwaveMedicalSeen, "event_heatwave_medical", storyFlags)
                : CreateTrigger(StoryFlags.WeatherHeatwaveSeen, "event_heatwave", storyFlags),
            WeatherType.CoolOvercast => CreateTrigger(StoryFlags.WeatherCoolDaySeen, "event_cool_day", storyFlags),
            WeatherType.Windy => CreateTrigger(StoryFlags.WeatherWindyDaySeen, "event_windy_day", storyFlags),
            _ => null
        };
    }

    /// <summary>Returns the eligible seasonal or holiday scene for the current day.</summary>
    public static NarrativeSceneTrigger? GetSeasonalTrigger(
        NarrativeReachabilityContext context,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(storyFlags);

        if (context.HolidayDay == 1)
        {
            var holidayTrigger = context.Holiday switch
            {
                HolidayId.Ramadan => CreateTrigger(StoryFlags.SeasonRamadanStartSeen, "event_ramadan_start", storyFlags),
                HolidayId.EidAlFitr => CreateTrigger(StoryFlags.SeasonEidAlFitrSeen, "event_eid_al_fitr", storyFlags),
                HolidayId.EidAlAdha => CreateTrigger(StoryFlags.SeasonEidAlAdhaSeen, "event_eid_al_adha", storyFlags),
                HolidayId.CopticChristmas => CreateTrigger(StoryFlags.SeasonCopticChristmasSeen, "event_coptic_christmas", storyFlags),
                HolidayId.ShamElNessim => CreateTrigger(StoryFlags.SeasonShamElNessimSeen, "event_sham_el_nessim", storyFlags),
                _ => null
            };

            if (holidayTrigger is not null)
            {
                return holidayTrigger;
            }
        }

        if (context.Holiday == HolidayId.Ramadan && context.HolidayDay == 2)
        {
            return context.Background == BackgroundType.SudaneseRefugee
                ? CreateTrigger(StoryFlags.SeasonRamadanIftarSudaneseSeen, "event_ramadan_iftar_sudanese", storyFlags)
                : CreateTrigger(StoryFlags.SeasonRamadanIftarSeen, "event_ramadan_iftar", storyFlags);
        }

        if (IsFirstDayOfSeason(context.Day, Season.Summer, context.Season))
        {
            return CreateTrigger(StoryFlags.SeasonSummerSolsticeSeen, "event_summer_solstice", storyFlags);
        }

        if (IsFirstDayOfSeason(context.Day, Season.Autumn, context.Season))
        {
            return CreateTrigger(StoryFlags.SeasonAutumnFirstSeen, "event_autumn_first", storyFlags);
        }

        if (IsFirstDayOfSeason(context.Day, Season.Winter, context.Season)
            && context.Weather == WeatherType.Rain)
        {
            return CreateTrigger(StoryFlags.SeasonWinterFirstRainSeen, "event_winter_first_rain", storyFlags);
        }

        if (IsFirstDayOfSeason(context.Day, Season.Spring, context.Season))
        {
            return CreateTrigger(StoryFlags.SeasonSpringKhamsinWarningSeen, "event_spring_khamsin_warning", storyFlags);
        }

        return null;
    }

    private static NarrativeSceneTrigger? GetKhamsinTrigger(
        BackgroundType background,
        IReadOnlySet<string> storyFlags)
    {
        return background switch
        {
            BackgroundType.ReleasedPoliticalPrisoner => CreateTrigger(
                StoryFlags.WeatherKhamsinPrisonerSeen,
                "event_khamsin_prisoner",
                storyFlags),
            BackgroundType.SudaneseRefugee => CreateTrigger(
                StoryFlags.WeatherKhamsinSudaneseSeen,
                "event_khamsin_sudanese",
                storyFlags),
            _ => CreateTrigger(StoryFlags.WeatherKhamsinSeen, "event_khamsin", storyFlags)
        };
    }

    private static NarrativeSceneTrigger? GetRainTrigger(
        NarrativeReachabilityContext context,
        IReadOnlySet<string> storyFlags)
    {
        if (context.Background == BackgroundType.ReleasedPoliticalPrisoner)
        {
            return CreateTrigger(StoryFlags.WeatherRainPrisonerSeen, "event_rain_prisoner", storyFlags);
        }

        if (!context.IsAtHome)
        {
            return CreateTrigger(StoryFlags.WeatherRainOutsideSeen, "event_rain_outside", storyFlags);
        }

        return context.HasCurtain
            ? CreateTrigger(StoryFlags.WeatherRainLeakWithCurtainSeen, "event_rain_leak_with_curtain", storyFlags)
            : CreateTrigger(StoryFlags.WeatherRainLeakSeen, "event_rain_leak", storyFlags);
    }

    private static NarrativeSceneTrigger? CreateTrigger(
        string flagName,
        string knotName,
        IReadOnlySet<string> storyFlags)
    {
        return storyFlags.Contains(flagName)
            ? null
            : new NarrativeSceneTrigger(flagName, knotName);
    }

    private static bool IsFirstDayOfSeason(int day, Season season, Season currentSeason)
    {
        if (currentSeason != season)
        {
            return false;
        }

        if (day == 1)
        {
            return true;
        }

        return GameCalendar.GetSeason(day - 1) != season;
    }
}
