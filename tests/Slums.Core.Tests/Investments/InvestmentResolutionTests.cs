using FluentAssertions;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Investments;

internal sealed class InvestmentResolutionTests
{
    [Test]
    public void ResolveWeeklyInvestments_ShouldAddIncome_WhenRisksDoNotTrigger()
    {
        var gameState = TestSessions.Create();
        gameState.Player.Stats.SetMoney(300);
        gameState.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 30, 1);

        var investmentResult = gameState.MakeInvestment(InvestmentType.FoulCart);

        investmentResult.Success.Should().BeTrue();

        var summary = gameState.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [10]));

        summary.TotalIncome.Should().Be(16);
        gameState.TotalInvestmentEarnings.Should().Be(16);
        gameState.Player.Stats.Money.Should().Be(166);
        gameState.ActiveInvestments.Should().ContainSingle();
        gameState.ActiveInvestments[0].WeeksActive.Should().Be(1);
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldSuspendOperation_WhenExtortionCannotBePaid()
    {
        var gameState = TestSessions.Create();
        gameState.Player.Stats.SetMoney(250);
        gameState.World.TravelTo(LocationId.Market);
        gameState.Relationships.SetNpcRelationship(NpcId.FixerUmmKarim, 40, 1);

        var investmentResult = gameState.MakeInvestment(InvestmentType.Kiosk);

        investmentResult.Success.Should().BeTrue();
        gameState.Player.Stats.Money.Should().Be(0);

        var firstWeek = gameState.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.0],
                intValues: [12]));

        firstWeek.TotalIncome.Should().Be(0);
        gameState.ActiveInvestments.Should().ContainSingle();
        gameState.ActiveInvestments[0].IsSuspended.Should().BeTrue();
        gameState.PolicePressure.Should().Be(2);

        var secondWeek = gameState.ResolveWeeklyInvestments(new SequenceRandom());

        secondWeek.TotalIncome.Should().Be(0);
        gameState.ActiveInvestments[0].IsSuspended.Should().BeFalse();
        gameState.ActiveInvestments[0].WeeksActive.Should().Be(2);
    }
}
