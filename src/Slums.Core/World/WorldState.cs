namespace Slums.Core.World;

using Slums.Core.Crimes;
using Slums.Core.Jobs;

public sealed class WorldState
{
    private readonly List<ActiveDistrictCondition> _activeDistrictConditions = [];
    private readonly IReadOnlyList<Location> _locations;

    public WorldState(IEnumerable<Location> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var locationList = locations.Where(static location => location is not null).ToArray();
        if (locationList.Length == 0)
        {
            throw new ArgumentException("At least one location must be provided.", nameof(locations));
        }

        _locations = Array.AsReadOnly(locationList);
    }

    public DistrictId CurrentDistrict { get; private set; } = DistrictId.Imbaba;
    public LocationId CurrentLocationId { get; private set; } = LocationId.Home;
    public IReadOnlyList<ActiveDistrictCondition> ActiveDistrictConditions => _activeDistrictConditions;
    public IReadOnlyList<Location> Locations => _locations;

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
}
