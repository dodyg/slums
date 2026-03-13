using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionRelationshipSnapshot
{
    public Dictionary<string, GameSessionNpcRelationshipSnapshot> Npcs { get; init; } = [];

    public Dictionary<string, int> Factions { get; init; } = [];

    public static GameSessionRelationshipSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionRelationshipSnapshot
        {
            Npcs = gameSession.Relationships.NpcRelationships.ToDictionary(
                static pair => pair.Key.ToString(),
                static pair => GameSessionNpcRelationshipSnapshot.Capture(pair.Value)),
            Factions = gameSession.Relationships.FactionStandings.ToDictionary(
                static pair => pair.Key.ToString(),
                static pair => pair.Value.Reputation)
        };
    }

    public GameSessionNpcRelationshipSnapshot GetNpcSnapshot(NpcId npcId)
    {
        return Npcs.GetValueOrDefault(npcId.ToString()) ?? new GameSessionNpcRelationshipSnapshot();
    }

    public int GetFactionReputation(FactionId factionId)
    {
        return Factions.GetValueOrDefault(factionId.ToString());
    }
}
