using Slums.Core.Training;

namespace Slums.Application.Activities;

public sealed class TrainingMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<TrainingMenuStatus> GetStatuses(TrainingMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return context
            .Activities
            .Select(activity =>
            {
                var canAfford = context.Money >= activity.MoneyCost;
                var hasEnergy = context.Energy >= activity.EnergyCost;
                var rightTime = context.Hour >= 18 && context.Hour < 22;
                var notTrainedToday = !context.TrainedToday.ContainsKey(activity.Skill);
                var notAtCap = true;
                var npcTrustMet = activity.RequiredNpc is null || true;

                var canPerform = canAfford && hasEnergy && rightTime && notTrainedToday && notAtCap && npcTrustMet;

                string? reason = null;
                if (!canAfford && !hasEnergy && !rightTime)
                {
                    reason = "Not enough money, energy, or wrong time.";
                }
                else if (!rightTime)
                {
                    reason = "Only available in the evening (18:00-22:00).";
                }
                else if (!canAfford)
                {
                    reason = $"Costs {activity.MoneyCost} LE. You have {context.Money} LE.";
                }
                else if (!hasEnergy)
                {
                    reason = $"Requires {activity.EnergyCost} energy. You have {context.Energy}.";
                }
                else if (!notTrainedToday)
                {
                    reason = $"Already trained {activity.Skill} today.";
                }

                return new TrainingMenuStatus(
                    activity,
                    canAfford,
                    hasEnergy,
                    rightTime,
                    npcTrustMet,
                    notTrainedToday,
                    notAtCap,
                    canPerform,
                    reason);
            })
            .ToArray();
    }
}
