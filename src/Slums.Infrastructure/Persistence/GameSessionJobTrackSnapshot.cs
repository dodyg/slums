using Slums.Core.Jobs;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionJobTrackSnapshot
{
    public int Reliability { get; init; } = 50;

    public int ShiftsCompleted { get; init; }

    public int LockoutUntilDay { get; init; }

    public static GameSessionJobTrackSnapshot Capture(JobTrackProgress track)
    {
        ArgumentNullException.ThrowIfNull(track);

        return new GameSessionJobTrackSnapshot
        {
            Reliability = track.Reliability,
            ShiftsCompleted = track.ShiftsCompleted,
            LockoutUntilDay = track.LockoutUntilDay
        };
    }
}
