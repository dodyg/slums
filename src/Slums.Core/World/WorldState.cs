namespace Slums.Core.World;

using Slums.Core.Crimes;
using Slums.Core.Jobs;

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

public sealed class WorldState
{
    private readonly List<ActiveDistrictCondition> _activeDistrictConditions = [];
    private static IReadOnlyList<Location>? _locations;

    public DistrictId CurrentDistrict { get; private set; } = DistrictId.Imbaba;
    public LocationId CurrentLocationId { get; private set; } = LocationId.Home;
    public IReadOnlyList<ActiveDistrictCondition> ActiveDistrictConditions => _activeDistrictConditions;

    public static IReadOnlyList<Location> AllLocations => GetConfiguredLocations();

    public static void ConfigureLocations(IEnumerable<Location> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var configuredLocations = locations.Where(static location => location is not null).ToArray();
        if (configuredLocations.Length == 0)
        {
            throw new InvalidOperationException("At least one location must be configured.");
        }

        _locations = configuredLocations;
    }

    public Location? GetCurrentLocation()
    {
        return GetConfiguredLocations().FirstOrDefault(location => location.Id == CurrentLocationId);
    }

    public IEnumerable<Location> GetLocationsInCurrentDistrict()
    {
        return GetConfiguredLocations().Where(location => location.District == CurrentDistrict);
    }

    public IEnumerable<Location> GetTravelableLocations()
    {
        return GetConfiguredLocations().Where(location => location.Id != CurrentLocationId);
    }

    public void TravelTo(LocationId locationId)
    {
        var location = GetConfiguredLocations().FirstOrDefault(candidate => candidate.Id == locationId);
        if (location is not null)
        {
            CurrentLocationId = locationId;
            CurrentDistrict = location.District;
        }
    }

    public ActiveDistrictCondition? GetActiveDistrictCondition(DistrictId districtId)
    {
        return _activeDistrictConditions.FirstOrDefault(condition => condition.District == districtId);
    }

    public void SetActiveDistrictConditions(IEnumerable<ActiveDistrictCondition> activeDistrictConditions)
    {
        ArgumentNullException.ThrowIfNull(activeDistrictConditions);

        var configuredConditions = activeDistrictConditions
            .Where(static condition => condition is not null && !string.IsNullOrWhiteSpace(condition.ConditionId))
            .GroupBy(static condition => condition.District)
            .Select(static group => group.Last())
            .ToArray();

        _activeDistrictConditions.Clear();
        _activeDistrictConditions.AddRange(configuredConditions);
    }

    private static IReadOnlyList<Location> GetConfiguredLocations()
    {
        return _locations
            ?? throw new InvalidOperationException("Location content is not configured. Configure GameContentCatalog before querying locations.");
    }
}
