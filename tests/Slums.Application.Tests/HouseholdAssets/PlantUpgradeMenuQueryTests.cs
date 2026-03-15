using FluentAssertions;
using Slums.Application.HouseholdAssets;
using Slums.Core.Characters;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.HouseholdAssets;

internal sealed class PlantUpgradeMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposeAllFourUpgradePaths()
    {
        var query = new PlantUpgradeMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.HouseholdAssets.BuyPlant(PlantType.Hibiscus, 1, 1);
        var plant = gameState.Player.HouseholdAssets.Plants.Should().ContainSingle().Subject;

        var statuses = query.GetStatuses(PlantUpgradeMenuContext.Create(gameState, plant.Id));

        statuses.Should().HaveCount(4);
        statuses.Should().Contain(static status => status.Name.Contains("Bigger Pot", StringComparison.Ordinal));
        statuses.Should().Contain(static status => status.Name.Contains("Irrigation", StringComparison.Ordinal));
    }
}
