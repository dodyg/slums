using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.Jobs;
using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed class WorkMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<WorkMenuStatus> GetStatuses(GameState gameState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var location = gameState.World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        return gameState
            .GetAvailableJobs()
            .Select(job =>
            {
                var preview = gameState.Jobs.PreviewJob(job.Type, gameState.Player, gameState.Relationships, gameState.JobProgress);
                var track = gameState.JobProgress.GetTrack(job.Type);
                var canPerform = gameState.Jobs.CanPerformJob(
                    job,
                    gameState.Player,
                    location,
                    gameState.Relationships,
                    gameState.JobProgress,
                    gameState.Clock.Day,
                    out var reason);

                int? lockoutUntilDay = track.IsLockedOut(gameState.Clock.Day)
                    ? track.LockoutUntilDay
                    : null;

                return new WorkMenuStatus(
                    preview.Job,
                    track.Reliability,
                    track.ShiftsCompleted,
                    lockoutUntilDay,
                    canPerform,
                    canPerform ? null : reason,
                    preview.VariantReason,
                    preview.NextUnlockHint,
                    preview.ActiveModifiers,
                    preview.RiskWarning,
                    GetNarrativeSignals(gameState, preview.Job));
            })
            .ToArray();
    }

    private static List<string> GetNarrativeSignals(GameState gameState, JobShift job)
    {
        var signals = new List<string>();
        var recentCrimeHeat = gameState.LastCrimeDay > 0 && gameState.Clock.Day - gameState.LastCrimeDay <= 1;

        if (recentCrimeHeat && gameState.PolicePressure >= 60 && IsPublicFacingJob(job.Type))
        {
            signals.Add("Taking this public-facing shift now can trigger a police-heat suspicion follow-up.");
        }

        if (job.Type == JobType.ClinicReception)
        {
            if (gameState.Player.BackgroundType == BackgroundType.MedicalSchoolDropout && !gameState.HasStoryFlag("background_medical_clinic_seen"))
            {
                signals.Add("A successful clinic shift can trigger a medical-dropout reflection scene.");
            }

            if (gameState.Player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
                gameState.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 12 &&
                gameState.Player.Household.MotherHealth < 65)
            {
                signals.Add("A strong clinic day can push Salma to quietly help with medicine.");
            }
        }

        if (job.Type == JobType.WorkshopSewing && recentCrimeHeat && gameState.Player.Stats.Energy <= 40)
        {
            signals.Add("A bad workshop shift under street heat can leave Abu Samir remembering the embarrassment.");
        }

        return signals;
    }

    private static bool IsPublicFacingJob(JobType jobType)
    {
        return jobType is JobType.CallCenterWork or JobType.ClinicReception or JobType.CafeService or JobType.PharmacyStock or JobType.MicrobusDispatch;
    }
}