using Slums.Core.State;

namespace Slums.Core.Events;

public sealed class RandomEventService
{
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
