using FluentAssertions;
using Slums.Core.Characters;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests.Characters;

internal sealed class HouseholdStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeWithDefaultValues()
    {
        var household = new HouseholdState();

        await Assert.That(household.MotherAlive).IsTrue();
        await Assert.That(household.MotherHealth).IsEqualTo(70);
        await Assert.That(household.FoodStockpile).IsEqualTo(3);
    }

    [Test]
    public async Task ConsumeFood_ShouldReduceStockpile()
    {
        var household = new HouseholdState();

        household.ConsumeFood();

        await Assert.That(household.FoodStockpile).IsEqualTo(2);
    }

    [Test]
    public async Task ConsumeFood_ShouldNotGoBelowZero()
    {
        var household = new HouseholdState();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();

        await Assert.That(household.FoodStockpile).IsEqualTo(0);
    }

    [Test]
    public async Task HasEnoughFood_ShouldReturnFalseWhenEmpty()
    {
        var household = new HouseholdState();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();

        await Assert.That(household.HasEnoughFood).IsFalse();
    }

    [Test]
    public async Task AddFood_ShouldIncreaseStockpile()
    {
        var household = new HouseholdState();

        household.AddFood(5);

        await Assert.That(household.FoodStockpile).IsEqualTo(8);
    }

    [Test]
    public async Task UpdateMotherHealth_ShouldClampToValidRange()
    {
        var household = new HouseholdState();

        household.UpdateMotherHealth(50);

        await Assert.That(household.MotherHealth).IsEqualTo(100);

        household.UpdateMotherHealth(-150);

        await Assert.That(household.MotherHealth).IsEqualTo(0);
    }

    [Test]
    public async Task UpdateMotherHealth_ShouldSetMotherDeadWhenHealthReachesZero()
    {
        var household = new HouseholdState();

        household.UpdateMotherHealth(-100);

        await Assert.That(household.MotherAlive).IsFalse();
    }

    [Test]
    public async Task MotherNeedsCare_ShouldReturnTrueWhenHealthBelow50()
    {
        var household = new HouseholdState();

        household.UpdateMotherHealth(-30);

        await Assert.That(household.MotherNeedsCare).IsTrue();
    }

    [Test]
    public async Task ApplyDailyDecay_WithoutFood_ShouldDamageMother()
    {
        var household = new HouseholdState();
        household.ConsumeFood();
        household.ConsumeFood();
        household.ConsumeFood();

        household.ApplyDailyDecay();

        await Assert.That(household.MotherHealth).IsEqualTo(65);
    }

    [Test]
    public async Task ApplyDailyDecay_WithFood_ShouldNotDamageMotherFromHunger()
    {
        var household = new HouseholdState();

        household.ApplyDailyDecay();

        await Assert.That(household.MotherHealth).IsEqualTo(70);
    }
}
