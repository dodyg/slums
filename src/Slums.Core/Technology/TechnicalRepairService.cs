using Slums.Core.Diagnostics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Technology;

internal static class TechnicalRepairService
{
    internal static IReadOnlyList<TechnicalRepairPreview> GetPreviews(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return TechnicalRepairRegistry.All.Select(action => Preview(session, action.Type)).ToArray();
    }

    internal static TechnicalRepairPreview Preview(GameSession session, TechnicalRepairActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var action = TechnicalRepairRegistry.Get(actionType);
        var skillLevel = session.Player.Skills.GetLevel(SkillId.RobotRepair);
        var atRequiredLocation = session.World.CurrentLocationId == action.RequiredLocation;
        var hasSkill = skillLevel >= action.RequiredSkillLevel;
        var hasTime = session.CanCompleteActivityToday(action.TimeCostMinutes);
        var canAfford = session.Player.Stats.Money >= action.MoneyCost;
        var hasEnergy = session.Player.Stats.Energy >= action.EnergyCost;
        var hasParts = session.Player.Robotics.Parts >= action.PartsRequired;
        var currentCondition = actionType == TechnicalRepairActionType.RepairHandset
            ? session.Phone.HandsetCondition
            : session.Technology.MicrogridStorageCondition;
        var needsRepair = actionType == TechnicalRepairActionType.TakeRepairBenchContract || currentCondition < 100;
        var conditionGain = TechnicalRepairCalculator.GetConditionGain(actionType, skillLevel);
        var income = actionType == TechnicalRepairActionType.TakeRepairBenchContract
            ? TechnicalRepairCalculator.GetContractIncome(skillLevel)
            : 0;
        var canPerform = atRequiredLocation && hasSkill && hasTime && canAfford && hasEnergy && hasParts && needsRepair;
        var reason = GetReason(atRequiredLocation, hasSkill, hasTime, canAfford, hasEnergy, hasParts, needsRepair, action);
        return new TechnicalRepairPreview(action, atRequiredLocation, hasSkill, hasTime, canAfford, hasEnergy, hasParts, needsRepair, currentCondition, conditionGain, income, canPerform, reason);
    }

    internal static bool Perform(GameSession session, TechnicalRepairActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var preview = Preview(session, actionType);
        var before = session.CaptureStats();
        if (!preview.CanPerform)
        {
            var reason = preview.UnavailabilityReason ?? "That repair is not available.";
            session.RaiseEvent(reason);
            session.RecordMutation(MutationCategories.GuardRejected, actionType.ToString(), before, session.CaptureStats(), reason);
            return false;
        }

        var action = preview.Action;
        session.Player.Stats.ModifyMoney(-action.MoneyCost);
        session.Player.Stats.ModifyEnergy(-action.EnergyCost);
        session.Player.Robotics.TryConsumeParts(action.PartsRequired);
        switch (actionType)
        {
            case TechnicalRepairActionType.RepairHandset:
                session.Phone.RepairHandset(preview.ConditionGain);
                session.Technology.RecordHandsetUse();
                session.RaiseEvent($"You reseal the handset to {session.Phone.HandsetCondition}% condition. The wallet still asks questions, but the battery holds.");
                break;
            case TechnicalRepairActionType.RestoreSolarStorage:
                session.Technology.RepairMicrogridStorage(preview.ConditionGain);
                session.Infrastructure.ReduceDisruption(session.World.CurrentDistrict, InfrastructureServiceType.Electricity, 1);
                session.RaiseEvent($"You restore the cooperative storage bank to {session.Technology.MicrogridStorageCondition}% condition. The next outage will still need people on the roof.");
                break;
            case TechnicalRepairActionType.TakeRepairBenchContract:
                session.Player.Stats.ModifyMoney(preview.Income);
                session.Technology.RecordHandsetUse();
                session.RaiseEvent($"You finish a relay repair for the local cooperative and earn {preview.Income} LE. Two of your own spare parts are gone.");
                break;
        }

        session.RecordMutation(MutationCategories.Technology, actionType.ToString(), before, session.CaptureStats(), $"Completed {action.Name} with Technical Repair {session.Player.Skills.GetLevel(SkillId.RobotRepair)}");
        session.AdvanceTime(action.TimeCostMinutes);
        return true;
    }

    private static string? GetReason(bool atRequiredLocation, bool hasSkill, bool hasTime, bool canAfford, bool hasEnergy, bool hasParts, bool needsRepair, TechnicalRepairActionDefinition action)
    {
        if (!atRequiredLocation)
        {
            return $"Requires {action.RequiredLocation.Value}.";
        }
        if (!hasSkill)
        {
            return $"Reach Technical Repair {action.RequiredSkillLevel}.";
        }
        if (!needsRepair)
        {
            return "That device is already at full condition.";
        }
        if (!hasParts)
        {
            return $"Requires {action.PartsRequired} spare parts.";
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
