using FluentAssertions;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Investments;

internal sealed class InvestmentPurchaseServiceTests
{
    [Test]
    public void MakeInvestment_ShouldUseEligibilityAndSessionOwnedInvestmentState()
    {
        var session = new GameSession();
        session.Player.Stats.SetMoney(300);
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);

        var result = InvestmentPurchaseService.MakeInvestment(session, InvestmentType.FoulCart);

        result.Success.Should().BeTrue();
        session.ActiveInvestments.Should().ContainSingle();
        session.Mutations[^1].Action.Should().Be("MakeInvestment");
    }

    [Test]
    public void Restore_ShouldHydrateTheExistingInvestmentState()
    {
        var session = new GameSession();
        var state = session.ActiveInvestments;
        var definition = InvestmentRegistry.GetByType(InvestmentType.FoulCart);
        definition.Should().NotBeNull();

        InvestmentPurchaseService.Restore(
            session,
            [new InvestmentSnapshot(InvestmentType.FoulCart, 120, 8, 12, 1, false)],
            55);

        session.ActiveInvestments.Should().BeSameAs(state);
        session.ActiveInvestments.Should().ContainSingle();
        session.TotalInvestmentEarnings.Should().Be(55);
    }
}
