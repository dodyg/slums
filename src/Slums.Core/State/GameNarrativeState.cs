namespace Slums.Core.State;

internal sealed class GameNarrativeState
{
    public Queue<string> PendingNarrativeScenes { get; } = new();

    public HashSet<string> StoryFlags { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, int> RandomEventHistory { get; } = new(StringComparer.OrdinalIgnoreCase);
}
