using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Core.World.News;

namespace Slums.Core.World;

/// <summary>Applies paid and walking travel rules while keeping session state canonical.</summary>
internal static class TravelService
{
    internal static bool TryTravelTo(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        if (location is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, session.CaptureStats(), $"Location {locationId} not found");
            return false;
        }

        if (session.World.CurrentLocationId == locationId)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, session.CaptureStats(), $"Already at {location.Name}");
            session.RaiseEvent($"You are already at {location.Name}.");
            return false;
        }

        if (WeatherActivityRules.BlocksTravelTo(session.CurrentWeather, location.District))
        {
            var reason = WeatherActivityRules.GetTravelBlockReason(session.CurrentWeather, location.District);
            session.RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, session.CaptureStats(), reason);
            session.RaiseEvent(reason);
            return false;
        }

        var travelCost = GetTravelCost(session, location);
        var travelEnergyCost = GetTravelEnergyCost(session, location);
        var travelTimeMinutes = GetTravelTimeMinutes(session, location);

        if (session.Player.Stats.Money < travelCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, session.CaptureStats(), $"Not enough money (need {travelCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent("Not enough money for transport.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-travelCost);
        session.Player.Stats.ModifyEnergy(-travelEnergyCost);
        ApplyCargoMuleWear(session);
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && location.District == DistrictId.Dokki)
        {
            session.Player.Stats.ModifyStress(2);
            session.RaiseEvent("Dokki's questions land harder when your accent gets there before your name does.");
        }

        if (location.District == DistrictId.BulaqAlDakrour && session.Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 12)
        {
            session.RaiseEvent("Safaa's route advice spares you one bad transfer and some wasted motion.");
        }

        if (location.District == DistrictId.Shubra && session.Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 12)
        {
            session.Player.Stats.ModifyStress(-1);
            session.RaiseEvent("Iman's directions keep you off the most exhausting side streets in Shubra.");
        }

        session.World.TravelTo(locationId);

        session.RaiseEvent($"Traveled to {location.Name}.");
        session.RecordMutation(MutationCategories.Travel, "TryTravelTo", before, session.CaptureStats(), $"Traveled to {location.Name} (cost {travelCost} LE)");
        session.AdvanceTime(travelTimeMinutes);
        return true;
    }

    internal static bool TryWalkTo(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        if (location is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, session.CaptureStats(), $"Location {locationId} not found");
            return false;
        }

        if (session.World.CurrentLocationId == locationId)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, session.CaptureStats(), $"Already at {location.Name}");
            session.RaiseEvent($"You are already at {location.Name}.");
            return false;
        }

        if (WeatherActivityRules.BlocksTravelTo(session.CurrentWeather, location.District))
        {
            var reason = WeatherActivityRules.GetTravelBlockReason(session.CurrentWeather, location.District);
            session.RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, session.CaptureStats(), reason);
            session.RaiseEvent(reason);
            return false;
        }

        var walkEnergyCost = GetWalkEnergyCost(session, location);
        var walkTimeMinutes = GetWalkTimeMinutes(session, location);

        if (session.Player.Stats.Energy < walkEnergyCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, session.CaptureStats(), $"Too exhausted (need {walkEnergyCost} energy, have {session.Player.Stats.Energy})");
            session.RaiseEvent("You are too exhausted to walk that far.");
            return false;
        }

        session.Player.Stats.ModifyEnergy(-walkEnergyCost);
        session.Player.Stats.ModifyStress(3);

        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && location.District == DistrictId.Dokki)
        {
            session.Player.Stats.ModifyStress(2);
            session.RaiseEvent("Dokki's stares follow you the entire way on foot.");
        }

        session.World.TravelTo(locationId);

        session.RaiseEvent($"Walked to {location.Name}. The streets took their toll.");
        session.RecordMutation(MutationCategories.Travel, "TryWalkTo", before, session.CaptureStats(), $"Walked to {location.Name}");
        session.AdvanceTime(walkTimeMinutes);
        return true;
    }

    internal static bool CanAfford(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is not null && session.Player.Stats.Money >= GetTravelCost(session, location);
    }

    internal static int GetTravelCost(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is null ? 0 : GetTravelCost(session, location);
    }

    internal static int GetTravelTimeMinutes(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is null ? 0 : GetTravelTimeMinutes(session, location);
    }

    internal static int GetWalkTimeMinutes(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is null ? 0 : GetWalkTimeMinutes(session, location);
    }

    internal static string? GetTravelConditionSummary(GameSession session, LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        if (location is null)
        {
            return null;
        }

        if (WeatherActivityRules.BlocksTravelTo(session.CurrentWeather, location.District))
        {
            return WeatherActivityRules.GetTravelBlockReason(session.CurrentWeather, location.District);
        }

        var summaries = new List<string>();
        if (session.CurrentWeather.TravelCostModifier != 0)
        {
            summaries.Add($"{WeatherModifiers.GetDisplayName(session.CurrentWeather.Type)} weather adds {session.CurrentWeather.TravelCostModifier} LE to transport.");
        }

        var infrastructureTravel = InfrastructureImpactCalculator.GetTravelCostModifier(session.Infrastructure, location.District);
        var newsTravel = NewsImpactCalculator.GetTravelCostModifier(session.News, location.District);
        if (infrastructureTravel != 0)
        {
            summaries.Add($"Transport service pressure adds {infrastructureTravel} LE and time to this trip.");
        }
        if (newsTravel != 0)
        {
            summaries.Add($"City news adds {newsTravel} LE to fares in this area.");
        }

        var districtCondition = session.GetActiveDistrictConditionDefinition(location.District);
        if (districtCondition is not null)
        {
            var effect = districtCondition.Effect;
            if (effect.TravelCostModifier != 0 || effect.TravelTimeMinutesModifier != 0 || effect.TravelEnergyModifier != 0)
            {
                summaries.Add($"{districtCondition.Title}: {districtCondition.GameplaySummary}");
            }
        }

        return summaries.Count == 0 ? null : string.Join(" ", summaries);
    }

    internal static int GetTravelCost(GameSession session, Location destination)
    {
        var districtCondition = session.GetActiveDistrictConditionDefinition(destination.District);
        var modifiedCost = session.LocationPricing.GetTravelCost(destination, session.Relationships)
            + (districtCondition?.Effect.TravelCostModifier ?? 0)
            + session.CurrentWeather.TravelCostModifier
            + InfrastructureImpactCalculator.GetTravelCostModifier(session.Infrastructure, destination.District)
            + NewsImpactCalculator.GetTravelCostModifier(session.News, destination.District);
        return Math.Max(1, modifiedCost);
    }

    private static int GetWalkEnergyCost(GameSession session, Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return GetTravelEnergyCost(session, destination) * 3;
    }

    private static int GetWalkTimeMinutes(GameSession session, Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return GetTravelTimeMinutes(session, destination) * 3;
    }

    internal static int GetTravelEnergyCost(GameSession session, Location destination)
    {
        var districtCondition = session.GetActiveDistrictConditionDefinition(destination.District);
        var modifiedCost = session.LocationPricing.GetTravelEnergyCost(destination, session.Relationships)
            + (districtCondition?.Effect.TravelEnergyModifier ?? 0)
            - RobotCapabilityRules.GetTransitEnergyReduction(session.Player.Robotics);
        return Math.Max(1, modifiedCost);
    }

    internal static int GetTravelTimeMinutes(GameSession session, Location destination)
    {
        var districtCondition = session.GetActiveDistrictConditionDefinition(destination.District);
        var modifiedMinutes = destination.TravelTimeMinutes
            + (districtCondition?.Effect.TravelTimeMinutesModifier ?? 0)
            + InfrastructureImpactCalculator.GetTravelTimeModifier(session.Infrastructure, destination.District);
        return Math.Max(1, modifiedMinutes);
    }

    internal static void ApplyCargoMuleWear(GameSession session)
    {
        var cargoMule = session.Player.Robotics.Robots.FirstOrDefault(robot => robot.Type == RobotType.CargoMule && robot.IsOperational);
        if (cargoMule is null)
        {
            return;
        }

        cargoMule.Damage(RobotCapabilityRules.TransitWear);
        session.RaiseEvent($"The Cargo Mule takes {RobotCapabilityRules.TransitWear} condition wear on the route. Condition: {cargoMule.Condition}%.");
    }
}
