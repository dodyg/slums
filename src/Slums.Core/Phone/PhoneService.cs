using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;

namespace Slums.Core.Phone;

/// <summary>Applies phone credit, daily message, response, replacement, and restore rules.</summary>
internal static class PhoneService
{
    internal static void ProcessDaily(GameSession session, Random random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(random);
        if (!session.Phone.IsOperational())
        {
            session.Phone.DailyCreditDrain();
            session.PhoneMessages.MarkPendingAsMissed();
            return;
        }

        session.Phone.DailyCreditDrain();
        var newMessages = PhoneMessageGenerator.GenerateMessages(
            session.Clock.Day,
            session.Relationships,
            session.PolicePressure,
            session.Player.Household.MotherHealth,
            session.DistrictHeat,
            session.Player.BackgroundType,
            random);

        foreach (var message in newMessages)
        {
            session.PhoneMessages.AddMessage(message);
        }

        session.PhoneMessages.RemoveExpired(session.Clock.Day);
        if (newMessages.Count > 0)
        {
            var before = session.CaptureStats();
            session.RecordMutation(MutationCategories.Phone, "ProcessDailyPhone", before, session.CaptureStats(), $"Received {newMessages.Count} message(s)");
        }
    }

    internal static (bool Success, string Message) RefillCredit(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.Phone.IsOperational() && !session.Phone.HasPhone)
        {
            return (false, "You don't have a phone.");
        }

        if (session.Phone.PhoneLost)
        {
            return (false, "Your phone is lost.");
        }

        var refillCost = DigitalLiteracyCalculator.GetCreditRefillCost(
            session.Player.Skills.GetLevel(SkillId.CyberHacking),
            session.Phone.CreditWeekCost);
        var digitalSkillLevel = session.Player.Skills.GetLevel(SkillId.CyberHacking);
        if (session.Player.Stats.Money < refillCost)
        {
            return (false, $"Not enough money (need {refillCost} LE, have {session.Player.Stats.Money} LE).");
        }

        var before = session.CaptureStats();
        session.Player.Stats.ModifyMoney(-refillCost);
        session.Phone.RefillCredit();
        session.Technology.RecordHandsetUse();
        session.PhoneMessages.DeliverMissedMessages();
        var mutationReason = digitalSkillLevel == 0
            ? $"Refilled phone credit for {session.Phone.CreditWeekCost} LE"
            : $"Refilled phone credit for {refillCost} LE with Digital Literacy {digitalSkillLevel}";
        session.RecordMutation(MutationCategories.Phone, "RefillPhoneCredit", before, session.CaptureStats(), mutationReason);
        var message = digitalSkillLevel == 0
            ? "Phone credit refilled for 7 days."
            : $"Phone credit refilled for 7 days ({refillCost} LE).";
        return (true, message);
    }

    internal static (bool Success, string Message) RespondToMessage(GameSession session, string messageId)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.Phone.IsOperational())
        {
            return (false, "Phone is not operational.");
        }

        var message = session.PhoneMessages.GetMessage(messageId);
        if (message is null)
        {
            return (false, "Message not found.");
        }

        if (message.Responded)
        {
            return (false, "Already responded to this message.");
        }

        if (message.Ignored)
        {
            return (false, "Message was ignored.");
        }

        if (message.IsExpired(session.Clock.Day))
        {
            return (false, "Message has expired.");
        }

        var missedCallCost = message.WasMissed ? 1 : 0;
        var totalMoneyCost = missedCallCost + message.ResponseMoneyCost;
        if (session.Player.Stats.Money < totalMoneyCost)
        {
            return message.WasMissed && message.ResponseMoneyCost == 0
                ? (false, "Not enough money to return this missed call (1 LE).")
                : (false, $"Not enough money (need {totalMoneyCost} LE).");
        }

        var responseTimeMinutes = message.ResponseTimeCost * 60;
        if (!session.CanCompleteActivityToday(responseTimeMinutes))
        {
            return (false, "Not enough time to respond today.");
        }

        var before = session.CaptureStats();
        if (totalMoneyCost > 0)
        {
            session.Player.Stats.ModifyMoney(-totalMoneyCost);
        }

        session.PhoneMessages.RespondToMessage(messageId);
        ApplyResponseEffects(session, message);
        session.RecordMutation(MutationCategories.Phone, "RespondToMessage", before, session.CaptureStats(), $"Responded to message from {message.Sender}: {message.Content}");
        if (responseTimeMinutes > 0)
        {
            session.AdvanceTime(responseTimeMinutes);
        }

        return (true, $"Responded to {message.Sender}.");
    }

    internal static (bool Success, string Message, int TrustLoss) IgnoreMessage(GameSession session, string messageId)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.Phone.IsOperational())
        {
            return (false, "Phone is not operational.", 0);
        }

        var message = session.PhoneMessages.GetMessage(messageId);
        if (message is null)
        {
            return (false, "Message not found.", 0);
        }

        if (message.Responded || message.Ignored)
        {
            return (false, "Message already handled.", 0);
        }

        var before = session.CaptureStats();
        var ignoreCount = session.PhoneMessages.IgnoreMessage(messageId);
        var trustLoss = 0;
        if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
        {
            var trust = session.Relationships.GetNpcRelationship(npc).Trust;
            if (ContactErosionRule.ShouldErode(trust, ignoreCount))
            {
                trustLoss = 1;
                session.Relationships.ModifyNpcTrust(npc, -trustLoss);
            }
        }

        session.RecordMutation(MutationCategories.Phone, "IgnoreMessage", before, session.CaptureStats(), $"Ignored message from {message.Sender}");
        return (true, $"Ignored message from {message.Sender}.", trustLoss);
    }

    internal static (bool Success, string Message) ReplacePhone(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!session.Phone.PhoneLost)
        {
            return (false, "Your phone is not lost.");
        }

        const int replacementCost = PhoneState.ReplacementCost;
        if (session.Player.Stats.Money < replacementCost)
        {
            return (false, $"Not enough money (need {replacementCost} LE for replacement + credit).");
        }

        var before = session.CaptureStats();
        session.Player.Stats.ModifyMoney(-replacementCost);
        session.Phone.ReplacePhone();
        session.PhoneMessages.DeliverMissedMessages();
        session.RecordMutation(MutationCategories.Phone, "ReplacePhone", before, session.CaptureStats(), $"Replaced phone for {replacementCost} LE");
        return (true, "New phone purchased. Credit refilled for 7 days.");
    }

    internal static void RestoreState(GameSession session, bool hasPhone, int creditRemaining, int daysSinceCreditRefill, bool phoneLost, int? phoneLostDay, bool phoneRecovered, int handsetCondition = 65)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Phone.Restore(hasPhone, creditRemaining, daysSinceCreditRefill, phoneLost, phoneLostDay, phoneRecovered, handsetCondition);
    }

    internal static void RestoreMessages(GameSession session, IEnumerable<PhoneMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(messages);
        session.PhoneMessages.RestoreMessages(messages);
    }

    private static void ApplyResponseEffects(GameSession session, PhoneMessage message)
    {
        switch (message.Type)
        {
            case PhoneMessageType.Opportunity:
            case PhoneMessageType.NetworkRequest:
            {
                if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
                {
                    session.Relationships.RecordFavor(npc, session.Clock.Day);
                }

                break;
            }
            case PhoneMessageType.Warning:
                session.Player.Stats.ModifyStress(-3);
                break;
            case PhoneMessageType.FamilyAlert:
                session.RaiseEvent("You check on your mother after Mona's message.");
                break;
            case PhoneMessageType.Background:
            {
                if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
                {
                    session.Relationships.ModifyNpcTrust(npc, 1);
                }

                break;
            }
        }
    }
}
