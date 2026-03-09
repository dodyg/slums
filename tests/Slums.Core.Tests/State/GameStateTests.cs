using FluentAssertions;
using Slums.Core.State;
using Slums.Core.World;
using Xunit;

namespace Slums.Core.Tests.State;

public class GameStateTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var state = new GameState();

        state.RunId.Should().NotBe(Guid.Empty);
        state.Clock.Day.Should().Be(1);
        state.Player.Should().NotBeNull();
        state.World.Should().NotBeNull();
        state.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void EndDay_ShouldDeductRentFromMoney()
    {
        var state = new GameState();

        state.EndDay();

        state.Player.Stats.Money.Should().Be(80);
    }

    [Fact]
    public void EndDay_ShouldConsumeFoodAndFeedPlayer()
    {
        var state = new GameState();
        state.Player.Stats.ModifyHunger(-30);

        state.EndDay();

        state.Player.Household.FoodStockpile.Should().Be(2);
        state.Player.Stats.Hunger.Should().Be(55);
    }

    [Fact]
    public void EndDay_ShouldAdvanceToNextDay()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(10);

        state.EndDay();

        state.Clock.Day.Should().Be(2);
        state.Clock.Hour.Should().Be(6);
    }

    [Fact]
    public void EndDay_ShouldApplyDailyDecayAndEat()
    {
        var state = new GameState();

        state.EndDay();

        state.Player.Stats.Hunger.Should().Be(85);
        state.Player.Stats.Energy.Should().Be(75);
    }

    [Fact]
    public void RestAtHome_ShouldRestoreEnergyAndAdvanceTime()
    {
        var state = new GameState();

        state.RestAtHome();

        state.Player.Stats.Energy.Should().Be(100);
        state.Clock.Hour.Should().Be(14);
    }

    [Fact]
    public void TryTravelTo_ShouldSucceedWithEnoughMoney()
    {
        var state = new GameState();

        var result = state.TryTravelTo(LocationId.Market);

        result.Should().BeTrue();
        state.World.CurrentLocationId.Should().Be(LocationId.Market);
        state.Player.Stats.Money.Should().Be(98);
    }

    [Fact]
    public void TryTravelTo_ShouldFailWithInsufficientMoney()
    {
        var state = new GameState();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TryTravelTo(LocationId.CallCenter);

        result.Should().BeFalse();
        state.World.CurrentLocationId.Should().Be(LocationId.Home);
    }

    [Fact]
    public void TryTravelTo_ShouldAdvanceTimeByTravelDuration()
    {
        var state = new GameState();

        state.TryTravelTo(LocationId.Market);

        state.Clock.Minute.Should().Be(15);
    }

    [Fact]
    public void IsGameOver_ShouldBeTrueWhenHealthIsZero()
    {
        var state = new GameState();
        state.Player.Stats.ModifyHealth(-100);

        state.EndDay();

        state.IsGameOver.Should().BeTrue();
        state.GameOverReason.Should().Contain("health");
    }

    [Fact]
    public void GameEvent_ShouldBeRaisedForActions()
    {
        var state = new GameState();
        var events = new List<string>();
        state.GameEvent += (_, e) => events.Add(e.Message);

        state.TryTravelTo(LocationId.Market);

        events.Should().ContainMatch("*Traveled*");
    }

    [Fact]
    public void GetStatusSummary_ShouldReturnCurrentStatus()
    {
        var state = new GameState();

        var summary = state.GetStatusSummary();

        summary.Should().HaveCount(8);
        summary[0].Should().Contain("Day 1");
        summary[2].Should().Contain("Money");
    }
}
