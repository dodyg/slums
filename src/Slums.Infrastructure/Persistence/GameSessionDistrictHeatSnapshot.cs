using Slums.Core.Heat;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed class GameSessionDistrictHeatSnapshot
{
    public IReadOnlyList<DistrictHeatEntrySnapshot> Entries { get; init; } = [];
    public double DecayRateModifier { get; init; }

    public static GameSessionDistrictHeatSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionDistrictHeatSnapshot
        {
            Entries = gameSession.DistrictHeat.Entries.Values
                .Select(static e => new DistrictHeatEntrySnapshot
                {
                    District = e.District.ToString(),
                    Heat = e.Heat,
                    DecayRate = e.DecayRate,
                    BaselineHeat = e.BaselineHeat
                })
                .ToArray(),
            DecayRateModifier = gameSession.DistrictHeat.DecayRateModifier
        };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        foreach (var entry in Entries)
        {
            if (!Enum.TryParse<DistrictId>(entry.District, out var district))
            {
                continue;
            }

            gameSession.DistrictHeat.RestoreEntry(district, entry.Heat, entry.DecayRate, entry.BaselineHeat);
        }

        gameSession.DistrictHeat.DecayRateModifier = DecayRateModifier;
    }
}

public sealed class DistrictHeatEntrySnapshot
{
    public string District { get; init; } = string.Empty;
    public int Heat { get; init; }
    public int DecayRate { get; init; }
    public int BaselineHeat { get; init; }
}
