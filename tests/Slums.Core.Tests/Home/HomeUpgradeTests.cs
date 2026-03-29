using Slums.Core.Characters;
using Slums.Core.Home;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Home;

internal sealed class HomeUpgradeTests
{
    [Test]
    public async Task PurchaseSucceeds_WithEnoughMoneyAtHome()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(100);
        var result = state.TryPurchaseHomeUpgrade(HomeUpgrade.CleanBedding);
        await Assert.That(result).IsTrue();
        await Assert.That(state.HomeUpgrades.HasUpgrade(HomeUpgrade.CleanBedding)).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(75);
    }

    [Test]
    public async Task PurchaseFails_WhenNotAtHome()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(100);
        state.World.TravelTo(LocationId.Bakery);
        var result = state.TryPurchaseHomeUpgrade(HomeUpgrade.CleanBedding);
        await Assert.That(result).IsFalse();
        await Assert.That(state.HomeUpgrades.HasUpgrade(HomeUpgrade.CleanBedding)).IsFalse();
    }

    [Test]
    public async Task PurchaseFails_WithInsufficientMoney()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(10);
        var result = state.TryPurchaseHomeUpgrade(HomeUpgrade.CleanBedding);
        await Assert.That(result).IsFalse();
        await Assert.That(state.HomeUpgrades.HasUpgrade(HomeUpgrade.CleanBedding)).IsFalse();
    }

    [Test]
    public async Task PurchaseFails_WhenAlreadyOwned()
    {
        using var state = new GameSession();
        state.Player.Stats.SetMoney(100);
        state.TryPurchaseHomeUpgrade(HomeUpgrade.CleanBedding);
        var result = state.TryPurchaseHomeUpgrade(HomeUpgrade.CleanBedding);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GetAvailableHomeUpgrades_ShouldExcludePurchased()
    {
        using var state = new GameSession();
        state.HomeUpgrades.Purchase(HomeUpgrade.CleanBedding);
        var available = state.GetAvailableHomeUpgrades();
        await Assert.That(available.Contains(HomeUpgrade.CleanBedding)).IsFalse();
        await Assert.That(available.Count).IsEqualTo(3);
    }

    [Test]
    public async Task HomeUpgradeDefinitions_Costs_ShouldMatchRequirements()
    {
        await Assert.That(HomeUpgradeDefinitions.GetCost(HomeUpgrade.CleanBedding)).IsEqualTo(25);
        await Assert.That(HomeUpgradeDefinitions.GetCost(HomeUpgrade.Fan)).IsEqualTo(40);
        await Assert.That(HomeUpgradeDefinitions.GetCost(HomeUpgrade.WindowScreen)).IsEqualTo(20);
        await Assert.That(HomeUpgradeDefinitions.GetCost(HomeUpgrade.Curtain)).IsEqualTo(15);
    }

    [Test]
    public async Task RestAtHome_ShouldUseCalculatedRecovery()
    {
        using var state = new GameSession();
        state.Player.Nutrition.Eat(MealQuality.Basic);
        state.Player.Stats.SetEnergy(50);
        state.RestAtHome();
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(80);
    }

    [Test]
    public async Task RestAtHome_WithStress_ShouldHaveLowerRecovery()
    {
        using var state = new GameSession();
        state.Player.Nutrition.Eat(MealQuality.Basic);
        state.Player.Stats.SetEnergy(20);
        state.Player.Stats.SetStress(70);
        state.RestAtHome();
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(45);
    }

    [Test]
    public async Task EndDay_ShouldApplyOvernightRecovery()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(30);
        state.EndDay();
        var energy = state.Player.Stats.Energy;
        await Assert.That(energy).IsGreaterThan(15);
    }

    [Test]
    public async Task WindowScreen_ShouldReduceStressInEndDay()
    {
        using var state = new GameSession();
        state.HomeUpgrades.Purchase(HomeUpgrade.WindowScreen);
        state.Player.Nutrition.Eat(MealQuality.Basic);
        state.Player.Stats.SetStress(50);
        state.EndDay();
        var stressWithScreen = state.Player.Stats.Stress;

        using var state2 = new GameSession();
        state2.Player.Nutrition.Eat(MealQuality.Basic);
        state2.Player.Stats.SetStress(50);
        state2.EndDay();
        var stressWithoutScreen = state2.Player.Stats.Stress;

        await Assert.That(stressWithScreen).IsLessThan(stressWithoutScreen);
    }

    [Test]
    public async Task RestoreHomeUpgrades_ShouldPersistUpgrades()
    {
        using var state = new GameSession();
        state.HomeUpgrades.Purchase(HomeUpgrade.CleanBedding);
        state.HomeUpgrades.Purchase(HomeUpgrade.Fan);
        var upgrades = state.HomeUpgrades.PurchasedUpgrades.ToList();

        using var state2 = new GameSession();
        state2.RestoreHomeUpgrades(upgrades);
        await Assert.That(state2.HomeUpgrades.HasUpgrade(HomeUpgrade.CleanBedding)).IsTrue();
        await Assert.That(state2.HomeUpgrades.HasUpgrade(HomeUpgrade.Fan)).IsTrue();
        await Assert.That(state2.HomeUpgrades.HasUpgrade(HomeUpgrade.WindowScreen)).IsFalse();
    }
}
