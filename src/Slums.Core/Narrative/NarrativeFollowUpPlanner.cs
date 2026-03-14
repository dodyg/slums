using Slums.Core.Characters;
using Slums.Core.Relationships;

namespace Slums.Core.Narrative;

public static class NarrativeFollowUpPlanner
{
    public static IReadOnlyList<NarrativeSceneTrigger> GetEndOfDayTriggers(
        bool crimeCommittedToday,
        PlayerCharacter player,
        int totalCrimeEarnings,
        int crimesCommitted,
        int policePressure,
        RelationshipState relationships,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);

        if (!crimeCommittedToday)
        {
            return [];
        }

        var triggers = new List<NarrativeSceneTrigger>();

        if (NarrativeSignalRules.HasPendingMotherWrongMoney(player, totalCrimeEarnings, crimesCommitted, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventMotherWrongMoneySeen, NarrativeKnots.EventMotherWrongMoney));
        }

        if (NarrativeSignalRules.HasPendingNeighborWatch(policePressure, relationships, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventNeighborWatchSeen, NarrativeKnots.EventNeighborWatch));
        }

        return triggers;
    }
}
