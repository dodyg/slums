namespace Slums.Application.Activities;

public sealed class CommunityEventMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<CommunityEventMenuStatus> GetStatuses(CommunityEventMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return context
            .AvailableEvents
            .Select(evt =>
            {
                var canAfford = context.PlayerMoney >= evt.MoneyCost;
                var remainingMinutes = (context.EndOfDayHour * 60) - (context.CurrentHour * 60);
                var hasTime = remainingMinutes >= evt.TimeCostMinutes;
                var alreadyAttended = context.Attendance.AttendedThisWeek.Contains(evt.Id);
                var canAttend = canAfford && hasTime && !alreadyAttended;

                string? reason = null;
                if (alreadyAttended)
                {
                    reason = "Already attended this week.";
                }
                else if (!canAfford && !hasTime)
                {
                    reason = $"Not enough money ({evt.MoneyCost} LE) or time.";
                }
                else if (!canAfford)
                {
                    reason = $"Costs {evt.MoneyCost} LE. You have {context.PlayerMoney} LE.";
                }
                else if (!hasTime)
                {
                    reason = $"Takes {evt.TimeCostMinutes / 60}h. Not enough time today.";
                }

                return new CommunityEventMenuStatus(evt, canAfford, hasTime, alreadyAttended, canAttend, reason);
            })
            .ToArray();
    }
}
