using Slums.Core.Entertainment;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Tests.Entertainment;

public sealed class EntertainmentTests
{
    [Test]
    public async Task EntertainmentRegistry_ShouldReturnAllActivities()
    {
        var activities = EntertainmentRegistry.AllActivities;

        await Assert.That(activities).HasCount(6);
    }

    [Test]
    public async Task EntertainmentRegistry_ShouldFilterByCafe()
    {
        var activities = EntertainmentRegistry.GetActivitiesForLocation(hasCafe: true, hasBar: false, hasBilliards: false).ToList();

        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Coffee)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Shisha)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.FootballWatching)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.SocialHangout)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.BarDrinking)).IsFalse();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Billiards)).IsFalse();
    }

    [Test]
    public async Task EntertainmentRegistry_ShouldFilterByBar()
    {
        var activities = EntertainmentRegistry.GetActivitiesForLocation(hasCafe: false, hasBar: true, hasBilliards: false).ToList();

        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.BarDrinking)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Coffee)).IsFalse();
    }

    [Test]
    public async Task EntertainmentRegistry_ShouldFilterByBilliards()
    {
        var activities = EntertainmentRegistry.GetActivitiesForLocation(hasCafe: false, hasBar: false, hasBilliards: true).ToList();

        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Billiards)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Coffee)).IsFalse();
    }

    [Test]
    public async Task GameSession_GetAvailableEntertainmentActivities_ShouldReturnEmptyAtHome()
    {
        using var state = new GameSession();

        var activities = state.GetAvailableEntertainmentActivities();

        await Assert.That(activities).IsEmpty();
    }

    [Test]
    public async Task GameSession_GetAvailableEntertainmentActivities_ShouldReturnCafeActivitiesAtCafe()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);

        var activities = state.GetAvailableEntertainmentActivities();

        await Assert.That(activities.Count).IsGreaterThan(0);
        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.Coffee)).IsTrue();
    }

    [Test]
    public async Task GameSession_GetAvailableEntertainmentActivities_ShouldReturnBarActivitiesAtSquare()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Square);

        var activities = state.GetAvailableEntertainmentActivities();

        await Assert.That(activities.Any(a => a.Type == EntertainmentActivityType.BarDrinking)).IsTrue();
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_ShouldReduceStress()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);
        state.Player.Stats.SetStress(50);
        var coffee = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Coffee);

        var result = state.TryPerformEntertainment(coffee);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(42);
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_ShouldCostMoney()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);
        var moneyBefore = state.Player.Stats.Money;
        var coffee = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Coffee);

        var result = state.TryPerformEntertainment(coffee);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(moneyBefore - coffee.BaseCost);
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_ShouldAdvanceTime()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);
        var coffee = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Coffee);

        var result = state.TryPerformEntertainment(coffee);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Clock.Minute).IsEqualTo(coffee.DurationMinutes);
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_ShouldFailIfNotEnoughMoney()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);
        state.Player.Stats.ModifyMoney(-100);
        var coffee = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Coffee);

        var result = state.TryPerformEntertainment(coffee);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_ShouldFailIfNotEnoughEnergy()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Cafe);
        state.Player.Stats.SetEnergy(0);
        var shisha = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Shisha);

        var result = state.TryPerformEntertainment(shisha);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_ShouldFailIfActivityNotAvailableAtLocation()
    {
        using var state = new GameSession();
        var coffee = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Coffee);

        var result = state.TryPerformEntertainment(coffee);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformEntertainment_BilliardsShouldCostEnergy()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Depot);
        var energyBefore = state.Player.Stats.Energy;
        var billiards = EntertainmentRegistry.AllActivities.First(a => a.Type == EntertainmentActivityType.Billiards);

        var result = state.TryPerformEntertainment(billiards);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(energyBefore - billiards.EnergyCost);
    }

    [Test]
    public async Task Location_HasCafe_ShouldBeTrueForCafe()
    {
        var cafe = WorldState.AllLocations.First(l => l.Id == LocationId.Cafe);

        await Assert.That(cafe!.HasCafe).IsTrue();
    }

    [Test]
    public async Task Location_HasBar_ShouldBeTrueForSquare()
    {
        var square = WorldState.AllLocations.First(l => l.Id == LocationId.Square);

        await Assert.That(square!.HasBar).IsTrue();
    }

    [Test]
    public async Task Location_HasBilliards_ShouldBeTrueForDepot()
    {
        var depot = WorldState.AllLocations.First(l => l.Id == LocationId.Depot);

        await Assert.That(depot!.HasBilliards).IsTrue();
    }
}
