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
}
