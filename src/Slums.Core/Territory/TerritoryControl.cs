using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Territory;

public sealed record TerritoryControl(
    DistrictId District,
    Dictionary<FactionId, int> FactionInfluence,
    int Tension,
    int LastConflictDay)
{
    public FactionId? ControllingFaction
    {
        get
        {
            FactionId? best = null;
            var bestValue = 0;
            foreach (var kvp in FactionInfluence)
            {
                if (kvp.Value > bestValue)
                {
                    bestValue = kvp.Value;
                    best = kvp.Key;
                }
            }

            return bestValue >= 50 ? best : null;
        }
    }

    public TensionLevel TensionLevel => Tension switch
    {
        <= 30 => Territory.TensionLevel.Normal,
        <= 50 => Territory.TensionLevel.Elevated,
        <= 70 => Territory.TensionLevel.High,
        _ => Territory.TensionLevel.Dangerous
    };

    public TerritoryControl SetInfluence(FactionId faction, int value)
    {
        var updated = new Dictionary<FactionId, int>(FactionInfluence)
        {
            [faction] = Math.Clamp(value, 0, 100)
        };
        return this with { FactionInfluence = updated };
    }

    public TerritoryControl ModifyInfluence(FactionId faction, int delta)
    {
        var current = FactionInfluence.TryGetValue(faction, out var v) ? v : 0;
        return SetInfluence(faction, current + delta);
    }

    public TerritoryControl ModifyTension(int delta)
    {
        return this with { Tension = Math.Clamp(Tension + delta, 0, 100) };
    }

    public TerritoryControl ModifyAllInfluence(int delta)
    {
        var updated = new Dictionary<FactionId, int>(FactionInfluence);
        foreach (var faction in updated.Keys.ToList())
        {
            updated[faction] = Math.Clamp(updated[faction] + delta, 0, 100);
        }

        return this with { FactionInfluence = updated };
    }
}
