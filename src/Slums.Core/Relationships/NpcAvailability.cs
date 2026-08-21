using Slums.Core.World;

namespace Slums.Core.Relationships;

public sealed record NpcAvailability
{
    public NpcId Npc { get; init; }
    public bool IsAvailable { get; init; }
    public LocationId? Location { get; init; }
    public string Reason { get; init; } = string.Empty;
}
