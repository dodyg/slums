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
        },
        new Location
        {
            Id = LocationId.Clinic,
            Name = "Rahma Clinic",
            Description = "A cramped low-cost clinic where waiting patients spill into the hallway.",
            District = DistrictId.ArdAlLiwa,
            HasJobOpportunities = true,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 25
        },
        new Location
        {
            Id = LocationId.Workshop,
            Name = "Abu Samir Sewing Workshop",
            Description = "A noisy garment workshop with irons hissing and fabric dust in the air.",
            District = DistrictId.ArdAlLiwa,
            HasJobOpportunities = true,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 20
        },
        new Location
        {
            Id = LocationId.Cafe,
            Name = "Ahwa El-Galaa",
            Description = "A Dokki street cafe serving tea, shai, and endless neighborhood gossip.",
            District = DistrictId.Dokki,
            HasJobOpportunities = true,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 35
        },
        new Location
        {
            Id = LocationId.Pharmacy,
            Name = "Saidaleya Al-Nahda",
            Description = "A discount pharmacy in Bulaq al-Dakrour with stacked boxes, tired fluorescent lights, and women comparing prices at the counter.",
            District = DistrictId.BulaqAlDakrour,
            HasJobOpportunities = true,
            HasCrimeOpportunities = false,
            TravelTimeMinutes = 30
        },
        new Location
        {
            Id = LocationId.Depot,
            Name = "Bulaq Microbus Depot",
            Description = "A chaotic transport yard where routes are shouted louder than engines and everybody is late for something.",
            District = DistrictId.BulaqAlDakrour,
            HasJobOpportunities = true,
            HasCrimeOpportunities = true,
            TravelTimeMinutes = 30
        },
        new Location
        {
            Id = LocationId.Laundry,
            Name = "Shubra Steam Laundry",
            Description = "A hot narrow laundry where steam, starch, and neighborhood gossip cling to everything at once.",
            District = DistrictId.Shubra,
            HasJobOpportunities = true,
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
