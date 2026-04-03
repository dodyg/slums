using FluentAssertions;
using Slums.Application.HouseholdAssets;
using Slums.Core.Characters;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.HouseholdAssets;

internal sealed class FishTankUpgradeMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposeAllFourUpgradePaths()
    {
        var query = new FishTankUpgradeMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.BuyFishTank(1, 1);

        var statuses = query.GetStatuses(FishTankUpgradeMenuContext.Create(gameState));

        statuses.Should().HaveCount(4);
        statuses.Should().Contain(static status => status.Name.Contains("Better Filter", StringComparison.Ordinal));
        statuses.Should().Contain(static status => status.Name.Contains("Heater", StringComparison.Ordinal));
        statuses.Should().Contain(static status => status.Name.Contains("Decorations", StringComparison.Ordinal));
        statuses.Should().Contain(static status => status.Name.Contains("Water Conditioner", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_AllUpgradesShouldBeAvailable_WhenNonePurchased()
    {
        var query = new FishTankUpgradeMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.BuyFishTank(1, 1);
        gameState.Player.Stats.SetMoney(100);

        var statuses = query.GetStatuses(FishTankUpgradeMenuContext.Create(gameState));

        statuses.Should().OnlyContain(static status => status.CanExecute);
    }

    [Test]
    public void GetStatuses_PermanentUpgrade_ShouldNotBeAvailable_WhenAlreadyOwned()
    {
        var query = new FishTankUpgradeMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.BuyFishTank(1, 1);
        gameState.Player.Stats.SetMoney(100);
        gameState.Player.HouseholdAssets.GetFishTank()!.PurchaseUpgrade(FishTankUpgradeType.BetterFilter, 1);

        var statuses = query.GetStatuses(FishTankUpgradeMenuContext.Create(gameState));

        var filterStatus = statuses.Should().Contain(static status => status.Name.Contains("Better Filter", StringComparison.Ordinal)).Subject;
        filterStatus.CanExecute.Should().BeFalse();
    }
}
