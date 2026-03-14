using Slums.Core.Characters;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Core.Jobs;

public static class WorkNarrativePlanner
{
    public static WorkSpilloverPlan? GetPublicWorkHeatPlan(
        int currentDay,
        int lastCrimeDay,
        int policePressure,
        IReadOnlySet<string> storyFlags,
        JobShift job)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        ArgumentNullException.ThrowIfNull(job);

        if (!NarrativeSignalRules.HasPendingPublicWorkHeat(currentDay, lastCrimeDay, policePressure) ||
            !ActivityLedgerSystem.IsPublicFacingJob(job.Type))
        {
            return null;
        }

        var trigger = storyFlags.Contains(StoryFlags.EventPublicWorkHeatSeen)
            ? null
            : new NarrativeSceneTrigger(StoryFlags.EventPublicWorkHeatSeen, NarrativeKnots.EventPublicWorkHeat);

        return new WorkSpilloverPlan(
            4,
            -2,
            "The street heat follows you into work. People notice how tense you look.",
            trigger);
    }

    public static NarrativeSceneTrigger? GetMedicalClinicTrigger(PlayerCharacter player, JobShift job, JobResult result, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(storyFlags);

        return result.Success &&
               job.Type == JobType.ClinicReception &&
               NarrativeSignalRules.HasPendingMedicalClinicReflection(player, storyFlags)
            ? new NarrativeSceneTrigger(StoryFlags.BackgroundMedicalClinicSeen, NarrativeKnots.BackgroundMedicalClinic)
            : null;
    }

    public static bool ShouldGrantSalmaMedicineHelp(PlayerCharacter player, JobShift job, JobResult result, RelationshipState relationships)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(relationships);

        return result.Success &&
               job.Type == JobType.ClinicReception &&
               NarrativeSignalRules.HasPendingSalmaMedicineHelp(player, relationships);
    }

    public static bool ShouldEmbarrassWorkshopBoss(JobShift job, JobResult result)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(result);

        return result.MistakeMade &&
               job.Type == JobType.WorkshopSewing;
    }
}
