using Slums.Core.Diagnostics;
using Slums.Core.Calendar;
using Slums.Core.Clock;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Core.World;
using Slums.Core.World.News;
using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

namespace Slums.Core.Characters;

/// <summary>Applies mother care and clinic visit rules, including composed clinic travel.</summary>
internal static class ClinicVisitService
{
    internal static void CheckOnMother(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        session.Player.Household.CheckOnMother();
        var message = GetMotherStatusMessage(session);
        session.RaiseEvent(message);
        session.RecordMutation(MutationCategories.Clinic, "CheckOnMother", before, session.CaptureStats(), message);
    }

    internal static bool GiveMotherMedicine(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (!session.Player.Household.GiveMedicine())
        {
            session.RecordMutation(MutationCategories.GuardRejected, "GiveMotherMedicine", before, session.CaptureStats(), "No medicine available");
            session.RaiseEvent("You have no medicine to give.");
            return false;
        }

        session.RaiseEvent("You give your mother her medicine.");
        session.RecordMutation(MutationCategories.Clinic, "GiveMotherMedicine", before, session.CaptureStats(), "Gave mother medicine");
        return true;
    }

    internal static MotherClinicVisitResult TakeMotherToClinic(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var clinicStatus = GetCurrentLocationClinicStatus(session);
        if (!clinicStatus.HasClinicServices)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TakeMotherToClinic", before, session.CaptureStats(), "No clinic at this location");
            session.RaiseEvent("There is no clinic service at this location.");
            return new MotherClinicVisitResult(false, 0, 0);
        }

        if (!clinicStatus.IsOpenToday)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TakeMotherToClinic", before, session.CaptureStats(), $"{clinicStatus.LocationName} closed today");
            session.RaiseEvent($"{clinicStatus.LocationName} is closed today. Open days: {clinicStatus.OpenDaysSummary}.");
            return new MotherClinicVisitResult(false, clinicStatus.VisitCost, 0);
        }

        if (session.Player.Stats.Money < clinicStatus.VisitCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TakeMotherToClinic", before, session.CaptureStats(), $"Not enough money (need {clinicStatus.VisitCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. A clinic visit costs {clinicStatus.VisitCost} LE here.");
            return new MotherClinicVisitResult(false, clinicStatus.VisitCost, 0);
        }

        const int clinicVisitMinutes = 90;
        var healthBonus = 0;
        if (session.Player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            healthBonus += 5;
        }

        if (session.World.CurrentLocationId == LocationId.Clinic && session.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 15)
        {
            healthBonus += 3;
        }

        if (session.World.CurrentLocationId == LocationId.Pharmacy && session.Relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 12)
        {
            healthBonus += 2;
        }

        var healthChange = Math.Clamp(15 + healthBonus, 0, 100 - session.Player.Household.MotherHealth);
        session.Player.Stats.ModifyMoney(-clinicStatus.VisitCost);
        session.Player.Household.UpdateMotherHealth(healthChange);
        session.Player.Stats.ModifyEnergy(-10);
        session.ApplySkillGain(SkillId.Medical);

        session.RaiseEvent($"You take your mother into {clinicStatus.LocationName}. The visit costs {clinicStatus.VisitCost} LE. Her health improves by {healthChange}.");
        if (NarrativeSignalRules.HasPendingClinicFirstVisit(session.StoryFlags.ToHashSet()))
        {
            session.TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.MotherClinicFirstVisit, NarrativeKnots.MotherClinicFirstVisit));
        }

        session.RecordMutation(MutationCategories.Clinic, "TakeMotherToClinic", before, session.CaptureStats(), $"Clinic visit at {clinicStatus.LocationName} (cost {clinicStatus.VisitCost} LE, health +{healthChange})");
        session.AdvanceTime(clinicVisitMinutes);
        return new MotherClinicVisitResult(true, clinicStatus.VisitCost, healthChange);
    }

    internal static CurrentLocationClinicStatus GetCurrentLocationClinicStatus(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = session.World.GetCurrentLocation();
        var currentDay = session.GetCurrentDayOfWeek();
        var currentDayName = currentDay.ToString();

        if (location is null || !location.HasClinicServices)
        {
            return new CurrentLocationClinicStatus(
                HasClinicServices: false,
                IsOpenToday: false,
                VisitCost: 0,
                LocationName: location?.Name ?? "Unknown",
                CurrentDayName: currentDayName,
                OpenDaysSummary: "No clinic here");
        }

        return new CurrentLocationClinicStatus(
            HasClinicServices: true,
            IsOpenToday: location.ClinicOpenDays.Contains(currentDay.ToSystemDayOfWeek()),
            VisitCost: GetClinicVisitCost(session, location),
            LocationName: location.Name,
            CurrentDayName: currentDayName,
            OpenDaysSummary: FormatOpenDays(location.ClinicOpenDays));
    }

    internal static IReadOnlyList<Location> GetClinicLocations(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return WorldState.AllLocations
            .Where(location => location.HasClinicServices)
            .ToList();
    }

    internal static ClinicTravelOption GetClinicTravelOption(GameSession session, LocationId clinicLocationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == clinicLocationId);
        if (location is null || !location.HasClinicServices)
        {
            return new ClinicTravelOption(
                LocationId: clinicLocationId,
                LocationName: "Unknown",
                DistrictName: "Unknown",
                TravelCost: 0,
                ClinicCost: 0,
                TotalCost: 0,
                IsOpenToday: false,
                OpenDaysSummary: "No clinic at this location",
                TravelTimeMinutes: 0,
                CanAfford: false,
                IsValidOption: false);
        }

        var travelCost = TravelService.GetTravelCost(session, location);
        var clinicCost = GetClinicVisitCost(session, location);
        var totalCost = travelCost + clinicCost;
        var currentDay = session.GetCurrentDayOfWeek();
        var travelBlocked = WeatherActivityRules.BlocksTravelTo(session.CurrentWeather, location.District);
        return new ClinicTravelOption(
            LocationId: clinicLocationId,
            LocationName: location.Name,
            DistrictName: location.District.ToString(),
            TravelCost: travelCost,
            ClinicCost: clinicCost,
            TotalCost: totalCost,
            IsOpenToday: location.ClinicOpenDays.Contains(currentDay.ToSystemDayOfWeek()),
            OpenDaysSummary: FormatOpenDays(location.ClinicOpenDays),
            TravelTimeMinutes: TravelService.GetTravelTimeMinutes(session, location),
            CanAfford: session.Player.Stats.Money >= totalCost,
            IsValidOption: !travelBlocked);
    }

    internal static TravelAndClinicVisitResult TravelAndTakeMotherToClinic(GameSession session, LocationId clinicLocationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var clinicLocation = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == clinicLocationId);
        if (clinicLocation is not null && WeatherActivityRules.BlocksTravelTo(session.CurrentWeather, clinicLocation.District))
        {
            var reason = WeatherActivityRules.GetTravelBlockReason(session.CurrentWeather, clinicLocation.District);
            session.RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, session.CaptureStats(), reason);
            session.RaiseEvent(reason);
            return new TravelAndClinicVisitResult(false, 0, 0, 0, 0);
        }

        var option = GetClinicTravelOption(session, clinicLocationId);
        if (!option.IsValidOption)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, session.CaptureStats(), "No clinic at that location");
            session.RaiseEvent("There is no clinic service at that location.");
            return new TravelAndClinicVisitResult(false, 0, 0, 0, 0);
        }

        if (!option.IsOpenToday)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, session.CaptureStats(), $"{option.LocationName} closed today");
            session.RaiseEvent($"{option.LocationName} is closed today. Open days: {option.OpenDaysSummary}.");
            return new TravelAndClinicVisitResult(false, option.TravelCost, option.ClinicCost, option.TotalCost, 0);
        }

        if (session.Player.Stats.Money < option.TotalCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, session.CaptureStats(), $"Not enough money (need {option.TotalCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. Travel + clinic visit costs {option.TotalCost} LE ({option.TravelCost} LE travel + {option.ClinicCost} LE clinic).");
            return new TravelAndClinicVisitResult(false, option.TravelCost, option.ClinicCost, option.TotalCost, 0);
        }

        var travelEnergyCost = TravelService.GetTravelEnergyCost(session, clinicLocation!);
        session.Player.Stats.ModifyMoney(-option.TravelCost);
        session.Player.Stats.ModifyEnergy(-travelEnergyCost);
        TravelService.ApplyCargoMuleWear(session);
        session.AdvanceTime(option.TravelTimeMinutes);
        session.World.TravelTo(clinicLocationId);

        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && clinicLocation!.District == DistrictId.Dokki)
        {
            session.Player.Stats.ModifyStress(2);
            session.RaiseEvent("Dokki's questions land harder when your accent gets there before your name does.");
        }

        session.RaiseEvent($"Traveled to {option.LocationName} with your mother.");
        var repairDrone = session.Player.Robotics.Robots.FirstOrDefault(robot => robot.Type == RobotType.RepairDrone && robot.IsOperational);
        if (repairDrone is not null)
        {
            repairDrone.Damage(RobotCapabilityRules.ClinicWear);
            session.RaiseEvent($"The Repair Drone's triage reader takes {RobotCapabilityRules.ClinicWear} condition wear. Condition: {repairDrone.Condition}%.");
        }

        var clinicResult = TakeMotherToClinic(session);
        session.RecordMutation(MutationCategories.Clinic, "TravelAndTakeMotherToClinic", before, session.CaptureStats(), $"Travel+clinic to {option.LocationName} (total cost {option.TravelCost + clinicResult.TotalCost} LE)");
        return new TravelAndClinicVisitResult(
            clinicResult.Success,
            option.TravelCost,
            clinicResult.TotalCost,
            option.TravelCost + clinicResult.TotalCost,
            clinicResult.HealthChange);
    }

    internal static int GetClinicVisitCost(GameSession session, Location location)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(location);
        var districtCondition = session.GetActiveDistrictConditionDefinition(location.District);
        var schedule = session.GetCurrentSchedule();
        var scheduleDiscount = schedule.ClinicDiscount ? schedule.ClinicDiscountAmount : 0;
        if (scheduleDiscount > 0 && session.Player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            scheduleDiscount *= 2;
        }

        var modifiedCost = session.LocationPricing.GetClinicVisitCost(location, session.Relationships, session.Player.Skills)
            + (districtCondition?.Effect.ClinicVisitCostModifier ?? 0)
            - scheduleDiscount
            - RobotCapabilityRules.GetClinicCostReduction(session.Player.Robotics);
        return Math.Max(1, modifiedCost);
    }

    private static string GetMotherStatusMessage(GameSession session)
    {
        return session.Player.Household.MotherCondition switch
        {
            MotherCondition.Stable => "Your mother seems stable today.",
            MotherCondition.Fragile => "Your mother looks fragile and needs attention.",
            MotherCondition.Crisis => "Your mother is in crisis. She needs care immediately.",
            _ => "You check on your mother."
        };
    }

    private static string FormatOpenDays(IEnumerable<DayOfWeek> openDays)
    {
        return string.Join(", ", openDays.Select(static day => day.ToString()[..3]));
    }
}
