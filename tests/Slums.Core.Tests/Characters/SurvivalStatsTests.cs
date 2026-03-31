using FluentAssertions;
using Slums.Core.Characters;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests.Characters;

internal sealed class SurvivalStatsTests
{
    [Test]
    public async Task Constructor_ShouldInitializeWithDefaultValues()
    {
        var stats = new SurvivalStats();

        await Assert.That(stats.Money).IsEqualTo(100);
        await Assert.That(stats.Hunger).IsEqualTo(80);
        await Assert.That(stats.Energy).IsEqualTo(80);
        await Assert.That(stats.Health).IsEqualTo(100);
        await Assert.That(stats.Stress).IsEqualTo(20);
    }

    [Test]
    public async Task ModifyHunger_ShouldClampToValidRange()
    {
        var stats = new SurvivalStats();

        stats.ModifyHunger(50);

        await Assert.That(stats.Hunger).IsEqualTo(100);

        stats.ModifyHunger(-200);

        await Assert.That(stats.Hunger).IsEqualTo(0);
    }

    [Test]
    public async Task ModifyEnergy_ShouldClampToValidRange()
    {
        var stats = new SurvivalStats();

        stats.ModifyEnergy(-90);

        await Assert.That(stats.Energy).IsEqualTo(0);

        stats.ModifyEnergy(150);

        await Assert.That(stats.Energy).IsEqualTo(100);
    }

    [Test]
    public async Task ModifyMoney_ShouldAllowNegativeForExpenses()
    {
        var stats = new SurvivalStats();

        stats.ModifyMoney(-50);

        await Assert.That(stats.Money).IsEqualTo(50);
    }

    [Test]
    public async Task ModifyMoney_ShouldNotGoBelowZero()
    {
        var stats = new SurvivalStats();

        stats.ModifyMoney(-150);

        await Assert.That(stats.Money).IsEqualTo(0);
    }

    [Test]
    public async Task Rest_ShouldIncreaseEnergyAndReduceStress()
    {
        var stats = new SurvivalStats();
        stats.ModifyEnergy(-50);
        stats.ModifyStress(30);

        stats.Rest();

        await Assert.That(stats.Energy).IsEqualTo(60);
        await Assert.That(stats.Stress).IsEqualTo(35);
        await Assert.That(stats.Hunger).IsEqualTo(70);
    }

    [Test]
    public async Task Eat_ShouldIncreaseHungerAndEnergy()
    {
        var stats = new SurvivalStats();
        stats.ModifyHunger(-50);

        stats.Eat(30);

        await Assert.That(stats.Hunger).IsEqualTo(60);
        await Assert.That(stats.Energy).IsEqualTo(87);
    }

    [Test]
    public async Task ApplyDailyDecay_ShouldReduceStats()
    {
        var stats = new SurvivalStats();

        stats.ApplyDailyDecay();

        await Assert.That(stats.Hunger).IsEqualTo(68);
        await Assert.That(stats.Energy).IsEqualTo(70);
        await Assert.That(stats.Stress).IsEqualTo(23);
    }

    [Test]
    public async Task IsStarving_ShouldReturnTrueWhenHungerIsLow()
    {
        var stats = new SurvivalStats();
        stats.ModifyHunger(-75);

        await Assert.That(stats.IsStarving).IsTrue();
    }

    [Test]
    public async Task IsExhausted_ShouldReturnTrueWhenEnergyIsLow()
    {
        var stats = new SurvivalStats();
        stats.ModifyEnergy(-75);

        await Assert.That(stats.IsExhausted).IsTrue();
    }

    [Test]
    public async Task IsOverstressed_ShouldReturnTrueWhenStressIsHigh()
    {
        var stats = new SurvivalStats();
        stats.ModifyStress(70);

        await Assert.That(stats.IsOverstressed).IsTrue();
    }
}
