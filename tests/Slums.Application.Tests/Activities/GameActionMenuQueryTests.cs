using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class GameActionMenuQueryTests
{
    [Test]
    public void GetActions_ShouldExposeHomeActionSet()
    {
        var query = new GameActionMenuQuery();
        using var gameState = new GameSession();

        var actions = query.GetActions(GameActionMenuContext.Create(gameState));

        actions.Select(static action => action.Id).Should().ContainInOrder(
            GameActionId.Rest,
            GameActionId.Talk,
            GameActionId.Invest,
            GameActionId.Shop,
            GameActionId.EatAtHome,
            GameActionId.CheckOnMother,
            GameActionId.GiveMotherMedicine,
            GameActionId.TakeMotherToClinic,
            GameActionId.Travel,
            GameActionId.SaveGame,
            GameActionId.EndDay);
    }

    [Test]
    public void GetActions_ShouldExposeLocationSpecificActions_AwayFromHome()
    {
        var query = new GameActionMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Depot);

        var actions = query.GetActions(GameActionMenuContext.Create(gameState));
        var actionIds = actions.Select(static action => action.Id).ToArray();

        actionIds.Should().Contain(GameActionId.Work);
        actionIds.Should().Contain(GameActionId.Crime);
        actionIds.Should().Contain(GameActionId.Entertainment);
        actionIds.Should().Contain(GameActionId.EatStreetFood);
        actionIds.Should().NotContain(GameActionId.EatAtHome);
        actionIds.Should().NotContain(GameActionId.CheckOnMother);
    }

    [Test]
    public void GetActions_ShouldExposeHouseholdAction_AtFishMarket()
    {
        var query = new GameActionMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.FishMarket);

        var actions = query.GetActions(GameActionMenuContext.Create(gameState));
        var householdAction = actions.Single(static action => action.Id == GameActionId.HouseholdAssets);

        householdAction.Label.Should().Be("Buy Fish Tank");
    }

    [Test]
    public void GetActions_ShouldExposePlantPurchaseAction_AtPlantShop()
    {
        var query = new GameActionMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.PlantShop);

        var actions = query.GetActions(GameActionMenuContext.Create(gameState));
        var householdAction = actions.Single(static action => action.Id == GameActionId.HouseholdAssets);

        householdAction.Label.Should().Be("Buy Plants");
    }

    [Test]
    public void GetActions_ShouldUsePetsAndPlantsLabel_AtHomeWhenManagementIsAvailable()
    {
        var query = new GameActionMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.BuyPlant(PlantType.Basil, 1, 1);

        var actions = query.GetActions(GameActionMenuContext.Create(gameState));
        var householdAction = actions.Single(static action => action.Id == GameActionId.HouseholdAssets);

        householdAction.Label.Should().Be("Pets & Plants");
    }
}
