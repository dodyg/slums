using Slums.Core.Jobs;

namespace Slums.Core.World.News;

public static class NewsImpactCalculator
{
    public static int GetFoodPriceModifier(NewsState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetEffectTotal(state, NewsEffectType.FoodPriceModifier, district);
    }

    public static int GetTravelCostModifier(NewsState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetEffectTotal(state, NewsEffectType.TravelCostModifier, district);
    }

    public static int GetJobPayModifier(NewsState state, JobType jobType)
    {
        ArgumentNullException.ThrowIfNull(state);
        var definitions = GetActiveDefinitions(state);
        return definitions
            .SelectMany(static definition => definition.Effects)
            .Where(effect => effect.Type == NewsEffectType.JobPayModifier)
            .Sum(static effect => effect.Amount);
    }

    public static int GetPolicePressureModifier(NewsState state, DistrictId district)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetEffectTotal(state, NewsEffectType.PolicePressureModifier, district);
    }

    public static int GetNpcHardshipModifier(NewsState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetActiveDefinitions(state)
            .SelectMany(static definition => definition.Effects)
            .Where(effect => effect.Type == NewsEffectType.NpcHardshipModifier)
            .Sum(static effect => effect.Amount);
    }

    public static IReadOnlySet<string> GetActiveNewsIds(NewsState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.ActiveFlashes.Select(static flash => flash.DefinitionId).ToHashSet(StringComparer.Ordinal);
    }

    private static int GetEffectTotal(NewsState state, NewsEffectType type, DistrictId district)
    {
        return GetActiveDefinitions(state)
            .Where(definition => definition.AffectedDistricts.Count == 0 || definition.AffectedDistricts.Contains(district))
            .SelectMany(static definition => definition.Effects)
            .Where(effect => effect.Type == type && (effect.District is null || effect.District == district))
            .Sum(static effect => effect.Amount);
    }

    private static IEnumerable<NewsFlashDefinition> GetActiveDefinitions(NewsState state)
    {
        return state.ActiveFlashes
            .Select(static flash => NewsRegistry.GetById(flash.DefinitionId))
            .OfType<NewsFlashDefinition>();
    }
}
