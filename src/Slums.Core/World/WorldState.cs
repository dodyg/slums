namespace Slums.Core.World;

public sealed class Location
{
    public LocationId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DistrictId District { get; init; }
    public bool HasJobOpportunities { get; init; }
    public bool HasCrimeOpportunities { get; init; }
    public int TravelTimeMinutes { get; init; } = 30;
}

public sealed class WorldState
{
    public DistrictId CurrentDistrict { get; private set; } = DistrictId.Imbaba;
    public LocationId CurrentLocationId { get; private set; } = LocationId.Home;

    private static readonly Location[] DefaultLocations =
    [
        new Location
        {
            Id = LocationId.Home,
            Name = "Your Apartment",
            Description = "A small two-room flat you share with your mother.",
            District = DistrictId.Imbaba,
            HasJobOpportunities = false,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 0
        },
        new Location
        {
            Id = LocationId.Market,
            Name = "Souk Al-Gom'a",
            Description = "The Friday market, busy with vendors and shoppers.",
            District = DistrictId.Imbaba,
            HasJobOpportunities = true,
            HasCrimeOpportunities = true,
            TravelTimeMinutes = 15
        },
        new Location
        {
            Id = LocationId.Bakery,
            Name = "Al-Forn Al-Baladi",
            Description = "A traditional bakery where bread is baked in stone ovens.",
            District = DistrictId.Imbaba,
            HasJobOpportunities = true,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 10
        },
        new Location
        {
            Id = LocationId.CallCenter,
            Name = "TechConnect Office",
            Description = "A modern call center serving international clients.",
            District = DistrictId.Dokki,
            HasJobOpportunities = true,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 45
        },
        new Location
        {
            Id = LocationId.Square,
            Name = "Midan Al-Tahrir",
            Description = "The busy central square connecting districts.",
            District = DistrictId.Dokki,
            HasJobOpportunities = false,
            HasCrimeOpportunities = true,
            TravelTimeMinutes = 40
        }
    ];

    private static IReadOnlyList<Location> _locations = DefaultLocations;

    public static IReadOnlyList<Location> AllLocations => _locations;

    public static void ConfigureLocations(IEnumerable<Location> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var configuredLocations = locations.Where(static location => location is not null).ToArray();
        if (configuredLocations.Length > 0)
        {
            _locations = configuredLocations;
        }
    }

    public Location? GetCurrentLocation()
    {
        return _locations.FirstOrDefault(l => l.Id == CurrentLocationId);
    }

    public IEnumerable<Location> GetLocationsInCurrentDistrict()
    {
        return _locations.Where(l => l.District == CurrentDistrict);
    }

    public IEnumerable<Location> GetTravelableLocations()
    {
        return _locations.Where(l => l.Id != CurrentLocationId);
    }

    public void TravelTo(LocationId locationId)
    {
        var location = _locations.FirstOrDefault(l => l.Id == locationId);
        if (location is not null)
        {
            CurrentLocationId = locationId;
            CurrentDistrict = location.District;
        }
    }
}
