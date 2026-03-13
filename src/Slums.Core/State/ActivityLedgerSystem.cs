using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Crimes;
using Slums.Core.Jobs;

namespace Slums.Core.State;

internal static class ActivityLedgerSystem
{
    public static void RecordWorkShift(GameWorkState workState, GameClock clock, JobShift job, JobResult result)
    {
        ArgumentNullException.ThrowIfNull(workState);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Success)
        {
            return;
        }

        workState.TotalHonestWorkEarnings += result.MoneyEarned;
        workState.HonestShiftsCompleted++;
        workState.LastHonestWorkDay = clock.Day;
        if (IsPublicFacingJob(job.Type))
        {
            workState.LastPublicFacingWorkDay = clock.Day;
        }
    }

    public static void RecordCrimeOutcome(GameCrimeState crimeState, GameClock clock, CrimeResult result)
    {
        ArgumentNullException.ThrowIfNull(crimeState);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(result);

        crimeState.CrimeCommittedToday = true;
        crimeState.LastCrimeDay = clock.Day;
        if (!result.Success)
        {
            return;
        }

        crimeState.TotalCrimeEarnings += result.MoneyEarned;
        crimeState.CrimesCommitted++;
    }

    public static int GetDailyPolicePressureDecay(BackgroundType backgroundType)
    {
        return backgroundType == BackgroundType.ReleasedPoliticalPrisoner ? 2 : 5;
    }

    public static void BeginNewDay(GameCrimeState crimeState)
    {
        ArgumentNullException.ThrowIfNull(crimeState);

        crimeState.CrimeCommittedToday = false;
    }

    public static bool IsPublicFacingJob(JobType jobType)
    {
        return jobType is JobType.CallCenterWork or JobType.ClinicReception or JobType.CafeService or JobType.PharmacyStock or JobType.MicrobusDispatch;
    }
}
