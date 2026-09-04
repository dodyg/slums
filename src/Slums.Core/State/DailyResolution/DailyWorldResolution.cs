using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Diagnostics;
using Slums.Core.Events;
using Slums.Core.Territory;
using Slums.Core.Weather;
using Slums.Core.World;
using Slums.Core.World.News;

namespace Slums.Core.State.DailyResolution;

/// <summary>
/// Resolves the world-facing blocks of the daily pipeline: police-heat and territory decay,
/// the night transition (clock, Ramadan, district conditions, weather, news), and daily
/// random events.
/// </summary>
internal static class DailyWorldResolution
{
    internal static void DecayPressures(GameSession session)
    {
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee)
        {
            session.DistrictHeat.SetBaselineHeat(DistrictId.Dokki, 10);
        }

        if (session.Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
        {
            session.DistrictHeat.DecayRateModifier = 0.5;
        }

        session.DistrictHeat.DecayAll();
        session.DistrictHeat.ApplyBleedOver();

        TerritoryDynamicsCalculator.ApplyDailyDecay(session.Territory);

        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            var control = session.Territory.GetControl(district);
            if (control.TensionLevel == TensionLevel.Dangerous)
            {
                session.DistrictHeat.AddHeat(district, 3);
            }
        }

        session.RollTerritoryEvents(new Random(session.Clock.Day * 31 + 7919));
    }

    internal static void AdvanceToNextMorning(GameSession session, Random random, Dictionary<string, object?> beforeStats)
    {
        session.Clock.AdvanceToNextDay();
        session.DaysSurvived++;
        session.World.TravelTo(LocationId.Home);

        var newHolidayState = HolidayRegistry.GetHolidayState(GameCalendar.GetDate(session.Clock.Day));
        if (newHolidayState.IsRamadan)
        {
            session.RamadanState = session.RamadanState.AdvanceDay() with
            {
                IsActive = true,
                DaysRemaining = newHolidayState.DaysRemaining
            };
        }
        else if (session.RamadanState.IsActive)
        {
            session.RamadanState = RamadanState.Inactive;
        }

        if (session.UseDynamicDistrictConditions)
        {
            session.RollDistrictConditionsForCurrentDay(random);
        }
        else
        {
            session.SetBaselineDistrictConditions();
        }

        var newSeason = session.GetCurrentSeason();
        session.CurrentWeather = WeatherRoller.Roll(newSeason, random) switch
        {
            var type => WeatherModifiers.GetModifiers(type)
        };
        session.RaiseEvent($"Weather: {WeatherModifiers.GetDisplayName(session.CurrentWeather.Type)}");
        session.RaiseEvent("You return home for the night.");

        var newNews = NewsService.ResolveStartOfDay(session.News, session.Infrastructure, session.EventJournal, session.Clock.Day, random);
        if (newNews is not null)
        {
            foreach (var district in newNews.AffectedDistricts)
            {
                var pressure = NewsImpactCalculator.GetPolicePressureModifier(session.News, district);
                if (pressure > 0)
                {
                    session.DistrictHeat.AddHeat(district, pressure);
                    session.RaiseEvent($"The {newNews.Headline} brings extra document pressure to {DistrictInfo.GetName(district)}.");
                }
            }

            session.RecordMutation(MutationCategories.News, "ResolveStartOfDay", beforeStats, session.CaptureStats(), newNews.Headline);

            if (newNews.InkKnot is not null)
            {
                session.QueueNarrativeScene(newNews.InkKnot);
            }
        }
    }

    internal static void RollDailyEvents(GameSession session, Random random)
    {
        foreach (var randomEvent in session.RandomEventService.RollDailyEvents(session, random))
        {
            session.ApplyRandomEvent(randomEvent);
        }

        session.TryRollStreetCatEncounter(random);
        session.QueueNarrativeFollowUpScenes();
    }
}
