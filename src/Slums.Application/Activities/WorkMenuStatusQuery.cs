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
                    preview.RiskWarning);
            })
            .ToArray();
    }
}