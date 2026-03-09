using FluentAssertions;
using Slums.Core.Characters;
using Xunit;

namespace Slums.Core.Tests.Characters;

public class HouseholdStateTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var household = new HouseholdState();

        household.MotherAlive.Should().BeTrue();
        household.MotherHealth.Should().Be(70);
        household.FoodStockpile.Should().Be(3);
    }

    [Fact]
    public void ConsumeFood_ShouldReduceStockpile()
    {
        var household = new HouseholdState();

        household.ConsumeFood();

        household.FoodStockpile.Should().Be(2);
    }

    [Fact]
    public void ConsumeFood_ShouldNotGoBelowZero()
    {
        var household = new HouseholdState();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();

        household.FoodStockpile.Should().Be(0);
    }

    [Fact]
    public void HasEnoughFood_ShouldReturnFalseWhenEmpty()
    {
        var household = new HouseholdState();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();

        household.HasEnoughFood.Should().BeFalse();
    }

    [Fact]
    public void AddFood_ShouldIncreaseStockpile()
    {
        var household = new HouseholdState();

        household.AddFood(5);

        household.FoodStockpile.Should().Be(8);
    }

    [Fact]
    public void UpdateMotherHealth_ShouldClampToValidRange()
    {
        var household = new HouseholdState();

        household.UpdateMotherHealth(50);

        household.MotherHealth.Should().Be(100);

        household.UpdateMotherHealth(-150);

        household.MotherHealth.Should().Be(0);
    }

    [Fact]
    public void UpdateMotherHealth_ShouldSetMotherDeadWhenHealthReachesZero()
    {
        var household = new HouseholdState();

        household.UpdateMotherHealth(-100);

        household.MotherAlive.Should().BeFalse();
    }

    [Fact]
    public void MotherNeedsCare_ShouldReturnTrueWhenHealthBelow50()
    {
        var household = new HouseholdState();

        household.UpdateMotherHealth(-30);

        household.MotherNeedsCare.Should().BeTrue();
    }

    [Fact]
    public void ApplyDailyDecay_WithoutFood_ShouldDamageMother()
    {
        var household = new HouseholdState();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();

        household.ApplyDailyDecay();

        household.MotherHealth.Should().Be(65);
    }

    [Fact]
    public void ApplyDailyDecay_WithFood_ShouldNotDamageMotherFromHunger()
    {
        var household = new HouseholdState();

        household.ApplyDailyDecay();

        household.MotherHealth.Should().Be(70);
    }
}
