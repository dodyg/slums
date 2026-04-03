using FluentAssertions;
using Slums.Application.HouseholdAssets;
using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.HouseholdAssets;

internal sealed class HouseholdAssetsMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposePlantShopCatalog()
    {
        var query = new HouseholdAssetsMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.PlantShop);

        var statuses = query.GetStatuses(HouseholdAssetsMenuContext.Create(gameState));

        statuses.Should().HaveCount(PlantRegistry.AllDefinitions.Count);
        statuses.Should().Contain(static status => status.Title == "Chamomile");
        statuses.Should().Contain(static status => status.Title == "Aloe Vera");
    }

    [Test]
    public void GetStatuses_ShouldExposeHomeManagement_WhenAssetsExist()
    {
        var query = new HouseholdAssetsMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.TryTriggerStreetCatEncounter(1);
        gameState.Player.HouseholdAssets.BuyPlant(PlantType.AloeVera, 1, 1);

        var statuses = query.GetStatuses(HouseholdAssetsMenuContext.Create(gameState));

        statuses.Should().Contain(static status => status.ActionType == HouseholdAssetActionType.AdoptCat);
        statuses.Should().Contain(static status => status.ActionType == HouseholdAssetActionType.PayPlantCare);
        statuses.Should().Contain(static status => status.ActionType == HouseholdAssetActionType.ManagePlant && status.Title.Contains("Aloe Vera", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldShowManageFishTank_WhenFishTankOwned()
    {
        var query = new HouseholdAssetsMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.BuyFishTank(1, 1);

        var statuses = query.GetStatuses(HouseholdAssetsMenuContext.Create(gameState));

        statuses.Should().Contain(static status => status.ActionType == HouseholdAssetActionType.ManageFishTank && status.Title == "Manage Fish Tank");
    }

    [Test]
    public void GetStatuses_ShouldNotShowManageFishTank_WhenNoFishTankOwned()
    {
        var query = new HouseholdAssetsMenuQuery();
        using var gameState = new GameSession();

        var statuses = query.GetStatuses(HouseholdAssetsMenuContext.Create(gameState));

        statuses.Should().NotContain(static status => status.ActionType == HouseholdAssetActionType.ManageFishTank);
    }
}
