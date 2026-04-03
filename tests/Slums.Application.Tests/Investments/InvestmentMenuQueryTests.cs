using FluentAssertions;
using Slums.Application.Investments;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Investments;

internal sealed class InvestmentMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldExposeBlockingReasons_ForCurrentLocationOpportunity()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        statuses.Should().Contain(s => s.Definition.Type == InvestmentType.FoulCart);
        statuses.Should().Contain(s => s.Definition.Type == InvestmentType.TeaCart);

        var foulCart = statuses.First(s => s.Definition.Type == InvestmentType.FoulCart);
        foulCart.CanInvest.Should().BeFalse();
        foulCart.BlockingReasons.Should().Contain(static reason => reason.Contains("Not enough money", StringComparison.Ordinal));
        foulCart.BlockingReasons.Should().Contain(static reason => reason.Contains("30 trust with Hajj Mahmoud", StringComparison.Ordinal));
        foulCart.RiskBreakdown.Should().Contain(static reason => reason.Contains("Failure 1%", StringComparison.Ordinal));
    }

    [Test]
    public void Execute_ShouldPurchaseInvestment_WhenEligible()
    {
        var command = new MakeInvestmentCommand();
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        gameState.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);

        var result = command.Execute(gameState, InvestmentType.FoulCart);

        result.Success.Should().BeTrue();
        gameState.ActiveInvestments.Should().ContainSingle();
        gameState.ActiveInvestments[0].Type.Should().Be(InvestmentType.FoulCart);
        result.Message.Should().Contain("Successfully invested");
    }

    [Test]
    public void GetStatuses_ShouldExposeOwnedStateSummary_WhenInvestmentAlreadyActive()
    {
        var query = new InvestmentMenuQuery();
        var definition = InvestmentRegistry.GetByType(InvestmentType.FoulCart)!;
        var activeInvestment = Investment.Restore(new InvestmentSnapshot(InvestmentType.FoulCart, 150, 8, 12, 2, true), definition.RiskProfile);
        var context = new InvestmentMenuContext(
            [definition],
            new Dictionary<InvestmentType, InvestmentEligibility>
            {
                [InvestmentType.FoulCart] = new(false, ["You already have this investment."])
            },
            [activeInvestment],
            new Dictionary<InvestmentType, Investment>
            {
                [InvestmentType.FoulCart] = activeInvestment
            },
            "Home");

        var statuses = query.GetStatuses(context);

        statuses.Should().ContainSingle();
        statuses[0].OwnedStateSummary.Should().Contain("Suspended this week");
        statuses[0].CurrentStateNotes.Should().Contain(static note => note.Contains("recover next week", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldExposeTeaCartAtHome()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        var teaCart = statuses.FirstOrDefault(s => s.Definition.Type == InvestmentType.TeaCart);
        teaCart.Should().NotBeNull();
        teaCart!.Definition.Cost.Should().Be(100);
        teaCart.UnlockSummary.Should().Contain("Need trust 10 with Mona");
        teaCart.WeeklyReturnSummary.Should().Be("5-8 LE / week");
    }

    [Test]
    public void GetStatuses_ShouldExposePhoneChargingAtDepot()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Depot);

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        var phone = statuses.FirstOrDefault(s => s.Definition.Type == InvestmentType.PhoneChargingStation);
        phone.Should().NotBeNull();
        phone!.UnlockSummary.Should().Contain("Need trust 15 with Safaa");
    }

    [Test]
    public void GetStatuses_ShouldExposeHerbalRemedyAtPharmacy_WithSkillRequirement()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Pharmacy);

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        var herbal = statuses.FirstOrDefault(s => s.Definition.Type == InvestmentType.HerbalRemedyTrade);
        herbal.Should().NotBeNull();
        herbal!.UnlockSummary.Should().Contain("medical knowledge");
        herbal.UnlockSummary.Should().Contain("Need trust 15 with Mariam");
    }

    [Test]
    public void GetStatuses_ShouldExposeSewingSideBusinessAtWorkshop_WithSkillRequirement()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Workshop);

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        var sewing = statuses.FirstOrDefault(s => s.Definition.Type == InvestmentType.SewingSideBusiness);
        sewing.Should().NotBeNull();
        sewing!.UnlockSummary.Should().Contain("physical capability");
        sewing.UnlockSummary.Should().Contain("Need trust 20 with Abu Samir");
    }

    [Test]
    public void Execute_ShouldPurchaseTeaCart_WhenEligible()
    {
        var command = new MakeInvestmentCommand();
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(100);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 10, 1);

        var result = command.Execute(gameState, InvestmentType.TeaCart);

        result.Success.Should().BeTrue();
        result.AmountInvested.Should().Be(100);
        gameState.Player.Stats.Money.Should().Be(0);
        gameState.ActiveInvestments.Should().ContainSingle();
        gameState.ActiveInvestments[0].Type.Should().Be(InvestmentType.TeaCart);
    }

    [Test]
    public void Execute_ShouldPurchaseCafeSupplyPartnership_WhenEligible()
    {
        var command = new MakeInvestmentCommand();
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(300);
        gameState.World.TravelTo(LocationId.Cafe);
        gameState.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 25, 1);

        var result = command.Execute(gameState, InvestmentType.CafeSupplyPartnership);

        result.Success.Should().BeTrue();
        result.AmountInvested.Should().Be(250);
        gameState.Player.Stats.Money.Should().Be(50);
    }

    [Test]
    public void GetStatuses_ShouldBlockHerbalRemedy_WhenMedicalSkillTooLow()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        gameState.World.TravelTo(LocationId.Pharmacy);
        gameState.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        var herbal = statuses.First(s => s.Definition.Type == InvestmentType.HerbalRemedyTrade);
        herbal.CanInvest.Should().BeFalse();
        herbal.BlockingReasons.Should().Contain(r => r.Contains("medical knowledge", StringComparison.Ordinal));
    }

    [Test]
    public void GetStatuses_ShouldAllowHerbalRemedy_WhenMedicalSkillMet()
    {
        var query = new InvestmentMenuQuery();
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        gameState.World.TravelTo(LocationId.Pharmacy);
        gameState.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);
        gameState.Player.Skills.SetLevel(SkillId.Medical, 2);

        var statuses = query.GetStatuses(InvestmentMenuContext.Create(gameState));

        var herbal = statuses.First(s => s.Definition.Type == InvestmentType.HerbalRemedyTrade);
        herbal.CanInvest.Should().BeTrue();
    }
}
