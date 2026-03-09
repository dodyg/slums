using FluentAssertions;
using Slums.Core.Characters;
using Xunit;

namespace Slums.Core.Tests.Characters;

public class SurvivalStatsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var stats = new SurvivalStats();

        stats.Money.Should().Be(100);
        stats.Hunger.Should().Be(80);
        stats.Energy.Should().Be(80);
        stats.Health.Should().Be(100);
        stats.Stress.Should().Be(20);
    }

    [Fact]
    public void ModifyHunger_ShouldClampToValidRange()
    {
        var stats = new SurvivalStats();

        stats.ModifyHunger(50);

        stats.Hunger.Should().Be(100);

        stats.ModifyHunger(-200);

        stats.Hunger.Should().Be(0);
    }

    [Fact]
    public void ModifyEnergy_ShouldClampToValidRange()
    {
        var stats = new SurvivalStats();

        stats.ModifyEnergy(-90);

        stats.Energy.Should().Be(0);

        stats.ModifyEnergy(150);

        stats.Energy.Should().Be(100);
    }

    [Fact]
    public void ModifyMoney_ShouldAllowNegativeForExpenses()
    {
        var stats = new SurvivalStats();

        stats.ModifyMoney(-50);

        stats.Money.Should().Be(50);
    }

    [Fact]
    public void ModifyMoney_ShouldNotGoBelowZero()
    {
        var stats = new SurvivalStats();

        stats.ModifyMoney(-150);

        stats.Money.Should().Be(0);
    }

    [Fact]
    public void Rest_ShouldIncreaseEnergyAndReduceStress()
    {
        var stats = new SurvivalStats();
        stats.ModifyEnergy(-50);
        stats.ModifyStress(30);

        stats.Rest();

        stats.Energy.Should().Be(60);
        stats.Stress.Should().Be(35);
        stats.Hunger.Should().Be(70);
    }

    [Fact]
    public void Eat_ShouldIncreaseHungerAndEnergy()
    {
        var stats = new SurvivalStats();
        stats.ModifyHunger(-50);

        stats.Eat(30);

        stats.Hunger.Should().Be(60);
        stats.Energy.Should().Be(87);
    }

    [Fact]
    public void ApplyDailyDecay_ShouldReduceStats()
    {
        var stats = new SurvivalStats();

        stats.ApplyDailyDecay();

        stats.Hunger.Should().Be(65);
        stats.Energy.Should().Be(70);
        stats.Stress.Should().Be(25);
    }

    [Fact]
    public void IsStarving_ShouldReturnTrueWhenHungerIsLow()
    {
        var stats = new SurvivalStats();
        stats.ModifyHunger(-75);

        stats.IsStarving.Should().BeTrue();
    }

    [Fact]
    public void IsExhausted_ShouldReturnTrueWhenEnergyIsLow()
    {
        var stats = new SurvivalStats();
        stats.ModifyEnergy(-75);

        stats.IsExhausted.Should().BeTrue();
    }

    [Fact]
    public void IsOverstressed_ShouldReturnTrueWhenStressIsHigh()
    {
        var stats = new SurvivalStats();
        stats.ModifyStress(70);

        stats.IsOverstressed.Should().BeTrue();
    }
}
