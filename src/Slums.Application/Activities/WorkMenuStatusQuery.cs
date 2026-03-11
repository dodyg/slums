using Slums.Core.State;

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
                    job,
                    track.Reliability,
                    track.ShiftsCompleted,
                    lockoutUntilDay,
                    canPerform,
                    canPerform ? null : reason);
            })
            .ToArray();
    }
}