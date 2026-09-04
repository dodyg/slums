using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Home;
using Slums.Core.Relationships;
using Slums.Core.Weather;
using Slums.Core.World;

namespace Slums.Core.State.DailyResolution;

/// <summary>
/// Resolves the overnight survival-math block of the daily pipeline: stat decay, season,
/// weather, holiday, and Ramadan modifiers, nutrition, sleep recovery, household care,
/// and infrastructure sleep stress.
/// </summary>
internal static class DailyStatResolution
{
    internal static void ApplyDecayAndRecovery(GameSession session, int currentWeek)
    {
        var player = session.Player;
        player.Stats.ApplyDailyDecay();

        var seasonModifiers = SeasonModifiersRegistry.GetModifiers(session.GetCurrentSeason());
        if (seasonModifiers.EnergyDrainModifier != 0)
        {
            player.Stats.ModifyEnergy(-seasonModifiers.EnergyDrainModifier);
        }

        if (seasonModifiers.StressModifier != 0)
        {
            player.Stats.ModifyStress(seasonModifiers.StressModifier);
        }

        if (session.CurrentWeather.EnergyDrainModifier != 0)
        {
            player.Stats.ModifyEnergy(-session.CurrentWeather.EnergyDrainModifier);
        }

        if (session.CurrentWeather.StressModifier != 0)
        {
            player.Stats.ModifyStress(session.CurrentWeather.StressModifier);
        }

        if (session.CurrentWeather.HealthModifier != 0
            && (session.CurrentWeather.Type == WeatherType.Heatwave && player.Stats.Energy < 30))
        {
            player.Stats.ModifyHealth(session.CurrentWeather.HealthModifier);
        }

        var holidayState = HolidayRegistry.GetHolidayState(GameCalendar.GetDate(session.Clock.Day));
        if (holidayState.IsActive)
        {
            if (holidayState.StressModifier.HasValue && holidayState.StressModifier.Value != 0)
            {
                player.Stats.ModifyStress(holidayState.StressModifier.Value);
            }

            if (holidayState.MotherHealthModifier.HasValue && holidayState.MotherHealthModifier.Value != 0)
            {
                player.Household.UpdateMotherHealth(holidayState.MotherHealthModifier.Value);
            }
        }

        if (holidayState.IsRamadan && session.RamadanState.PlayerIsFasting)
        {
            if (session.RamadanState.EnergyModifier != 0)
            {
                player.Stats.ModifyEnergy(session.RamadanState.EnergyModifier);
            }
            if (session.RamadanState.StressModifier != 0)
            {
                player.Stats.ModifyStress(session.RamadanState.StressModifier);
            }
            if (session.RamadanState.TrustModifierWithReligiousNpcs != 0)
            {
                session.Relationships.ModifyNpcTrust(NpcId.LandlordHajjMahmoud, session.RamadanState.TrustModifierWithReligiousNpcs);
            }
        }

        var nutritionResolution = player.Nutrition.ResolveDay();
        player.Stats.ModifyEnergy(nutritionResolution.EnergyDelta);
        player.Stats.ModifyHealth(nutritionResolution.HealthDelta);
        player.Stats.ModifyStress(nutritionResolution.StressDelta);
        session.SyncLegacyHunger();

        var seasonRestBonus = seasonModifiers.RestRecoveryBonus;
        var overnightRecovery = SleepQualityCalculator.CalculateOvernightRecovery(
            player.Stats, player.Nutrition, player.Household,
            session.UnpaidRentDays, session.HomeUpgrades, seasonRestBonus);
        player.Stats.ModifyEnergy(overnightRecovery);

        if (session.HomeUpgrades.GetStressBonus() > 0)
        {
            player.Stats.ModifyStress(-session.HomeUpgrades.GetStressBonus());
        }

        var motherCareResolution = player.Household.ResolveDay();
        player.Stats.ModifyStress(motherCareResolution.StressDelta);
        var householdAssetsBonus = player.HouseholdAssets.GetMotherDailyHealthBonus(currentWeek);
        if (householdAssetsBonus > 0)
        {
            player.Household.UpdateMotherHealth(householdAssetsBonus);
        }

        var overnightInfrastructureStress = InfrastructureImpactCalculator.GetSleepStressModifier(
            session.Infrastructure, session.World.CurrentDistrict);
        if (overnightInfrastructureStress > 0)
        {
            player.Stats.ModifyStress(overnightInfrastructureStress);
            session.RaiseEvent($"Unreliable utilities make sleep harder. Stress +{overnightInfrastructureStress}.");
        }
    }

    internal static void RaiseDailyRecapEvents(GameSession session)
    {
        if (!session.Player.Nutrition.AteToday)
        {
            session.RaiseEvent("You go to sleep hungry.");
        }

        if (!session.Player.Household.FedMotherToday)
        {
            session.RaiseEvent("Your mother went without a proper meal today.");
        }

        if (!session.Player.Household.MedicationGivenToday && session.Player.Household.MotherNeedsCare)
        {
            session.RaiseEvent("Your mother needed medicine today and did not get it.");
        }
    }

    internal static void ApplyBackgroundAndGenderStress(GameSession session)
    {
        if (session.Player.BackgroundType == BackgroundType.MedicalSchoolDropout
            && session.Player.Household.MotherHealth < 60)
        {
            session.Player.Stats.ModifyStress(3);
            session.RaiseEvent("Your training makes it harder to ignore every sign your mother's health is slipping.");
        }

        var genderDailyStress = GenderModifiers.DailyStressModifier(session.Player.Gender);
        if (genderDailyStress != 0)
        {
            session.Player.Stats.ModifyStress(genderDailyStress);
            session.RaiseEvent(genderDailyStress > 0
                ? "The streets have their own weight today."
                : "You move through the city a little easier today.");
        }
    }

    internal static void BeginNewDay(GameSession session)
    {
        session.Player.Nutrition.BeginNewDay();
        session.Player.Household.BeginNewDay();
        session.ClearDailyTraining();
    }
}
