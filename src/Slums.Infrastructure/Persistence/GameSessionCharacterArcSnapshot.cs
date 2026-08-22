using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionCharacterArcSnapshot
{
    public Dictionary<string, int> Beats { get; init; } = [];
    public Dictionary<string, string> Decisions { get; init; } = [];

    public static GameSessionCharacterArcSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new GameSessionCharacterArcSnapshot
        {
            Beats = gameSession.CentralCharacterArcs.Beats.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value),
            Decisions = gameSession.CentralCharacterArcs.Decisions.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.ToString())
        };
    }
}
