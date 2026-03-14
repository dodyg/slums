using FluentAssertions;
using Slums.Application.Investments;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;
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

        statuses.Should().ContainSingle();
        statuses[0].Definition.Type.Should().Be(InvestmentType.FoulCart);
        statuses[0].CanInvest.Should().BeFalse();
        statuses[0].BlockingReasons.Should().Contain(static reason => reason.Contains("Not enough money", StringComparison.Ordinal));
        statuses[0].BlockingReasons.Should().Contain(static reason => reason.Contains("30 trust with Hajj Mahmoud", StringComparison.Ordinal));
        statuses[0].RiskBreakdown.Should().Contain(static reason => reason.Contains("Failure 1%", StringComparison.Ordinal));
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
}
