namespace Slums.Core.Jobs;

public sealed class JobProgressState
{
    private readonly Dictionary<JobType, JobTrackProgress> _tracks = Enum
        .GetValues<JobType>()
        .ToDictionary(
            static jobType => jobType,
            static jobType => new JobTrackProgress(jobType, 50, 0, 0));

    public IReadOnlyDictionary<JobType, JobTrackProgress> Tracks => _tracks;

    public JobTrackProgress GetTrack(JobType jobType)
    {
        return _tracks.GetValueOrDefault(jobType) ?? new JobTrackProgress(jobType, 50, 0, 0);
    }

    public void RecordSuccessfulShift(JobType jobType, int reliabilityDelta)
    {
        var current = GetTrack(jobType);
        _tracks[jobType] = current with
        {
            Reliability = Math.Clamp(current.Reliability + reliabilityDelta, 0, 100),
            ShiftsCompleted = current.ShiftsCompleted + 1
        };
    }

    public void RecordMistake(JobType jobType, int reliabilityDelta, int lockoutUntilDay)
    {
        var current = GetTrack(jobType);
        _tracks[jobType] = current with
        {
            Reliability = Math.Clamp(current.Reliability + reliabilityDelta, 0, 100),
            LockoutUntilDay = Math.Max(current.LockoutUntilDay, lockoutUntilDay)
        };
    }

    public void RestoreTrack(JobType jobType, int reliability, int shiftsCompleted, int lockoutUntilDay)
    {
        _tracks[jobType] = new JobTrackProgress(
            jobType,
            Math.Clamp(reliability, 0, 100),
            Math.Max(0, shiftsCompleted),
            Math.Max(0, lockoutUntilDay));
    }
}