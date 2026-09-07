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
            var district = SaveValueParser.ParseEnum<DistrictId>(entry.District, "district heat district");
            gameSession.DistrictHeat.RestoreEntry(district, entry.Heat, entry.DecayRate, entry.BaselineHeat);
        }

        gameSession.DistrictHeat.DecayRateModifier = DecayRateModifier;
    }
}
