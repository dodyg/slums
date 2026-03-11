namespace Slums.Core.Relationships;

public sealed class RelationshipState
{
    private readonly Dictionary<NpcId, NpcRelationship> _npcRelationships = Enum
        .GetValues<NpcId>()
        .ToDictionary(static npcId => npcId, static npcId => new NpcRelationship(npcId, 0, 0));

    private readonly Dictionary<FactionId, FactionStanding> _factionStandings = Enum
        .GetValues<FactionId>()
        .ToDictionary(static factionId => factionId, static factionId => new FactionStanding(factionId, 0));

    public IReadOnlyDictionary<NpcId, NpcRelationship> NpcRelationships => _npcRelationships;

    public IReadOnlyDictionary<FactionId, FactionStanding> FactionStandings => _factionStandings;

    public NpcRelationship GetNpcRelationship(NpcId npcId)
    {
        return _npcRelationships.GetValueOrDefault(npcId) ?? new NpcRelationship(npcId, 0, 0);
    }

    public FactionStanding GetFactionStanding(FactionId factionId)
    {
        return _factionStandings.GetValueOrDefault(factionId) ?? new FactionStanding(factionId, 0);
    }

    public void SetNpcRelationship(NpcId npcId, int trust, int lastSeenDay)
    {
        _npcRelationships[npcId] = new NpcRelationship(npcId, trust, lastSeenDay);
    }

    public void SetFactionStanding(FactionId factionId, int reputation)
    {
        _factionStandings[factionId] = new FactionStanding(factionId, reputation);
    }
}