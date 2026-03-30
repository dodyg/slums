using Slums.Core.World;

namespace Slums.Core.Heat;

public sealed class DistrictHeatState
{
    private readonly Dictionary<DistrictId, DistrictHeatEntry> _entries;
    private double _decayRateModifier = 1.0;

    public DistrictHeatState()
    {
        _entries = Enum.GetValues<DistrictId>()
            .Select(d => new DistrictHeatEntry(d, 0, HeatDecayRates.GetDecayRate(d), 0))
            .ToDictionary(static e => e.District);
    }

    public static DistrictHeatState CreateDefault() => new();

    public double DecayRateModifier
    {
        get => _decayRateModifier;
        set => _decayRateModifier = value;
    }

    public int GetHeat(DistrictId district)
    {
        return _entries.TryGetValue(district, out var entry) ? entry.Heat : 0;
    }

    public void AddHeat(DistrictId district, int amount)
    {
        if (!_entries.TryGetValue(district, out var entry))
        {
            return;
        }

        _entries[district] = entry.WithHeat(entry.Heat + amount);
    }

    public void SetHeat(DistrictId district, int heat)
    {
        if (!_entries.TryGetValue(district, out var entry))
        {
            return;
        }

        _entries[district] = entry.WithHeat(heat);
    }

    public void SetHeatAll(int heat)
    {
        var clamped = Math.Clamp(heat, 0, 100);
        foreach (var district in _entries.Keys.ToList())
        {
            _entries[district] = _entries[district].WithHeat(clamped);
        }
    }

    public void DecayAll()
    {
        foreach (var district in _entries.Keys.ToList())
        {
            var entry = _entries[district];
            var effectiveDecay = Math.Max(1, (int)(entry.DecayRate * _decayRateModifier));
            var newHeat = Math.Max(entry.BaselineHeat, entry.Heat - effectiveDecay);
            _entries[district] = entry with { Heat = newHeat };
        }
    }

    public void ApplyBleedOver()
    {
        foreach (var (from, to, rate) in HeatBleedOverTable.Relationships)
        {
            var fromHeat = GetHeat(from);
            var toHeat = GetHeat(to);
            if (fromHeat > toHeat)
            {
                var transfer = (int)((fromHeat - toHeat) * rate);
                if (transfer > 0)
                {
                    AddHeat(from, -transfer);
                    AddHeat(to, transfer);
                }
            }
        }
    }

    public int GetGlobalPressure()
    {
        return _entries.Values.Count > 0 ? _entries.Values.Max(static e => e.Heat) : 0;
    }

    public IReadOnlyList<DistrictId> GetHighHeatDistricts(int threshold)
    {
        return _entries.Values
            .Where(e => e.Heat > threshold)
            .Select(static e => e.District)
            .ToList();
    }

    public IReadOnlyDictionary<DistrictId, DistrictHeatEntry> Entries => _entries;

    public void SetBaselineHeat(DistrictId district, int baseline)
    {
        if (!_entries.TryGetValue(district, out var entry))
        {
            return;
        }

        _entries[district] = entry with { BaselineHeat = baseline, Heat = Math.Max(entry.Heat, baseline) };
    }

    public void RestoreEntry(DistrictId district, int heat, int decayRate, int baselineHeat)
    {
        _entries[district] = new DistrictHeatEntry(district, heat, decayRate, baselineHeat);
    }
}
