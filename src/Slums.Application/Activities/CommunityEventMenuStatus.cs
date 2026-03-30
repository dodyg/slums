using Slums.Core.Community;

namespace Slums.Application.Activities;

public sealed record CommunityEventMenuStatus(
    CommunityEventDefinition Event,
    bool CanAfford,
    bool HasTime,
    bool AlreadyAttendedThisWeek,
    bool CanAttend,
    string? UnavailabilityReason);
