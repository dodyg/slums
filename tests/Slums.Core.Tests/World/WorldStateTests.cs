using FluentAssertions;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.World;

internal sealed class WorldStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeAtHomeInImbaba()
    {
        var world = new WorldState();

        await Assert.That(world.CurrentLocationId).IsEqualTo(LocationId.Home);
        await Assert.That(world.CurrentDistrict).IsEqualTo(DistrictId.Imbaba);
    }

    [Test]
    public async Task GetCurrentLocation_ShouldReturnHomeInitially()
    {
        var world = new WorldState();

        var location = world.GetCurrentLocation();

        await Assert.That(location).IsNotNull();
        await Assert.That(location!.Id).IsEqualTo(LocationId.Home);
        await Assert.That(location.Name).Contains("Apartment");
    }

    [Test]
    public async Task TravelTo_ShouldChangeCurrentLocation()
    {
        var world = new WorldState();

        world.TravelTo(LocationId.Market);

        await Assert.That(world.CurrentLocationId).IsEqualTo(LocationId.Market);
        await Assert.That(world.CurrentDistrict).IsEqualTo(DistrictId.Imbaba);
    }

    [Test]
    public async Task TravelTo_ShouldChangeDistrict_WhenTravelingToDifferentDistrict()
    {
        var world = new WorldState();

        world.TravelTo(LocationId.CallCenter);

        await Assert.That(world.CurrentLocationId).IsEqualTo(LocationId.CallCenter);
        await Assert.That(world.CurrentDistrict).IsEqualTo(DistrictId.Dokki);
    }

    [Test]
    public async Task TravelTo_ShouldNotChangeLocation_WhenLocationNotFound()
    {
        var world = new WorldState();

        world.TravelTo(new LocationId("nonexistent"));

        await Assert.That(world.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task AllLocations_ShouldContainAllDefinedLocations()
    {
        var locations = WorldState.AllLocations;

        locations.Should().HaveCount(11);
        locations.Select(l => l.Id).Should().Contain(
            new[] { LocationId.Home, LocationId.Market, LocationId.Bakery, LocationId.CallCenter, LocationId.Square, LocationId.Clinic, LocationId.Workshop, LocationId.Cafe, LocationId.Pharmacy, LocationId.Depot, LocationId.Laundry });
    }

    [Test]
    public async Task GetLocationsInCurrentDistrict_ShouldReturnLocationsInSameDistrict()
    {
        var world = new WorldState();

        var locations = world.GetLocationsInCurrentDistrict();

        locations.Should().HaveCount(3);
        locations.All(l => l.District == DistrictId.Imbaba).Should().BeTrue();
    }

    [Test]
    public async Task GetLocationsInCurrentDistrict_ShouldReturnDokkiLocations_WhenInDokki()
    {
        var world = new WorldState();
        world.TravelTo(LocationId.CallCenter);

        var locations = world.GetLocationsInCurrentDistrict();

        locations.Should().HaveCount(3);
        locations.All(l => l.District == DistrictId.Dokki).Should().BeTrue();
    }

    [Test]
    public async Task GetTravelableLocations_ShouldExcludeCurrentLocation()
    {
        var world = new WorldState();

        var locations = world.GetTravelableLocations();

        locations.Should().HaveCount(10);
        locations.Any(l => l.Id == LocationId.Home).Should().BeFalse();
    }

    [Test]
    public async Task HomeLocation_ShouldHaveNoJobsOrCrime()
    {
        var home = WorldState.AllLocations.First(l => l.Id == LocationId.Home);

        await Assert.That(home.HasJobOpportunities).IsFalse();
        await Assert.That(home.HasCrimeOpportunities).IsFalse();
        await Assert.That(home.TravelTimeMinutes).IsEqualTo(0);
    }

    [Test]
    public async Task MarketLocation_ShouldHaveJobsAndCrime()
    {
        var market = WorldState.AllLocations.First(l => l.Id == LocationId.Market);

        await Assert.That(market.HasJobOpportunities).IsTrue();
        await Assert.That(market.HasCrimeOpportunities).IsTrue();
    }

    [Test]
    public async Task BakeryLocation_ShouldHaveJobsButNoCrime()
    {
        var bakery = WorldState.AllLocations.First(l => l.Id == LocationId.Bakery);

        await Assert.That(bakery.HasJobOpportunities).IsTrue();
        await Assert.That(bakery.HasCrimeOpportunities).IsFalse();
    }

    [Test]
    public async Task CallCenterLocation_ShouldHaveJobsButNoCrime()
    {
        var callCenter = WorldState.AllLocations.First(l => l.Id == LocationId.CallCenter);

        await Assert.That(callCenter.HasJobOpportunities).IsTrue();
        await Assert.That(callCenter.HasCrimeOpportunities).IsFalse();
    }

    [Test]
    public async Task SquareLocation_ShouldHaveCrimeButNoJobs()
    {
        var square = WorldState.AllLocations.First(l => l.Id == LocationId.Square);

        await Assert.That(square.HasJobOpportunities).IsFalse();
        await Assert.That(square.HasCrimeOpportunities).IsTrue();
    }

    [Test]
    public async Task ClinicLocation_ShouldSupportWorkInArdAlLiwa()
    {
        var clinic = WorldState.AllLocations.First(l => l.Id == LocationId.Clinic);

        await Assert.That(clinic.HasJobOpportunities).IsTrue();
        await Assert.That(clinic.HasCrimeOpportunities).IsFalse();
        await Assert.That(clinic.HasClinicServices).IsTrue();
        await Assert.That(clinic.ClinicVisitBaseCost).IsEqualTo(35);
        await Assert.That(clinic.District).IsEqualTo(DistrictId.ArdAlLiwa);
    }

    [Test]
    public async Task WorkshopLocation_ShouldSupportWorkInArdAlLiwa()
    {
        var workshop = WorldState.AllLocations.First(l => l.Id == LocationId.Workshop);

        await Assert.That(workshop.HasJobOpportunities).IsTrue();
        await Assert.That(workshop.HasCrimeOpportunities).IsFalse();
        await Assert.That(workshop.District).IsEqualTo(DistrictId.ArdAlLiwa);
    }

    [Test]
    public async Task CafeLocation_ShouldSupportWorkInDokki()
    {
        var cafe = WorldState.AllLocations.First(l => l.Id == LocationId.Cafe);

        await Assert.That(cafe.HasJobOpportunities).IsTrue();
        await Assert.That(cafe.HasCrimeOpportunities).IsFalse();
        await Assert.That(cafe.District).IsEqualTo(DistrictId.Dokki);
    }

    [Test]
    public async Task PharmacyAndDepot_ShouldSupportWorkInBulaqAlDakrour()
    {
        var pharmacy = WorldState.AllLocations.First(l => l.Id == LocationId.Pharmacy);
        var depot = WorldState.AllLocations.First(l => l.Id == LocationId.Depot);

        await Assert.That(pharmacy.HasJobOpportunities).IsTrue();
        await Assert.That(pharmacy.HasClinicServices).IsTrue();
        await Assert.That(pharmacy.ClinicVisitBaseCost).IsEqualTo(46);
        await Assert.That(pharmacy.District).IsEqualTo(DistrictId.BulaqAlDakrour);
        await Assert.That(depot.HasJobOpportunities).IsTrue();
        await Assert.That(depot.HasCrimeOpportunities).IsTrue();
        await Assert.That(depot.District).IsEqualTo(DistrictId.BulaqAlDakrour);
    }

    [Test]
    public async Task Laundry_ShouldSupportWorkInShubra()
    {
        var laundry = WorldState.AllLocations.First(l => l.Id == LocationId.Laundry);

        await Assert.That(laundry.HasJobOpportunities).IsTrue();
        await Assert.That(laundry.HasCrimeOpportunities).IsTrue();
        await Assert.That(laundry.District).IsEqualTo(DistrictId.Shubra);
    }
}
