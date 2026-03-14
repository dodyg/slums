using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed record WorkMenuContext(
    int CurrentDay,
    PlayerCharacter Player,
    RelationshipState Relationships,
    int PolicePressure,
    int LastCrimeDay,
    IReadOnlyList<WorkMenuOptionContext> Options,
    IReadOnlySet<string> StoryFlags)
{
    public static WorkMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var location = gameSession.World.GetCurrentLocation();
        var options = gameSession
            .GetAvailableJobs()
            .Select(job => CreateOption(gameSession, location, job))
            .ToArray();

        return new WorkMenuContext(
            gameSession.Clock.Day,
            gameSession.Player,
            gameSession.Relationships,
            gameSession.PolicePressure,
            gameSession.LastCrimeDay,
            options,
            gameSession.StoryFlags.ToHashSet(StringComparer.Ordinal));
    }

    public bool HasStoryFlag(string flag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flag);
        return StoryFlags.Contains(flag);
    }

    private static WorkMenuOptionContext CreateOption(GameSession gameSession, Slums.Core.World.Location? location, JobShift job)
    {
        var preview = gameSession.PreviewJob(job.Type);
        var track = gameSession.JobProgress.GetTrack(job.Type);
        string? reason = null;
        var canPerform = false;
        if (location is not null)
        {
            canPerform = gameSession.Jobs.CanPerformJob(
                job,
                gameSession.Player,
                location,
                gameSession.Relationships,
                gameSession.JobProgress,
                gameSession.Clock.Day,
                out var evaluatedReason);
            reason = evaluatedReason;
        }

        return new WorkMenuOptionContext(job, preview, track, canPerform, canPerform ? null : reason);
    }
}
