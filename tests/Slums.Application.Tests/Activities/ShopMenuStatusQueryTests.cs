using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class ShopMenuStatusQueryTests
{
    [Test]
    public void GetStatuses_ShouldUseDynamicDistrictPrices()
    {
        var query = new ShopMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.CallCenter);
        gameState.Player.Stats.SetMoney(59);

        var statuses = query.GetStatuses(ShopMenuContext.Create(gameState));

        statuses.Should().HaveCount(2);
        statuses[0].OptionId.Should().Be(ShopOptionId.BuyFood);
        statuses[0].Name.Should().Be("Buy Food");
        statuses[0].Cost.Should().Be(20);
        statuses[0].CanAfford.Should().BeTrue();
        statuses[1].OptionId.Should().Be(ShopOptionId.BuyMedicine);
        statuses[1].Name.Should().Be("Buy Medicine");
        statuses[1].Cost.Should().Be(58);
        statuses[1].CanAfford.Should().BeTrue();
    }

    [Test]
    public void GetStatuses_ShouldIncludeClinicOption_OnlyWhereClinicServicesExist()
    {
        var query = new ShopMenuStatusQuery();
        using var marketState = new GameSession();
        marketState.World.TravelTo(LocationId.Market);
        using var clinicState = new GameSession();
        clinicState.World.TravelTo(LocationId.Clinic);

        var marketStatuses = query.GetStatuses(ShopMenuContext.Create(marketState));
        var clinicStatuses = query.GetStatuses(ShopMenuContext.Create(clinicState));

        marketStatuses.Should().HaveCount(2);
        clinicStatuses.Should().HaveCount(3);
        clinicStatuses[2].OptionId.Should().Be(ShopOptionId.TakeMotherToClinic);
        clinicStatuses[2].Name.Should().Be("Take Mother to Clinic");
        clinicStatuses[2].Cost.Should().Be(35);
        clinicStatuses[2].CanAfford.Should().BeTrue();
    }

    [Test]
    public void GetStatuses_ShouldDisableClinicOption_WhenClosedToday()
    {
        var query = new ShopMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.Clock.SetTime(day: 4, hour: 10, minute: 0);
        gameState.World.TravelTo(LocationId.Clinic);

        var statuses = query.GetStatuses(ShopMenuContext.Create(gameState));

        statuses.Should().HaveCount(3);
        statuses[2].CanAfford.Should().BeFalse();
        statuses[2].Note.Should().Contain("Closed on Tuesday");
    }

    [Test]
    public void GetStatuses_ShouldReflectMedicineDiscounts_AndAffordability()
    {
        var query = new ShopMenuStatusQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Pharmacy);
        gameState.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 12, 0);
        gameState.Player.Skills.SetLevel(SkillId.Medical, 3);
        gameState.Player.Stats.SetMoney(31);

        var statuses = query.GetStatuses(ShopMenuContext.Create(gameState));

        statuses.Should().HaveCount(3);
        statuses[1].Cost.Should().Be(32);
        statuses[1].CanAfford.Should().BeFalse();
    }
}
