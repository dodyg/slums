using FluentAssertions;
using Slums.Core.Characters;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class HouseholdAssetsStateTests
{
    [Test]
    public void AdoptCat_ShouldRequireEncounter_AndRespectCap()
    {
        var assets = new HouseholdAssetsState();

        assets.AdoptCat(1, 1).Should().BeFalse();

        for (var i = 0; i < HouseholdAssetsState.MaxCats; i++)
        {
            assets.TryTriggerStreetCatEncounter(i + 1).Should().BeTrue();
            assets.AdoptCat(i + 1, 1).Should().BeTrue();
        }

        assets.Pets.Should().HaveCount(HouseholdAssetsState.MaxCats);
        assets.TryTriggerStreetCatEncounter(10).Should().BeFalse();
    }

    [Test]
    public void ResolveWeeklyNeglect_ShouldReportMissedPetAndPlantCare()
    {
        var assets = new HouseholdAssetsState();
        assets.TryTriggerStreetCatEncounter(1);
        assets.AdoptCat(1, 1).Should().BeTrue();
        assets.BuyPlant(PlantType.Basil, 1, 1).Should().BeTrue();

        var resolution = assets.ResolveWeeklyNeglect(3);

        resolution.UnpaidPetCount.Should().Be(1);
        resolution.UnpaidPlantCount.Should().Be(1);
        resolution.StressPenalty.Should().Be(3);
    }

    [Test]
    public void ResolveSellablePlantIncome_ShouldIncludeUpgradeBoosts()
    {
        var assets = new HouseholdAssetsState();
        assets.BuyPlant(PlantType.Chamomile, 1, 1).Should().BeTrue();
        var plant = assets.Plants.Should().ContainSingle().Subject;
        plant.PurchaseUpgrade(PlantUpgradeType.BiggerPot, 1);
        plant.PurchaseUpgrade(PlantUpgradeType.Fertilizer, 1);

        var income = assets.ResolveSellablePlantIncome(6, 1);

        income.Should().Be(20);
        assets.TotalHerbEarnings.Should().Be(20);
    }

    [Test]
    public void GetMotherDailyHealthBonus_ShouldReflectCareAndUpgrades()
    {
        var assets = new HouseholdAssetsState();
        assets.BuyFishTank(1, 1).Should().BeTrue();
        assets.BuyPlant(PlantType.AloeVera, 1, 1).Should().BeTrue();
        var plant = assets.Plants.Should().ContainSingle().Subject;
        plant.PurchaseUpgrade(PlantUpgradeType.WindowPlacement, 1);

        assets.GetMotherDailyHealthBonus(1).Should().Be(6);
    }
}
