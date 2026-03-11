using FluentAssertions;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests.State;

internal sealed class GameStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeWithDefaultValues()
    {
        var state = new GameState();

        await Assert.That(state.RunId).IsNotEqualTo(Guid.Empty);
        await Assert.That(state.Clock.Day).IsEqualTo(1);
        await Assert.That(state.Player).IsNotNull();
        await Assert.That(state.World).IsNotNull();
        await Assert.That(state.IsGameOver).IsFalse();
    }

    [Test]
    public async Task EndDay_ShouldDeductRentFromMoney()
    {
        var state = new GameState();

        state.EndDay();

        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task EndDay_ShouldConsumeFoodAndFeedPlayer()
    {
        var state = new GameState();
        state.Player.Stats.ModifyHunger(-30);

        state.EndDay();

        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(2);
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(55);
    }

    [Test]
    public async Task EndDay_ShouldAdvanceToNextDay()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(10);

        state.EndDay();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
    }

    [Test]
    public async Task EndDay_ShouldApplyDailyDecayAndEat()
    {
        var state = new GameState();

        state.EndDay();

        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(85);
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(75);
    }

    [Test]
    public async Task RestAtHome_ShouldRestoreEnergyAndAdvanceTime()
    {
        var state = new GameState();

        state.RestAtHome();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(100);
        await Assert.That(state.Clock.Hour).IsEqualTo(14);
    }

    [Test]
    public async Task RestAtHome_ShouldTriggerEndDayWhenRestPassesCurfew()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(12);

        state.RestAtHome();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task TryTravelTo_ShouldSucceedWithEnoughMoney()
    {
        var state = new GameState();

        var result = state.TryTravelTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Market);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(98);
    }

    [Test]
    public async Task TryTravelTo_ShouldFailWithInsufficientMoney()
    {
        var state = new GameState();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TryTravelTo_ShouldAdvanceTimeByTravelDuration()
    {
        var state = new GameState();

        state.TryTravelTo(LocationId.Market);

        await Assert.That(state.Clock.Minute).IsEqualTo(15);
    }

    [Test]
    public async Task TryTravelTo_ShouldTriggerEndDayWhenTravelPassesCurfew()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(15);
        state.Clock.AdvanceMinutes(50);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
        await Assert.That(state.Clock.Minute).IsEqualTo(35);
    }

    [Test]
    public async Task WorkJob_ShouldTriggerEndDayWhenShiftPassesCurfew()
    {
        var state = new GameState();
        state.World.TravelTo(LocationId.Bakery);
        state.Clock.AdvanceHours(14);

        var result = state.WorkJob(Slums.Core.Jobs.JobRegistry.BakeryWork);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Clock.Minute).IsEqualTo(0);
    }

    [Test]
    public async Task IsGameOver_ShouldBeTrueWhenHealthIsZero()
    {
        var state = new GameState();
        state.Player.Stats.ModifyHealth(-100);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("health");
    }

    [Test]
    public async Task GameEvent_ShouldBeRaisedForActions()
    {
        var state = new GameState();
        var events = new List<string>();
        state.GameEvent += (_, e) => events.Add(e.Message);

        state.TryTravelTo(LocationId.Market);

        events.Should().ContainMatch("*Traveled*");
    }

    [Test]
    public async Task GetStatusSummary_ShouldReturnCurrentStatus()
    {
        var state = new GameState();

        var summary = state.GetStatusSummary();

        summary.Should().HaveCount(8);
        summary[0].Should().Contain("Day 1");
        summary[2].Should().Contain("Money");
    }
}
