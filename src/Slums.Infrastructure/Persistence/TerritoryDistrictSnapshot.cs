namespace Slums.Infrastructure.Persistence;

public sealed class TerritoryDistrictSnapshot
{
    public string District { get; init; } = string.Empty;
    public Dictionary<string, int> FactionInfluence { get; init; } = [];
    public int Tension { get; init; }
    public int LastConflictDay { get; init; }
}
