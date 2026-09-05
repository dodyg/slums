using Slums.Core.Technology;

namespace Slums.Application.Technology;

public sealed record TechnicalRepairMenuStatus(
    TechnicalRepairPreview Preview,
    bool CanPerform,
    string? UnavailabilityReason);
