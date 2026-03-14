using FluentAssertions;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Investments;

internal sealed class InvestmentEligibilityTests
{
    [Test]
    public void CheckInvestmentEligibility_ShouldRequireTrustForFoulCart()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        var definition = InvestmentRegistry.GetByType(InvestmentType.FoulCart);
        definition.Should().NotBeNull();
        var foulCart = definition!;

        var blocked = gameState.CheckInvestmentEligibility(foulCart);

        blocked.IsEligible.Should().BeFalse();
        blocked.FailureReasons.Should().Contain(static reason => reason.Contains("30 trust with Hajj Mahmoud", StringComparison.Ordinal));

        gameState.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);

        var eligible = gameState.CheckInvestmentEligibility(foulCart);

        eligible.IsEligible.Should().BeTrue();
    }

    [Test]
    public void GetCurrentInvestmentOpportunities_ShouldReflectCurrentLocationContacts()
    {
        using var gameState = new GameSession();

        var homeOpportunities = gameState.GetCurrentInvestmentOpportunities().Select(static definition => definition.Type).ToArray();
        homeOpportunities.Should().Contain(InvestmentType.FoulCart);
        homeOpportunities.Should().NotContain(InvestmentType.Kiosk);

        gameState.World.TravelTo(LocationId.Market);

        var marketOpportunities = gameState.GetCurrentInvestmentOpportunities().Select(static definition => definition.Type).ToArray();
        marketOpportunities.Should().Contain(InvestmentType.Kiosk);
        marketOpportunities.Should().Contain(InvestmentType.HashishCourier);
        marketOpportunities.Should().NotContain(InvestmentType.FoulCart);
    }
}
