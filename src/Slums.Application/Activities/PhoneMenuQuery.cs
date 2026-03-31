using Slums.Core.Information;
using Slums.Core.Phone;

namespace Slums.Application.Activities;

public sealed class PhoneMenuQuery
{
#pragma warning disable CA1822
    public PhoneMenuStatus GetStatus(PhoneMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        var entries = new List<PhoneEntryDisplay>();

        foreach (var tip in context.UndeliveredTips)
        {
            entries.Add(new PhoneEntryDisplay(
                tip.Id,
                GetTipLabel(tip),
                tip.Content,
                GetTipTypeIcon(tip.Type),
                tip.IsEmergency,
                false,
                true,
                tip.ExpiresAfterDay - context.CurrentDay,
                tip.Source.ToString()));
        }

        foreach (var msg in context.ActiveMessages)
        {
            if (msg.Responded || msg.Ignored)
            {
                continue;
            }

            var isTipMessage = msg.Type == PhoneMessageType.Tip;
            entries.Add(new PhoneEntryDisplay(
                msg.Id,
                GetSenderLabel(msg),
                msg.Content,
                GetMessageTypeIcon(msg.Type),
                msg.Type == PhoneMessageType.Warning,
                msg.RequiresResponse,
                isTipMessage,
                msg.ExpiresAfterDay.HasValue ? msg.ExpiresAfterDay.Value - context.CurrentDay : (int?)null,
                msg.Sender));
        }

        return new PhoneMenuStatus(entries, context.CreditRemaining, context.CreditWeekCost, context.PhoneLost);
    }

    private static string GetTipLabel(Tip tip)
    {
        var prefix = tip.Type switch
        {
            TipType.PoliceTip => "Police intel",
            TipType.JobLead => "Job lead",
            TipType.MarketIntel => "Market intel",
            TipType.CrimeWarning => "Crime warning",
            TipType.PersonalWarning => "Personal warning",
            _ => "Tip"
        };

        return tip.IsEmergency ? $"[URGENT] {prefix}" : prefix;
    }

    private static string GetTipTypeIcon(TipType type) => type switch
    {
        TipType.PoliceTip => "[!]",
        TipType.JobLead => "[J]",
        TipType.MarketIntel => "[$]",
        TipType.CrimeWarning => "[X]",
        TipType.PersonalWarning => "[?]",
        _ => "[i]"
    };

    private static string GetSenderLabel(PhoneMessage msg) => msg.Type switch
    {
        PhoneMessageType.Tip => "Tip received",
        PhoneMessageType.Opportunity => $"Opportunity from {msg.Sender}",
        PhoneMessageType.Warning => $"Warning from {msg.Sender}",
        PhoneMessageType.FamilyAlert => "Family alert",
        PhoneMessageType.NetworkRequest => $"Request from {msg.Sender}",
        PhoneMessageType.Background => "Background intel",
        _ => msg.Sender
    };

    private static string GetMessageTypeIcon(PhoneMessageType type) => type switch
    {
        PhoneMessageType.Opportunity => "[O]",
        PhoneMessageType.Warning => "[!]",
        PhoneMessageType.FamilyAlert => "[F]",
        PhoneMessageType.NetworkRequest => "[N]",
        PhoneMessageType.Tip => "[i]",
        PhoneMessageType.Background => "[B]",
        _ => "[ ]"
    };
}

public sealed record PhoneMenuStatus(
    IReadOnlyList<PhoneEntryDisplay> Entries,
    int CreditRemaining,
    int CreditWeekCost,
    bool PhoneLost);

public sealed record PhoneEntryDisplay(
    string Id,
    string Label,
    string Content,
    string TypeIcon,
    bool IsEmergency,
    bool RequiresResponse,
    bool IsTip,
    int? DaysUntilExpiry,
    string SourceName);
