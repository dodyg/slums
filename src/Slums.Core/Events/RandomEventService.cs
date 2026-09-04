using Slums.Core.Diagnostics;
using Slums.Core.Narrative;
using Slums.Core.State;
using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

namespace Slums.Core.Events;

public sealed class RandomEventService
{
    internal static int GetEffectiveEventWeight(GameSession gameState, RandomEvent randomEvent)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(randomEvent);

        var weight = randomEvent.Weight;
        var districtCondition = gameState.GetActiveDistrictConditionDefinition(gameState.World.CurrentDistrict);
        if (districtCondition is null)
        {
            return weight;
        }

        if (districtCondition.Effect.BoostedRandomEventIds.Contains(randomEvent.Id, StringComparer.Ordinal))
        {
            weight += 4;
        }

        if (districtCondition.Effect.SuppressedRandomEventIds.Contains(randomEvent.Id, StringComparer.Ordinal))
        {
            weight = Math.Max(1, weight - 3);
        }

        return weight;
    }

    internal static void ApplyEvent(GameSession gameState, RandomEvent randomEvent)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(randomEvent);

        var before = gameState.CaptureStats();
        gameState.RecordEventHistory(randomEvent.Id, gameState.GetEventCount(randomEvent.Id) + 1);

        var effect = randomEvent.Effect;
        if (effect.MoneyChange != 0)
        {
            gameState.Player.Stats.ModifyMoney(effect.MoneyChange);
        }

        if (effect.HealthChange != 0)
        {
            gameState.Player.Stats.ModifyHealth(effect.HealthChange);
        }

        if (effect.EnergyChange != 0)
        {
            gameState.Player.Stats.ModifyEnergy(effect.EnergyChange);
        }

        if (effect.HungerChange != 0)
        {
            gameState.Player.Nutrition.ModifySatiety(effect.HungerChange);
            gameState.SyncLegacyHunger();
        }

        if (effect.StressChange != 0)
        {
            gameState.Player.Stats.ModifyStress(effect.StressChange);
        }

        if (effect.PolicePressureChange != 0)
        {
            gameState.DistrictHeat.AddHeat(gameState.World.CurrentDistrict, effect.PolicePressureChange);
        }

        if (effect.MotherHealthChange != 0)
        {
            gameState.Player.Household.UpdateMotherHealth(effect.MotherHealthChange);
        }

        if (effect.FoodChange > 0)
        {
            gameState.Player.Household.AddFood(effect.FoodChange);
        }
        else if (effect.FoodChange < 0)
        {
            for (var i = 0; i < -effect.FoodChange; i++)
            {
                gameState.Player.Household.ConsumeFood();
            }
        }

        gameState.RaiseEvent(randomEvent.Description);

        if (NarrativeSignalRules.HasPendingSudaneseSolidarity(gameState.Player.BackgroundType, randomEvent.Id, gameState.StoryFlags.ToHashSet(StringComparer.Ordinal)))
        {
            gameState.TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.BackgroundSudaneseSolidaritySeen, NarrativeKnots.BackgroundSudaneseSolidarity));
        }

        if (!string.IsNullOrWhiteSpace(effect.InkKnot))
        {
            gameState.QueueNarrativeScene(effect.InkKnot);
        }

        gameState.RecordMutation(MutationCategories.RandomEvent, "ApplyRandomEvent", before, gameState.CaptureStats(), $"Event: {randomEvent.Id} - {randomEvent.Description}");
    }

#pragma warning disable CA1822
    public IReadOnlyList<RandomEvent> RollDailyEvents(GameSession gameState, Random random)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(random);

        var eligibleEvents = RandomEventRegistry.AllEvents
            .Where(randomEvent => gameState.Clock.Day >= randomEvent.MinDay)
            .Where(randomEvent => randomEvent.Condition is null || randomEvent.Condition(gameState))
            .ToList();

        if (eligibleEvents.Count == 0)
        {
            return [];
        }

        var rolledEvents = new List<RandomEvent>(capacity: 2);
        var rolls = 0;
        while (eligibleEvents.Count > 0 && rolledEvents.Count < 2 && rolls < 2)
        {
            rolls++;
#pragma warning disable CA5394
            if (random.NextDouble() > 0.55d)
#pragma warning restore CA5394
            {
                continue;
            }

            var selected = SelectWeightedEvent(eligibleEvents, gameState, random);
            rolledEvents.Add(selected);
            eligibleEvents.Remove(selected);
        }

        return rolledEvents;
    }

    private static RandomEvent SelectWeightedEvent(IReadOnlyList<RandomEvent> events, GameSession gameState, Random random)
    {
        var totalWeight = events.Sum(gameState.GetEffectiveRandomEventWeight);
#pragma warning disable CA5394
        var roll = random.Next(1, totalWeight + 1);
#pragma warning restore CA5394
        var cumulativeWeight = 0;

        foreach (var randomEvent in events)
        {
            cumulativeWeight += gameState.GetEffectiveRandomEventWeight(randomEvent);
            if (roll <= cumulativeWeight)
            {
                return randomEvent;
            }
        }

        return events[^1];
    }
}
