using Slums.Core.Diagnostics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.Weather;
using Slums.Core.World;

namespace Slums.Core.Community;

/// <summary>Applies transparent, supply-limited neighborhood adaptation actions.</summary>
internal static class CommunityOrganizingService
{
    internal static IReadOnlyList<CommunityActionPreview> GetPreviews(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return CommunityActionRegistry.All.Select(action => Preview(session, action.Type)).ToArray();
    }

    internal static CommunityActionPreview Preview(GameSession session, CommunityActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var action = CommunityActionRegistry.Get(actionType);
        var hasSkill = session.Player.Skills.GetLevel(SkillId.CommunityOrganizing) >= action.RequiredSkillLevel;
        var hasTime = session.CanCompleteActivityToday(action.TimeCostMinutes);
        var canAfford = session.Player.Stats.Money >= action.MoneyCost;
        var hasEnergy = session.Player.Stats.Energy >= action.EnergyCost;
        var hasSupplies = actionType != CommunityActionType.OrganizeWaterRationing || session.Player.Household.FoodStockpile > 0;
        var hasPressureNeed = actionType != CommunityActionType.NeighborhoodPressureResponse
            || session.CurrentWeather.Type == WeatherType.Heatwave
            || session.Territory.GetControl(session.World.CurrentDistrict).Tension >= 30;
        var hasParticipation = actionType == CommunityActionType.NeighborhoodPressureResponse
            ? session.EventAttendance.TotalAttended >= 2
            : session.EventAttendance.TotalAttended >= 1;
        var isAtHome = session.World.CurrentLocationId == LocationId.Home;
        var canPerform = isAtHome && hasSkill && hasTime && canAfford && hasEnergy && hasSupplies && hasPressureNeed && hasParticipation;
        var reason = GetReason(isAtHome, hasSkill, hasTime, canAfford, hasEnergy, hasSupplies, hasPressureNeed, hasParticipation, action);
        return new CommunityActionPreview(action, isAtHome, hasSkill, hasTime, canAfford, hasEnergy, hasSupplies, hasPressureNeed, hasParticipation, canPerform, reason);
    }

    internal static bool Perform(GameSession session, CommunityActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var preview = Preview(session, actionType);
        var before = session.CaptureStats();
        if (!preview.CanPerform)
        {
            session.RaiseEvent(preview.UnavailabilityReason ?? "That community action is not available.");
            session.RecordMutation(MutationCategories.GuardRejected, actionType.ToString(), before, session.CaptureStats(), preview.UnavailabilityReason ?? "Action unavailable");
            return false;
        }

        var action = preview.Action;
        session.Player.Stats.ModifyMoney(-action.MoneyCost);
        session.Player.Stats.ModifyEnergy(-action.EnergyCost);
        switch (actionType)
        {
            case CommunityActionType.OrganizeWaterRationing:
                session.Player.Household.ConsumeFood();
                session.CommunityAdaptation.AddWaterReserve(2);
                session.Infrastructure.ReduceDisruption(session.World.CurrentDistrict, InfrastructureServiceType.Water, 1);
                session.RaiseEvent("The water committee posts a fairer rooftop schedule. Two reserve units are set aside for the block.");
                break;
            case CommunityActionType.CoordinateCoolingRoom:
                session.CommunityAdaptation.AddCoolingRoomDays(2);
                var coolingRelief = 4 + ComposureCalculator.GetCrisisStressRelief(
                    session.Player.Skills.GetLevel(SkillId.Composure),
                    4);
                session.Player.Stats.ModifyStress(-coolingRelief);
                session.RaiseEvent("The shared cooling room stays open for two more nights. It is patched shade, not a miracle.");
                break;
            case CommunityActionType.NeighborhoodPressureResponse:
                session.Territory.ModifyTension(session.World.CurrentDistrict, -5);
                session.DistrictHeat.AddHeat(session.World.CurrentDistrict, -2);
                session.CommunityAdaptation.RecordSuccessfulAction(2);
                var pressureRelief = 3 + ComposureCalculator.GetCrisisStressRelief(
                    session.Player.Skills.GetLevel(SkillId.Composure),
                    3);
                session.Player.Stats.ModifyStress(-pressureRelief);
                session.RaiseEvent("Neighbors coordinate watches and shade routes. Local tension eases, but the factions still own their grudges.");
                break;
        }

        if (actionType != CommunityActionType.NeighborhoodPressureResponse)
        {
            session.CommunityAdaptation.RecordSuccessfulAction(1);
        }

        session.RecordMutation(MutationCategories.Community, actionType.ToString(), before, session.CaptureStats(), $"Completed {action.Name} with Community Organizing {session.Player.Skills.GetLevel(SkillId.CommunityOrganizing)}");
        session.AdvanceTime(action.TimeCostMinutes);
        return true;
    }

    internal static void AdvanceDay(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.CommunityAdaptation.AdvanceDay();
    }

    private static string? GetReason(bool isAtHome, bool hasSkill, bool hasTime, bool canAfford, bool hasEnergy, bool hasSupplies, bool hasPressureNeed, bool hasParticipation, CommunityActionDefinition action)
    {
        if (!isAtHome)
        {
            return "Return home to coordinate shared resources.";
        }
        if (!hasSkill)
        {
            return $"Reach Community Organizing {action.RequiredSkillLevel}.";
        }
        if (!hasParticipation)
        {
            return "Attend at least one community event first.";
        }
        if (!hasSupplies)
        {
            return "Keep one food staple for the water committee's shared reserve.";
        }
        if (!hasPressureNeed)
        {
            return "Wait for a heatwave or elevated territory pressure before organizing a pressure response.";
        }
        if (!hasTime)
        {
            return $"Requires {action.TimeCostMinutes} minutes before 22:00.";
        }
        if (!canAfford)
        {
            return $"Requires {action.MoneyCost} LE.";
        }
        if (!hasEnergy)
        {
            return $"Requires {action.EnergyCost} energy.";
        }
        return null;
    }
}
