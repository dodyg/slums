using System.Collections.ObjectModel;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionNarrativeSnapshot
{
    public Collection<string> StoryFlags { get; init; } = [];

    public Dictionary<string, int> RandomEventHistory { get; init; } = [];

    public Collection<string> PendingNarrativeScenes { get; init; } = [];

    public static GameSessionNarrativeSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionNarrativeSnapshot
        {
            StoryFlags = new Collection<string>([.. gameSession.StoryFlags]),
            RandomEventHistory = gameSession.RandomEventHistory.ToDictionary(static pair => pair.Key, static pair => pair.Value),
            PendingNarrativeScenes = new Collection<string>([.. gameSession.PendingNarrativeScenes])
        };
    }
}
