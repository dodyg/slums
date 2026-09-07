namespace Slums.Infrastructure.Persistence;

public sealed class DistrictHeatEntrySnapshot
{
    public string District { get; init; } = string.Empty;
    public int Heat { get; init; }
    public int DecayRate { get; init; }
    public int BaselineHeat { get; init; }
}
