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
}
