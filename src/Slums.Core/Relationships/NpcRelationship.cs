namespace Slums.Core.Relationships;

public sealed record NpcRelationship(NpcId NpcId, int Trust, int LastSeenDay);