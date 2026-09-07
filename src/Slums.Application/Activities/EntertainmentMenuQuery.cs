namespace Slums.Application.Activities;

public sealed class EntertainmentMenuQuery
{
    public IReadOnlyList<EntertainmentMenuStatus> GetStatuses(EntertainmentMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context
            .Activities
            .Select(activity =>
            {
                var canAfford = context.Player.Stats.Money >= activity.BaseCost;
                var hasEnergy = context.Player.Stats.Energy >= activity.EnergyCost;
                var canPerform = canAfford && hasEnergy;

                string? reason = null;
                if (!canAfford && !hasEnergy)
                {
                    reason = "Not enough money or energy.";
                }
                else if (!canAfford)
                {
                    reason = $"Costs {activity.BaseCost} LE. You have {context.Player.Stats.Money} LE.";
                }
                else if (!hasEnergy)
                {
                    reason = $"Requires {activity.EnergyCost} energy. You have {context.Player.Stats.Energy}.";
                }

                return new EntertainmentMenuStatus(
                    activity,
                    canAfford,
                    hasEnergy,
                    canPerform,
                    reason);
            })
            .ToArray();
    }
}
