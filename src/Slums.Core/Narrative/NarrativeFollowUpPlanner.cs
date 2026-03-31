using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Relationships;namespace Slums.Core.Narrative;

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

        var triggers = new List<NarrativeSceneTrigger>();

        if (crimeCommittedToday)
        {
            if (NarrativeSignalRules.HasPendingMotherWrongMoney(player, totalCrimeEarnings, crimesCommitted, storyFlags))
            {
                triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventMotherWrongMoneySeen, NarrativeKnots.EventMotherWrongMoney));
            }

            if (NarrativeSignalRules.HasPendingNeighborWatch(policePressure, relationships, storyFlags))
            {
                triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventNeighborWatchSeen, NarrativeKnots.EventNeighborWatch));
            }
        }

        if (NarrativeSignalRules.HasPendingArrestCloseCall(policePressure, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventArrestCloseCallSeen, NarrativeKnots.EventArrestCloseCall));
        }

        if (NarrativeSignalRules.HasPendingPrisonerKhalid(player.BackgroundType, relationships, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.BackgroundPrisonerKhalidSeen, NarrativeKnots.BackgroundPrisonerKhalid));
        }

        if (NarrativeSignalRules.HasPendingSudaneseMariam(player.BackgroundType, relationships, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.BackgroundSudaneseMariamSeen, NarrativeKnots.BackgroundSudaneseMariam));
        }

        if (NarrativeSignalRules.HasPendingYoussefEmbedded(crimesCommitted, relationships, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventYoussefEmbeddedSeen, NarrativeKnots.EventYoussefEmbedded));
        }

        return triggers;
    }

    public static IReadOnlyList<NarrativeSceneTrigger> GetWorkFollowUpTriggers(
        int honestShiftsCompleted,
        int crimesCommitted,
        RelationshipState relationships,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);

        var triggers = new List<NarrativeSceneTrigger>();

        if (NarrativeSignalRules.HasPendingHonestMilestone(honestShiftsCompleted, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventHonestMilestoneSeen, NarrativeKnots.EventHonestMilestone));
        }

        if (NarrativeSignalRules.HasPendingEmbarrassmentRecovery(relationships, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventEmbarrassmentRecoverySeen, NarrativeKnots.EventEmbarrassmentRecovery));
        }

        if (NarrativeSignalRules.HasPendingNadiaSuspicion(honestShiftsCompleted, crimesCommitted, relationships, storyFlags))
        {
            triggers.Add(new NarrativeSceneTrigger(StoryFlags.EventNadiaSuspicionSeen, NarrativeKnots.EventNadiaSuspicion));
        }

        return triggers;
    }

    public static NarrativeSceneTrigger? GetCommunityAftermathTrigger(
        CommunityEventAttendance eventAttendance,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(eventAttendance);
        ArgumentNullException.ThrowIfNull(storyFlags);

        var attendedSolidarityEvent = eventAttendance.TotalAttended >= 2;

        return NarrativeSignalRules.HasPendingCommunityAftermath(attendedSolidarityEvent, storyFlags)
            ? new NarrativeSceneTrigger(StoryFlags.EventCommunityAftermathSeen, NarrativeKnots.EventCommunityAftermath)
            : null;
    }
}
