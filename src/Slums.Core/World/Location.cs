using Slums.Core.Crimes;
using Slums.Core.Jobs;

namespace Slums.Core.World;

public sealed class Location
{
    public LocationId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DistrictId District { get; init; }
    public bool HasJobOpportunities { get; init; }
    public bool HasCrimeOpportunities { get; init; }
    public IReadOnlyList<JobType> AvailableJobTypes { get; init; } = [];
    public IReadOnlyList<CrimeType> AvailableCrimeTypes { get; init; } = [];
    public bool HasClinicServices { get; init; }
    public int ClinicVisitBaseCost { get; init; }
    public IReadOnlyList<DayOfWeek> ClinicOpenDays { get; init; } = [];
    public int TravelTimeMinutes { get; init; } = 30;
    public bool HasCafe { get; init; }
    public bool HasBar { get; init; }
    public bool HasBilliards { get; init; }
}
