using Slums.Core.Jobs;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionJobProgressSnapshot
{
    public Dictionary<string, GameSessionJobTrackSnapshot> Tracks { get; init; } = [];

    public static GameSessionJobProgressSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionJobProgressSnapshot
        {
            Tracks = gameSession.JobProgress.Tracks.ToDictionary(
                static pair => pair.Key.ToString(),
                static pair => GameSessionJobTrackSnapshot.Capture(pair.Value))
        };
    }

    public GameSessionJobTrackSnapshot GetTrackSnapshot(JobType jobType)
    {
        return Tracks.GetValueOrDefault(jobType.ToString()) ?? new GameSessionJobTrackSnapshot();
    }
}
