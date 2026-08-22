namespace Slums.Core.Narrative;

public sealed record CityCrisisBeat(int Index, int EarliestDay, string KnotName, CityCrisisPhase Phase);

/// <summary>Derives the next crisis beat from time and persistent crisis state.</summary>
public static class CityCrisisPlanner
{
    private static readonly CityCrisisBeat[] Beats =
    [
        new(0, 1, NarrativeKnots.CrisisIntroduction, CityCrisisPhase.NotDiscovered),
        new(1, 6, NarrativeKnots.CrisisClassification, CityCrisisPhase.IrregularClassification),
        new(2, 12, NarrativeKnots.CrisisAppeal, CityCrisisPhase.Appeal),
        new(3, 18, NarrativeKnots.CrisisHeatEmergency, CityCrisisPhase.HeatEmergency),
        new(4, 25, NarrativeKnots.CrisisCommitment, CityCrisisPhase.Commitment),
        new(5, 30, NarrativeKnots.CrisisResolution, CityCrisisPhase.Commitment)
    ];

    public static CityCrisisBeat? GetNextBeat(int currentDay, CityCrisisState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return Beats.FirstOrDefault(beat => beat.Index == state.BeatIndex && currentDay >= beat.EarliestDay && state.Resolution == CityCrisisResolution.Unresolved);
    }

    /// <summary>Returns the one delayed callback created by the current crisis decision.</summary>
    public static NarrativeSceneTrigger? GetDelayedCallback(
        int currentDay,
        CityCrisisState state,
        IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(storyFlags);

        if (!state.HasDueCallback(currentDay))
        {
            return null;
        }

        return state.PendingCallbackDecision switch
        {
            CityCrisisDecision.EvidenceAppeal => CreateCallback(StoryFlags.CrisisEvidenceCallbackSeen, NarrativeKnots.CrisisEvidenceCallback, storyFlags),
            CityCrisisDecision.MutualAid => CreateCallback(StoryFlags.CrisisMutualAidCallbackSeen, NarrativeKnots.CrisisMutualAidCallback, storyFlags),
            CityCrisisDecision.Diversion => CreateCallback(StoryFlags.CrisisDiversionCallbackSeen, NarrativeKnots.CrisisDiversionCallback, storyFlags),
            CityCrisisDecision.PublicPressure => CreateCallback(StoryFlags.CrisisPublicPressureCallbackSeen, NarrativeKnots.CrisisPublicPressureCallback, storyFlags),
            _ => null
        };
    }

    private static NarrativeSceneTrigger? CreateCallback(
        string flagName,
        string knotName,
        IReadOnlySet<string> storyFlags)
    {
        return storyFlags.Contains(flagName) ? null : new NarrativeSceneTrigger(flagName, knotName);
    }
}
