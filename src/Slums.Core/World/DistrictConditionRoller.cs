using Slums.Core.State;

namespace Slums.Core.World;

internal static class DistrictConditionRoller
{
    internal static IReadOnlyList<DistrictConditionDefinition> GetDailyConditions(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.World.ActiveDistrictConditions
            .Select(activeCondition => (activeCondition, definition: session.ContentCatalog.DistrictConditions.FirstOrDefault(definition => definition.Id == activeCondition.ConditionId)))
            .Where(static item => item.definition is not null)
            .OrderBy(static item => item.activeCondition.District)
            .Select(static item => item.definition!)
            .ToArray();
    }

    internal static DistrictConditionDefinition? GetActiveCondition(GameSession session, DistrictId districtId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var conditionId = session.World.GetActiveDistrictCondition(districtId)?.ConditionId;
        return session.ContentCatalog.DistrictConditions.FirstOrDefault(definition => definition.Id == conditionId);
    }

    internal static void RollForCurrentDay(GameSession session, Random random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(random);

        var activeConditions = new List<ActiveDistrictCondition>();
        foreach (var districtId in Enum.GetValues<DistrictId>())
        {
            var candidates = session.ContentCatalog.DistrictConditions
                .Where(definition => definition.District == districtId)
                .Where(definition => definition.IsEligible(session.Clock.Day, session.PolicePressure))
                .ToArray();
            if (candidates.Length == 0)
            {
                continue;
            }

            var selected = SelectWeightedCondition(candidates, random);
            activeConditions.Add(new ActiveDistrictCondition
            {
                District = districtId,
                ConditionId = selected.Id
            });
        }

        session.World.SetActiveDistrictConditions(activeConditions);
    }

    internal static void SetBaseline(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        session.World.SetActiveDistrictConditions(
        [
            new ActiveDistrictCondition { District = DistrictId.Imbaba, ConditionId = "imbaba_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.Dokki, ConditionId = "dokki_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.ArdAlLiwa, ConditionId = "ardalliwa_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.BulaqAlDakrour, ConditionId = "bulaq_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.Shubra, ConditionId = "shubra_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.DowntownCairo, ConditionId = "downtown_cairo_steady_day" }
        ]);
    }

    private static DistrictConditionDefinition SelectWeightedCondition(
        IReadOnlyList<DistrictConditionDefinition> candidates,
        Random random)
    {
        var totalWeight = candidates.Sum(static definition => definition.Weight);
#pragma warning disable CA5394
        var roll = random.Next(1, totalWeight + 1);
#pragma warning restore CA5394
        var cumulativeWeight = 0;
        foreach (var candidate in candidates)
        {
            cumulativeWeight += candidate.Weight;
            if (roll <= cumulativeWeight)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }
}
