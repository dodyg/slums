using Slums.Core.Diagnostics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Technology;

internal static class DigitalServiceService
{
    internal static IReadOnlyList<DigitalServicePreview> GetPreviews(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return DigitalServiceRegistry.All.Select(action => Preview(session, action.Type)).ToArray();
    }

    internal static DigitalServicePreview Preview(GameSession session, DigitalServiceActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var action = DigitalServiceRegistry.Get(actionType);
        var skillLevel = session.Player.Skills.GetLevel(SkillId.CyberHacking);
        var atRequiredLocation = session.World.CurrentLocationId == action.RequiredLocation;
        var hasSkill = skillLevel >= action.RequiredSkillLevel;
        var hasOperationalPhone = session.Phone.IsOperational();
        var hasTime = session.CanCompleteActivityToday(action.TimeCostMinutes);
        var canAfford = session.Player.Stats.Money >= action.MoneyCost;
        var hasEnergy = session.Player.Stats.Energy >= action.EnergyCost;
        var noPendingAppeal = !session.Technology.BiometricAppealPending;
        var successChance = DigitalLiteracyCalculator.GetBiometricAppealSuccessChance(skillLevel);
        var canPerform = atRequiredLocation && hasSkill && hasOperationalPhone && hasTime && canAfford && hasEnergy && noPendingAppeal;
        var reason = GetReason(atRequiredLocation, hasSkill, hasOperationalPhone, hasTime, canAfford, hasEnergy, noPendingAppeal, action);
        return new DigitalServicePreview(action, atRequiredLocation, hasSkill, hasOperationalPhone, hasTime, canAfford, hasEnergy, noPendingAppeal, successChance, true, canPerform, reason);
    }

    internal static bool Perform(GameSession session, DigitalServiceActionType actionType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var preview = Preview(session, actionType);
        var before = session.CaptureStats();
        if (!preview.CanPerform)
        {
            var reason = preview.UnavailabilityReason ?? "That digital service is not available.";
            session.RaiseEvent(reason);
            session.RecordMutation(MutationCategories.GuardRejected, actionType.ToString(), before, session.CaptureStats(), reason);
            return false;
        }

        session.Player.Stats.ModifyMoney(-preview.Action.MoneyCost);
        session.Player.Stats.ModifyEnergy(-preview.Action.EnergyCost);
        session.Technology.RecordHandsetUse(2);
        session.Technology.RecordBiometricAppeal();
#pragma warning disable CA5394 // Gameplay uncertainty does not require cryptographic strength
        var succeeds = session.SharedRandom.Next(0, 100) < preview.SuccessChance;
#pragma warning restore CA5394
        if (succeeds)
        {
            session.Player.Stats.ModifyStress(-2);
            session.RaiseEvent("The biometric appeal submits and the record correction enters review. The institution keeps a note that you challenged it.");
        }
        else
        {
            session.Player.Stats.ModifyStress(3);
            session.RaiseEvent("The biometric appeal bounces on a mismatch. The review remains pending, and the institution now knows the handset challenged its record.");
        }

        session.RecordMutation(MutationCategories.Technology, actionType.ToString(), before, session.CaptureStats(), $"Submitted biometric appeal with Digital Literacy {session.Player.Skills.GetLevel(SkillId.CyberHacking)}; success chance {preview.SuccessChance}%");
        session.AdvanceTime(preview.Action.TimeCostMinutes);
        return true;
    }

    private static string? GetReason(bool atRequiredLocation, bool hasSkill, bool hasOperationalPhone, bool hasTime, bool canAfford, bool hasEnergy, bool noPendingAppeal, DigitalServiceActionDefinition action)
    {
        if (!atRequiredLocation)
        {
            return $"Requires {action.RequiredLocation.Value}.";
        }
        if (!hasSkill)
        {
            return $"Reach Digital Literacy {action.RequiredSkillLevel}.";
        }
        if (!hasOperationalPhone)
        {
            return "Requires an operational handset with active credit.";
        }
        if (!noPendingAppeal)
        {
            return "A biometric review is already pending.";
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
