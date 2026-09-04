using Slums.Application.Narrative;

namespace Slums.Narrative.Ink;

/// <summary>Merges the independent outcomes emitted by one Ink scene.</summary>
internal static class NarrativeOutcomeMerger
{
    internal static NarrativeOutcome MergeOutcome(NarrativeOutcome? existing, NarrativeOutcome next)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (existing is null)
        {
            return next;
        }

        return existing with
        {
            MoneyChange = existing.MoneyChange + next.MoneyChange,
            HealthChange = existing.HealthChange + next.HealthChange,
            EnergyChange = existing.EnergyChange + next.EnergyChange,
            HungerChange = existing.HungerChange + next.HungerChange,
            StressChange = existing.StressChange + next.StressChange,
            MotherHealthChange = existing.MotherHealthChange + next.MotherHealthChange,
            FoodChange = existing.FoodChange + next.FoodChange,
            SetFlags = MergeFlags(existing, next),
            SetFlag = next.SetFlag ?? existing.SetFlag,
            Message = string.IsNullOrWhiteSpace(existing.Message) ? next.Message : string.Join(" ", new[] { existing.Message, next.Message }.Where(static message => !string.IsNullOrWhiteSpace(message))),
            Effects = existing.Effects.Concat(next.Effects).ToArray()
        };
    }

    internal static string[] MergeFlags(NarrativeOutcome existing, NarrativeOutcome next)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(next);

        var existingFlags = existing.SetFlags.Count > 0
            ? existing.SetFlags
            : existing.SetFlag is { } existingFlag ? [existingFlag] : [];
        var nextFlags = next.SetFlags.Count > 0
            ? next.SetFlags
            : next.SetFlag is { } nextFlag ? [nextFlag] : [];

        return existingFlags.Concat(nextFlags).ToArray();
    }
}
