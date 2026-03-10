using FluentAssertions;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.World;

public class WorldStateTests
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

        locations.Should().HaveCount(5);
        locations.Select(l => l.Id).Should().Contain(
            new[] { LocationId.Home, LocationId.Market, LocationId.Bakery, LocationId.CallCenter, LocationId.Square });
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

        locations.Should().HaveCount(2);
        locations.All(l => l.District == DistrictId.Dokki).Should().BeTrue();
    }

    [Test]
    public async Task GetTravelableLocations_ShouldExcludeCurrentLocation()
    {
        var world = new WorldState();

        var locations = world.GetTravelableLocations();

        locations.Should().HaveCount(4);
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
}
