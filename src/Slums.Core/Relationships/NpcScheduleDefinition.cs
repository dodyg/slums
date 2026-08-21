using Slums.Core.Clock;
using Slums.Core.World;

namespace Slums.Core.Relationships;

public sealed record NpcScheduleDefinition
{
    public NpcId Npc { get; init; }
    public IReadOnlyList<GameDayOfWeek> Days { get; init; } = [];
    public int StartMinute { get; init; }
    public int EndMinute { get; init; }
    public LocationId Location { get; init; }
    public string AbsenceReason { get; init; } = string.Empty;
    public string? ConditionId { get; init; }
}
