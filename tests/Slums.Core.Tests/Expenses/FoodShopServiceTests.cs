using Slums.Core.Characters;
using Slums.Core.Expenses;
using Slums.Core.State;
using Slums.Core.World;
using Slums.Core.Skills;
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

    [Test]
    public async Task BuyFood_WithProvisioningTwo_ShouldAddFourUnits()
    {
        var session = new GameSession();
        session.Player.Skills.SetLevel(SkillId.Provisioning, 2);
        var stockBefore = session.Player.Household.FoodStockpile;

        var result = FoodShopService.BuyFood(session);

        await Assert.That(result).IsTrue();
        await Assert.That(session.Player.Household.FoodStockpile).IsEqualTo(stockBefore + 4);
    }

    [Test]
    public async Task ProvisioningMealPlan_UsesHerbAtAdvancedLevelAndImprovesMotherCareAtMastery()
    {
        var session = new GameSession();
        session.Player.Skills.SetLevel(SkillId.Provisioning, 8);
        session.Player.HouseholdAssets.BuyPlant(PlantType.Mint, session.Clock.Day, session.CurrentWeek);

        var plan = session.GetProvisioningMealPlan();

        await Assert.That(plan.Quality).IsEqualTo(MealQuality.HotMeal);
        await Assert.That(plan.UsesHouseholdHerb).IsTrue();
        await Assert.That(ProvisioningCalculator.GetMotherCareMealBonus(8, plan.Quality)).IsEqualTo(1);
    }
}
