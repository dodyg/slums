using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
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
                    GetNarrativeSignals(context, option.Preview.Job),
                    GetAvailabilitySignals(option, lockoutUntilDay),
                    GetReliabilitySummary(option));
            })
            .ToArray();
    }

    private static List<string> GetAvailabilitySignals(WorkMenuOptionContext option, int? lockoutUntilDay)
    {
        var signals = new List<string>();

        if (lockoutUntilDay is not null)
        {
            signals.Add($"A recent mistake triggered an employer shutout until day {lockoutUntilDay + 1}.");
        }
        else if (option.CanPerform)
        {
            signals.Add("Open now. A clean shift raises reliability and keeps better variants within reach.");
        }
        else if (!string.IsNullOrWhiteSpace(option.AvailabilityReason))
        {
            signals.Add(option.AvailabilityReason);
        }

        if (!string.IsNullOrWhiteSpace(option.Preview.NextUnlockHint))
        {
            signals.Add($"Improvement target: {option.Preview.NextUnlockHint}");
        }

        if (!string.IsNullOrWhiteSpace(option.Preview.RiskWarning))
        {
            signals.Add("Mistakes cut pay, damage reliability, and can trigger a temporary shutout.");
        }

        return signals;
    }

    private static string GetReliabilitySummary(WorkMenuOptionContext option)
    {
        var baseSummary = option.Track.Reliability switch
        {
            < 45 => "Reliability is shaky. Employers are quick to blame you and better variants stay out of reach.",
            < 60 => "Reliability is recovering. Steady shifts will start opening stronger variants.",
            < 75 => "Reliability is solid. You are close to the better-trusted versions of this job.",
            _ => "Reliability is strong. This track is already trusted enough for the higher-end variants."
        };

        return $"{baseSummary} Current track: {option.Track.Reliability}/100 after {option.Track.ShiftsCompleted} shifts.";
    }

    private static List<string> GetNarrativeSignals(WorkMenuContext context, JobShift job)
    {
        var signals = new List<string>();
        var recentCrimeHeat = NarrativeSignalRules.HasPendingPublicWorkHeat(context.CurrentDay, context.LastCrimeDay, context.PolicePressure);

        if (recentCrimeHeat && IsPublicFacingJob(job.Type))
        {
            signals.Add("Taking this public-facing shift now can trigger a police-heat suspicion follow-up.");
        }

        if (job.Type == JobType.ClinicReception)
        {
            if (NarrativeSignalRules.HasPendingMedicalClinicReflection(context.Player, context.StoryFlags))
            {
                signals.Add("A successful clinic shift can trigger a medical-dropout reflection scene.");
            }

            if (NarrativeSignalRules.HasPendingSalmaMedicineHelp(context.Player, context.Relationships))
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
