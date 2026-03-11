namespace Slums.Core.Jobs;

public sealed record JobTrackProgress(
    JobType Type,
    int Reliability,
    int ShiftsCompleted,
    int LockoutUntilDay)
{
    public bool IsLockedOut(int currentDay)
    {
        return LockoutUntilDay >= currentDay;
    }
}