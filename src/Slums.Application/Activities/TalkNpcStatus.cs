using Slums.Core.Relationships;

namespace Slums.Application.Activities;

public sealed record TalkNpcStatus(
    NpcId NpcId,
    string Name,
    int Trust,
    string Summary,
    string? FactionLink,
    IReadOnlyList<string> MemoryFlags);