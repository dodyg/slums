using Slums.Core.Entertainment;

namespace Slums.Application.Activities;

public sealed record EntertainmentMenuStatus(
    EntertainmentActivity Activity,
    bool CanAfford,
    bool HasEnergy,
    bool CanPerform,
    string? UnavailabilityReason);
