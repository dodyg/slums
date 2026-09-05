using Slums.Core.Community;

namespace Slums.Application.Activities;

public sealed record CommunityActionMenuStatus(
    CommunityActionPreview Preview,
    bool CanPerform,
    string? UnavailabilityReason);
