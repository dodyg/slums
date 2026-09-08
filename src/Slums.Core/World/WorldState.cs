namespace Slums.Core.World;

using Slums.Core.Crimes;
using Slums.Core.Jobs;

public sealed class WorldState
{
    private readonly List<ActiveDistrictCondition> _activeDistrictConditions = [];
    private static IReadOnlyList<Location>? _configuredLocations;
    private readonly IReadOnlyList<Location> _locations;

    public WorldState(IEnumerable<Location>? locations = null)
    {
        _locations = locations is null
            ? GetConfiguredLocations()
            : Array.AsReadOnly(locations.Where(static location => location is not null).ToArray());
    }

    public DistrictId CurrentDistrict { get; private set; } = DistrictId.Imbaba;
    public LocationId CurrentLocationId { get; private set; } = LocationId.Home;
    public IReadOnlyList<ActiveDistrictCondition> ActiveDistrictConditions => _activeDistrictConditions;
    public IReadOnlyList<Location> Locations => _locations;

    public static IReadOnlyList<Location> AllLocations => GetConfiguredLocations();

    public static void ConfigureLocations(IEnumerable<Location> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var configuredLocations = locations.Where(static location => location is not null).ToArray();
        if (configuredLocations.Length == 0)
        {
            throw new InvalidOperationException("At least one location must be configured.");
        }

        _configuredLocations = configuredLocations;
    }

    public Location? GetLocationById(LocationId locationId)
    {
        return _locations.FirstOrDefault(location => location.Id == locationId);
    }

    public Location? GetCurrentLocation()
    {
        return GetLocationById(CurrentLocationId);
    }

    public IEnumerable<Location> GetLocationsInCurrentDistrict()
    {
        return _locations.Where(location => location.District == CurrentDistrict);
    }

    public IEnumerable<Location> GetTravelableLocations()
    {
        return _locations.Where(location => location.Id != CurrentLocationId);
    }

    public void TravelTo(LocationId locationId)
    {
        var location = GetLocationById(locationId);
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
        return _configuredLocations
            ?? throw new InvalidOperationException("Location content is not configured. Configure GameContentCatalog before querying locations.");
    }
}
