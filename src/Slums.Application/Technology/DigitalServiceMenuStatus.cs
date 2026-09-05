using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed record DigitalServiceMenuStatus(
    DigitalServicePreview Preview,
    bool CanPerform,
    string? UnavailabilityReason);
