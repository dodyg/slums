using Slums.Core.Jobs;

namespace Slums.Core.World.News;

public static class NewsImpactCalculator
{
    public static int GetFoodPriceModifier(NewsState state, DistrictId district, IReadOnlyList<NewsFlashDefinition>? definitions = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetEffectTotal(state, NewsEffectType.FoodPriceModifier, district, definitions);
    }

    public static int GetTravelCostModifier(NewsState state, DistrictId district, IReadOnlyList<NewsFlashDefinition>? definitions = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetEffectTotal(state, NewsEffectType.TravelCostModifier, district, definitions);
    }

    public static int GetJobPayModifier(NewsState state, JobType jobType, IReadOnlyList<NewsFlashDefinition>? definitions = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetActiveDefinitions(state, definitions)
            .SelectMany(static definition => definition.Effects)
            .Where(effect => effect.Type == NewsEffectType.JobPayModifier)
            .Sum(static effect => effect.Amount);
    }

    public static int GetPolicePressureModifier(NewsState state, DistrictId district, IReadOnlyList<NewsFlashDefinition>? definitions = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetEffectTotal(state, NewsEffectType.PolicePressureModifier, district, definitions);
    }

    public static int GetNpcHardshipModifier(NewsState state, IReadOnlyList<NewsFlashDefinition>? definitions = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        return GetActiveDefinitions(state, definitions)
            .SelectMany(static definition => definition.Effects)
            .Where(effect => effect.Type == NewsEffectType.NpcHardshipModifier)
            .Sum(static effect => effect.Amount);
    }

    public static IReadOnlySet<string> GetActiveNewsIds(NewsState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.ActiveFlashes.Select(static flash => flash.DefinitionId).ToHashSet(StringComparer.Ordinal);
    }

    private static int GetEffectTotal(NewsState state, NewsEffectType type, DistrictId district, IReadOnlyList<NewsFlashDefinition>? definitions)
    {
        return GetActiveDefinitions(state, definitions)
            .Where(definition => definition.AffectedDistricts.Count == 0 || definition.AffectedDistricts.Contains(district))
            .SelectMany(static definition => definition.Effects)
            .Where(effect => effect.Type == type && (effect.District is null || effect.District == district))
            .Sum(static effect => effect.Amount);
    }

    private static IEnumerable<NewsFlashDefinition> GetActiveDefinitions(NewsState state, IReadOnlyList<NewsFlashDefinition>? definitions)
    {
        return state.ActiveFlashes
            .Select(flash => (definitions ?? NewsRegistry.All).FirstOrDefault(definition => definition.Id == flash.DefinitionId))
            .OfType<NewsFlashDefinition>();
    }
}
