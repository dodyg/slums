using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed class WorkMenuStatusQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<WorkMenuStatus> GetStatuses(WorkMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return context
            .Options
            .Select(option =>
            {
                int? lockoutUntilDay = option.Track.IsLockedOut(context.CurrentDay)
                    ? option.Track.LockoutUntilDay
                    : null;

                return new WorkMenuStatus(
                    option.Preview.Job,
                    option.Track.Reliability,
                    option.Track.ShiftsCompleted,
                    lockoutUntilDay,
                    option.CanPerform,
                    option.AvailabilityReason,
                    option.Preview.VariantReason,
                    option.Preview.NextUnlockHint,
                    option.Preview.ActiveModifiers,
                    option.Preview.RiskWarning,
                    GetNarrativeSignals(context, option.Preview.Job));
            })
            .ToArray();
    }

    private static List<string> GetNarrativeSignals(WorkMenuContext context, JobShift job)
    {
        var signals = new List<string>();
        var recentCrimeHeat = context.LastCrimeDay > 0 && context.CurrentDay - context.LastCrimeDay <= 1;

        if (recentCrimeHeat && context.PolicePressure >= 60 && IsPublicFacingJob(job.Type))
        {
            signals.Add("Taking this public-facing shift now can trigger a police-heat suspicion follow-up.");
        }

        if (job.Type == JobType.ClinicReception)
        {
            if (context.Player.BackgroundType == BackgroundType.MedicalSchoolDropout && !context.HasStoryFlag("background_medical_clinic_seen"))
            {
                signals.Add("A successful clinic shift can trigger a medical-dropout reflection scene.");
            }

            if (context.Player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
                context.Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 12 &&
                context.Player.Household.MotherHealth < 65)
            {
                signals.Add("A strong clinic day can push Salma to quietly help with medicine.");
            }
        }

        if (job.Type == JobType.WorkshopSewing && recentCrimeHeat && context.Player.Stats.Energy <= 40)
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
