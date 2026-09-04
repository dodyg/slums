using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Phone;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Core.Information;

/// <summary>Applies tip generation, delivery, acknowledgement, ignore erosion, and restore rules.</summary>
internal static class TipService
{
    internal static void ProcessDaily(GameSession session, Random random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(random);
        var newTips = TipGenerator.GenerateTips(
            session.Clock.Day,
            session.Relationships,
            session.DistrictHeat,
            session.NpcEconomies,
            session.Player.BackgroundType,
            session.CrimesCommitted,
            session.Relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud).Trust,
            random);

        foreach (var tip in newTips)
        {
            session.Tips.AddTip(tip);
            var deliveryMethod = TipDeliveryConfig.GetDeliveryMethod(tip, session.World.CurrentDistrict);
            if (deliveryMethod == TipDeliveryMethod.Phone || deliveryMethod == TipDeliveryMethod.Emergency)
            {
                if (session.Phone.IsOperational())
                {
                    session.PhoneMessages.AddMessage(new PhoneMessage
                    {
                        Type = PhoneMessageType.Tip,
                        Sender = NpcRegistry.GetName(tip.Source),
                        SenderNpcId = tip.Source.ToString(),
                        Content = tip.Content,
                        DayReceived = tip.DayGenerated,
                        ExpiresAfterDay = tip.ExpiresAfterDay,
                        RequiresResponse = false,
                        ResponseTimeCost = 0,
                        ResponseMoneyCost = 0
                    });
                    session.Tips.MarkAsDelivered(tip.Id);
                }
            }
        }

        ApplyIgnoreErosion(session);
        var removed = session.Tips.RemoveExpired(session.Clock.Day);
        if (newTips.Count > 0 || removed > 0)
        {
            var before = session.CaptureStats();
            session.RecordMutation(MutationCategories.Information, "ProcessDailyTips", before, session.CaptureStats(), $"Generated {newTips.Count} tip(s), expired {removed}");
        }
    }

    internal static (bool Success, string Message) Acknowledge(GameSession session, string tipId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var tip = session.Tips.GetTip(tipId);
        if (tip is null)
        {
            return (false, "Tip not found.");
        }

        if (tip.Acknowledged)
        {
            return (false, "Already acknowledged.");
        }

        if (tip.Ignored)
        {
            return (false, "Tip was ignored.");
        }

        var before = session.CaptureStats();
        session.Tips.AcknowledgeTip(tipId);
        session.RecordMutation(MutationCategories.Information, "AcknowledgeTip", before, session.CaptureStats(), $"Acknowledged tip from {NpcRegistry.GetName(tip.Source)}: {tip.Content}");
        return (true, $"Acknowledged tip from {NpcRegistry.GetName(tip.Source)}.");
    }

    internal static (bool Success, string Message, int TrustLoss) Ignore(GameSession session, string tipId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var tip = session.Tips.GetTip(tipId);
        if (tip is null)
        {
            return (false, "Tip not found.", 0);
        }

        if (tip.Acknowledged || tip.Ignored)
        {
            return (false, "Tip already handled.", 0);
        }

        var before = session.CaptureStats();
        var ignoreCount = session.Tips.IgnoreTip(tipId);
        var trustLoss = 0;
        var trust = session.Relationships.GetNpcRelationship(tip.Source).Trust;
        if (ContactErosionRule.ShouldErode(trust, ignoreCount))
        {
            trustLoss = 1;
            session.Relationships.ModifyNpcTrust(tip.Source, -trustLoss);
        }

        session.RecordMutation(MutationCategories.Information, "IgnoreTip", before, session.CaptureStats(), $"Ignored tip from {NpcRegistry.GetName(tip.Source)}");
        return (true, $"Ignored tip from {NpcRegistry.GetName(tip.Source)}.", trustLoss);
    }

    internal static void Restore(GameSession session, IEnumerable<Tip> tips, Dictionary<NpcId, int> ignoredCounts)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(tips);
        ArgumentNullException.ThrowIfNull(ignoredCounts);
        session.Tips.RestoreTips(tips, ignoredCounts);
    }

    private static void ApplyIgnoreErosion(GameSession session)
    {
        foreach (NpcId npc in Enum.GetValues<NpcId>())
        {
            var ignoredCount = session.Tips.GetIgnoredCount(npc);
            var trust = session.Relationships.GetNpcRelationship(npc).Trust;
            if (!ContactErosionRule.ShouldErode(trust, ignoredCount))
            {
                continue;
            }

            session.Relationships.ModifyNpcTrust(npc, -1);
            session.RaiseEvent($"{NpcRegistry.GetName(npc)} seems annoyed that you keep ignoring their advice. Trust -1.");
        }
    }
}
