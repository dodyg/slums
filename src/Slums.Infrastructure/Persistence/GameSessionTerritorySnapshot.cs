using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed class GameSessionTerritorySnapshot
{
    public IReadOnlyList<TerritoryDistrictSnapshot> Districts { get; init; } = [];

    public static GameSessionTerritorySnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionTerritorySnapshot
        {
            Districts = gameSession.Territory.Districts.Values
                .Select(static c => new TerritoryDistrictSnapshot
                {
                    District = c.District.ToString(),
                    FactionInfluence = c.FactionInfluence.ToDictionary(static kvp => kvp.Key.ToString(), static kvp => kvp.Value),
                    Tension = c.Tension,
                    LastConflictDay = c.LastConflictDay
                })
                .ToArray()
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        foreach (var entry in Districts)
        {
            if (!Enum.TryParse<DistrictId>(entry.District, out var district))
            {
                continue;
            }

            var influence = new Dictionary<FactionId, int>();
            foreach (var kvp in entry.FactionInfluence)
            {
                if (Enum.TryParse<FactionId>(kvp.Key, out var faction))
                {
                    influence[faction] = kvp.Value;
                }
            }

            gameSession.Territory.RestoreEntry(district, influence, entry.Tension, entry.LastConflictDay);
        }
    }
}

public sealed class TerritoryDistrictSnapshot
{
    public string District { get; init; } = string.Empty;
    public Dictionary<string, int> FactionInfluence { get; init; } = [];
    public int Tension { get; init; }
    public int LastConflictDay { get; init; }
}
