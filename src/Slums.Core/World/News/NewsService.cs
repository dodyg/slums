using Slums.Core.Diagnostics;
using Slums.Core.State;

namespace Slums.Core.World.News;

public static class NewsService
{
    private const int DailyGenerationChancePercent = 24;

    public static NewsFlashDefinition? ResolveStartOfDay(
        NewsState news,
        InfrastructureState infrastructure,
        EventJournal journal,
        int currentDay,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(news);
        ArgumentNullException.ThrowIfNull(infrastructure);
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(random);

        news.BeginDay(currentDay);
        infrastructure.AdvanceDay();

        #pragma warning disable CA5394 // Seeded gameplay randomness is intentional and persisted with the session.
        var shouldGenerate = currentDay >= 2 && NewsRegistry.All.Count > 0 && random.Next(100) < DailyGenerationChancePercent;
        #pragma warning restore CA5394
        if (!shouldGenerate)
        {
            return null;
        }

        var eligible = NewsRegistry.All
            .Where(definition => definition.MinimumDay <= currentDay)
            .Where(definition => definition.Weight > 0 && definition.DurationDays > 0)
            .Where(definition => !news.HasSeen(definition.Id))
            .Where(definition => currentDay - news.LastGeneratedDayFor(definition.Category) >= definition.CooldownDays)
            .Where(definition => !news.ActiveFlashes.Any(active => NewsRegistry.GetById(active.DefinitionId)?.Category == definition.Category))
            .ToArray();
        if (eligible.Length == 0)
        {
            return null;
        }

        var selected = SelectWeighted(eligible, random);
        news.Activate(selected, currentDay);
        ApplyInfrastructureEffects(selected, infrastructure, currentDay);
        journal.Add(currentDay, EventSource.GameEvent, $"News: {selected.Headline} ({selected.SourceLabel})");
        return selected;
    }

    public static bool TryUseResponse(NewsState news, NewsFlashDefinition definition, string responseId)
    {
        ArgumentNullException.ThrowIfNull(news);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseId);

        return definition.Responses.Any(response => response.Id == responseId)
            && news.TryUseResponse(definition.Id, responseId);
    }

    private static NewsFlashDefinition SelectWeighted(IReadOnlyList<NewsFlashDefinition> definitions, Random random)
    {
        var totalWeight = definitions.Sum(static definition => definition.Weight);
        #pragma warning disable CA5394 // Seeded gameplay randomness is intentional and persisted with the session.
        var roll = random.Next(totalWeight);
        #pragma warning restore CA5394
        foreach (var definition in definitions)
        {
            roll -= definition.Weight;
            if (roll < 0)
            {
                return definition;
            }
        }

        return definitions[^1];
    }

    private static void ApplyInfrastructureEffects(NewsFlashDefinition definition, InfrastructureState infrastructure, int currentDay)
    {
        foreach (var effect in definition.Effects.Where(static effect => effect.Type == NewsEffectType.StartInfrastructureDisruption))
        {
            var districts = effect.District is DistrictId district
                ? [district]
                : definition.AffectedDistricts;
            foreach (var affectedDistrict in districts)
            {
                if (effect.Service is InfrastructureServiceType service)
                {
                    infrastructure.StartDisruption(
                        affectedDistrict,
                        service,
                        effect.Severity,
                        effect.DurationDays > 0 ? effect.DurationDays : definition.DurationDays,
                        currentDay,
                        definition.Id);
                }
            }
        }
    }
}
