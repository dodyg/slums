using FluentAssertions;
using Slums.Application.Activities;
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
}
