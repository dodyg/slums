using Slums.Core.Characters;
using Slums.Core.Home;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Home;

internal sealed class SleepQualityTests
{
    private static GameSession CreateSessionWithEaten()
    {
        var state = new GameSession();
        state.Player.Nutrition.Eat(MealQuality.Basic);
        return state;
    }

    [Test]
    public async Task BaseRecovery_ShouldBe30WithNoModifiers()
    {
        var state = CreateSessionWithEaten();
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(30);
    }

    [Test]
    public async Task StressAbove60_ShouldReduceBy5()
    {
        var state = CreateSessionWithEaten();
        state.Player.Stats.SetStress(65);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(25);
    }

    [Test]
    public async Task StressAbove80_ShouldReduceBy10()
    {
        var state = CreateSessionWithEaten();
        state.Player.Stats.SetStress(85);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(20);
    }

    [Test]
    public async Task StressAbove80_ShouldReplaceAbove60Modifier()
    {
        var state = CreateSessionWithEaten();
        state.Player.Stats.SetStress(90);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(20);
    }

    [Test]
    public async Task DidNotEatToday_ShouldReduceBy5()
    {
        var state = new GameSession();
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(25);
    }

    [Test]
    public async Task DaysUndereatingAbove2_ShouldReduceBy5()
    {
        var state = CreateSessionWithEaten();
        state.Player.Nutrition.SetDaysUndereating(3);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(25);
    }

    [Test]
    public async Task MotherHealthInCrisis_ShouldReduceBy5()
    {
        var state = CreateSessionWithEaten();
        state.Player.Household.SetMotherHealth(20);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(25);
    }

    [Test]
    public async Task RentUnpaidMoreThan3Days_ShouldReduceBy3()
    {
        var state = CreateSessionWithEaten();
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            4, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(27);
    }

    [Test]
    public async Task CleanBedding_ShouldAdd2()
    {
        var state = CreateSessionWithEaten();
        state.HomeUpgrades.Purchase(HomeUpgrade.CleanBedding);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(32);
    }

    [Test]
    public async Task FanNotSummer_ShouldAdd1()
    {
        var state = CreateSessionWithEaten();
        state.HomeUpgrades.Purchase(HomeUpgrade.Fan);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(31);
    }

    [Test]
    public async Task WindowScreen_ShouldAdd1()
    {
        var state = CreateSessionWithEaten();
        state.HomeUpgrades.Purchase(HomeUpgrade.WindowScreen);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(31);
    }

    [Test]
    public async Task Curtain_ShouldAdd1()
    {
        var state = CreateSessionWithEaten();
        state.HomeUpgrades.Purchase(HomeUpgrade.Curtain);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(31);
    }

    [Test]
    public async Task MultipleUpgrades_ShouldStackAdditively()
    {
        var state = CreateSessionWithEaten();
        state.HomeUpgrades.Purchase(HomeUpgrade.CleanBedding);
        state.HomeUpgrades.Purchase(HomeUpgrade.Fan);
        state.HomeUpgrades.Purchase(HomeUpgrade.WindowScreen);
        state.HomeUpgrades.Purchase(HomeUpgrade.Curtain);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(35);
    }

    [Test]
    public async Task MinimumRecovery_ShouldBe10()
    {
        var state = new GameSession();
        state.Player.Stats.SetStress(90);
        state.Player.Nutrition.SetDaysUndereating(3);
        state.Player.Household.SetMotherHealth(20);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            4, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(10);
    }

    [Test]
    public async Task OvernightRecovery_BaseShouldBe15()
    {
        var state = CreateSessionWithEaten();
        var recovery = SleepQualityCalculator.CalculateOvernightRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(15);
    }

    [Test]
    public async Task OvernightRecovery_MinimumShouldBe5()
    {
        var state = new GameSession();
        state.Player.Stats.SetStress(90);
        state.Player.Nutrition.SetDaysUndereating(3);
        state.Player.Household.SetMotherHealth(20);
        var recovery = SleepQualityCalculator.CalculateOvernightRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            4, state.HomeUpgrades);
        await Assert.That(recovery).IsEqualTo(5);
    }

    [Test]
    public async Task BuildRecoveryBreakdown_ShouldIncludeBaseAndRecovery()
    {
        var state = CreateSessionWithEaten();
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        var breakdown = SleepQualityCalculator.BuildRecoveryBreakdown(
            recovery, state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(breakdown).Contains("Base: 30", StringComparison.Ordinal);
        await Assert.That(breakdown).Contains("Recovery: 30", StringComparison.Ordinal);
    }

    [Test]
    public async Task BuildRecoveryBreakdown_WithStress_ShouldShowStressModifier()
    {
        var state = CreateSessionWithEaten();
        state.Player.Stats.SetStress(65);
        var recovery = SleepQualityCalculator.CalculateRecovery(
            state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        var breakdown = SleepQualityCalculator.BuildRecoveryBreakdown(
            recovery, state.Player.Stats, state.Player.Nutrition, state.Player.Household,
            state.UnpaidRentDays, state.HomeUpgrades);
        await Assert.That(breakdown).Contains("Stress: -5", StringComparison.Ordinal);
    }
}
