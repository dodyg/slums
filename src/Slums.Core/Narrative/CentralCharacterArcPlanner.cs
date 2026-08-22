namespace Slums.Core.Narrative;

public static class CentralCharacterArcPlanner
{
    private static readonly (int Day, CentralCharacterId Character, string Knot, string Flag)[] Beats =
    [
        (2, CentralCharacterId.Mother, NarrativeKnots.CentralMotherArc, StoryFlags.CentralMotherArcSeen),
        (3, CentralCharacterId.NeighborMona, NarrativeKnots.CentralMonaArc, StoryFlags.CentralMonaArcSeen),
        (4, CentralCharacterId.NurseSalma, NarrativeKnots.CentralSalmaArc, StoryFlags.CentralSalmaArcSeen),
        (5, CentralCharacterId.HajjMahmoud, NarrativeKnots.CentralMahmoudArc, StoryFlags.CentralMahmoudArcSeen),
        (6, CentralCharacterId.UmmKarim, NarrativeKnots.CentralUmmKarimArc, StoryFlags.CentralUmmKarimArcSeen),
        (8, CentralCharacterId.Mother, NarrativeKnots.CentralMotherVulnerability, StoryFlags.CentralMotherVulnerabilitySeen),
        (9, CentralCharacterId.NeighborMona, NarrativeKnots.CentralMonaTransaction, StoryFlags.CentralMonaTransactionSeen),
        (10, CentralCharacterId.NurseSalma, NarrativeKnots.CentralSalmaTransaction, StoryFlags.CentralSalmaTransactionSeen),
        (11, CentralCharacterId.HajjMahmoud, NarrativeKnots.CentralMahmoudTransaction, StoryFlags.CentralMahmoudTransactionSeen),
        (12, CentralCharacterId.UmmKarim, NarrativeKnots.CentralUmmKarimTransaction, StoryFlags.CentralUmmKarimTransactionSeen),
        (14, CentralCharacterId.Mother, NarrativeKnots.CentralMotherConflict, StoryFlags.CentralMotherConflictSeen),
        (15, CentralCharacterId.NeighborMona, NarrativeKnots.CentralMonaVulnerability, StoryFlags.CentralMonaVulnerabilitySeen),
        (16, CentralCharacterId.NurseSalma, NarrativeKnots.CentralSalmaVulnerability, StoryFlags.CentralSalmaVulnerabilitySeen),
        (17, CentralCharacterId.HajjMahmoud, NarrativeKnots.CentralMahmoudVulnerability, StoryFlags.CentralMahmoudVulnerabilitySeen),
        (18, CentralCharacterId.UmmKarim, NarrativeKnots.CentralUmmKarimVulnerability, StoryFlags.CentralUmmKarimVulnerabilitySeen),
        (20, CentralCharacterId.Mother, NarrativeKnots.CentralMotherReckoning, StoryFlags.CentralMotherReckoningSeen),
        (21, CentralCharacterId.NeighborMona, NarrativeKnots.CentralMonaConflict, StoryFlags.CentralMonaConflictSeen),
        (22, CentralCharacterId.NurseSalma, NarrativeKnots.CentralSalmaConflict, StoryFlags.CentralSalmaConflictSeen),
        (23, CentralCharacterId.HajjMahmoud, NarrativeKnots.CentralMahmoudConflict, StoryFlags.CentralMahmoudConflictSeen),
        (24, CentralCharacterId.UmmKarim, NarrativeKnots.CentralUmmKarimConflict, StoryFlags.CentralUmmKarimConflictSeen),
        (26, CentralCharacterId.Mother, NarrativeKnots.CentralMotherOutcome, StoryFlags.CentralMotherOutcomeSeen),
        (27, CentralCharacterId.NeighborMona, NarrativeKnots.CentralMonaOutcome, StoryFlags.CentralMonaOutcomeSeen),
        (28, CentralCharacterId.NurseSalma, NarrativeKnots.CentralSalmaOutcome, StoryFlags.CentralSalmaOutcomeSeen),
        (29, CentralCharacterId.HajjMahmoud, NarrativeKnots.CentralMahmoudOutcome, StoryFlags.CentralMahmoudOutcomeSeen),
        (30, CentralCharacterId.UmmKarim, NarrativeKnots.CentralUmmKarimOutcome, StoryFlags.CentralUmmKarimOutcomeSeen)
    ];

    public static NarrativeSceneTrigger? GetNextTrigger(int day, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);

        foreach (var beat in Beats)
        {
            if (day >= beat.Day && !storyFlags.Contains(beat.Flag))
            {
                return new NarrativeSceneTrigger(beat.Flag, beat.Knot);
            }
        }

        return null;
    }
}
