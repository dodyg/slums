using Slums.Core.Characters;
using Slums.Core.Expenses;
using Slums.Core.State;
using Slums.Core.World;
using TUnit;

namespace Slums.Core.Tests.Expenses;

internal sealed class FoodShopServiceTests
{
    [Test]
    public async Task BuyFood_ShouldKeepBackgroundBonusAndMutationCategory()
    {
        var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        var stockBefore = session.Player.Household.FoodStockpile;

        var result = FoodShopService.BuyFood(session);

        await Assert.That(result).IsTrue();
        await Assert.That(session.Player.Household.FoodStockpile).IsEqualTo(stockBefore + 4);
        await Assert.That(session.Mutations[^1].Category).IsEqualTo("Food");
        await Assert.That(session.Mutations[^1].Action).IsEqualTo("BuyFood");
    }

    [Test]
    public async Task GetMedicineCost_ShouldUseTheSameLocationPricingServiceAsTheSession()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Pharmacy);

        var serviceCost = FoodShopService.GetMedicineCost(session);

        await Assert.That(serviceCost).IsEqualTo(session.GetMedicineCost());
    }
}
