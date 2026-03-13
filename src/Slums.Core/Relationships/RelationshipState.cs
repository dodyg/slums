namespace Slums.Core.Relationships;

public sealed class RelationshipState
{
    private readonly Dictionary<NpcId, NpcRelationship> _npcRelationships = Enum
        .GetValues<NpcId>()
        .ToDictionary(static npcId => npcId, static npcId => new NpcRelationship(npcId));

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
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with
        {
            Trust = trust,
            LastSeenDay = lastSeenDay
        };
    }

    public void SetNpcRelationshipMemory(
        NpcId npcId,
        int lastFavorDay,
        int lastRefusalDay,
        bool hasUnpaidDebt,
        bool wasEmbarrassed,
        bool wasHelped,
        int recentContactCount)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with
        {
            LastFavorDay = Math.Max(0, lastFavorDay),
            LastRefusalDay = Math.Max(0, lastRefusalDay),
            HasUnpaidDebt = hasUnpaidDebt,
            WasEmbarrassed = wasEmbarrassed,
            WasHelped = wasHelped,
            RecentContactCount = Math.Max(0, recentContactCount)
        };
    }

    public void RecordFavor(NpcId npcId, int currentDay, bool hasUnpaidDebt = false)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with
        {
            LastFavorDay = Math.Max(0, currentDay),
            HasUnpaidDebt = hasUnpaidDebt,
            WasHelped = true,
            RecentContactCount = existing.RecentContactCount + 1
        };
    }

    public void RecordRefusal(NpcId npcId, int currentDay)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with
        {
            LastRefusalDay = Math.Max(0, currentDay),
            RecentContactCount = existing.RecentContactCount + 1
        };
    }

    public void SetDebtState(NpcId npcId, bool hasUnpaidDebt)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with { HasUnpaidDebt = hasUnpaidDebt };
    }

    public void SetEmbarrassedState(NpcId npcId, bool wasEmbarrassed)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with { WasEmbarrassed = wasEmbarrassed };
    }

    public void SetHelpedState(NpcId npcId, bool wasHelped)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with { WasHelped = wasHelped };
    }

    public void RecordContact(NpcId npcId, int currentDay)
    {
        var existing = GetNpcRelationship(npcId);
        _npcRelationships[npcId] = existing with
        {
            LastSeenDay = Math.Max(existing.LastSeenDay, currentDay),
            RecentContactCount = existing.RecentContactCount + 1
        };
    }

    public void SetFactionStanding(FactionId factionId, int reputation)
    {
        _factionStandings[factionId] = new FactionStanding(factionId, reputation);
    }

    public void RecordSeenConversation(NpcId npcId, string knotName)
    {
        if (string.IsNullOrWhiteSpace(knotName))
        {
            return;
        }

        var existing = GetNpcRelationship(npcId);
        var seenKnots = new HashSet<string>(existing.SeenConversationKnots) { knotName };
        _npcRelationships[npcId] = existing with { SeenConversationKnots = seenKnots };
    }

    public void RestoreConversationHistory(NpcId npcId, IEnumerable<string> seenKnots)
    {
        ArgumentNullException.ThrowIfNull(seenKnots);

        var existing = GetNpcRelationship(npcId);
        var knotSet = new HashSet<string>(seenKnots.Where(static k => !string.IsNullOrWhiteSpace(k)));
        _npcRelationships[npcId] = existing with { SeenConversationKnots = knotSet };
    }

    public bool HasSeenConversation(NpcId npcId, string knotName)
    {
        if (string.IsNullOrWhiteSpace(knotName))
        {
            return false;
        }

        var relationship = GetNpcRelationship(npcId);
        return relationship.SeenConversationKnots.Contains(knotName);
    }
}