using Slums.Core.World;

namespace Slums.Core.Heat;

public sealed record DistrictHeatEntry(
    DistrictId District,
    int Heat,
    int DecayRate,
    int BaselineHeat)
{
    public DistrictHeatEntry WithHeat(int heat)
    {
        return this with { Heat = Math.Clamp(heat, 0, 100) };
    }

    public DistrictHeatEntry Decay()
    {
        var newHeat = Math.Max(BaselineHeat, Heat - DecayRate);
        return this with { Heat = newHeat };
    }
}
