using FluentAssertions;
using Slums.Core.Investments;
using TUnit.Core;

namespace Slums.Core.Tests.Investments;

internal sealed class InvestmentResolutionCalculatorTests
{
    [Test]
    public void Resolve_ShouldReturnIncome_WhenNoRiskTriggers()
    {
        var definition = InvestmentRegistry.GetByType(InvestmentType.FoulCart);
        var investment = new Investment(InvestmentType.FoulCart, 150, 8, 12, InvestmentRiskProfile.Low);

        var calculation = InvestmentResolutionCalculator.Resolve(
            investment,
            definition,
            currentMoney: 100,
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [10]));

        calculation.ShouldSuspend.Should().BeFalse();
        calculation.Resolution.Income.Should().Be(10);
        calculation.Resolution.WasLost.Should().BeFalse();
    }

    [Test]
    public void Resolve_ShouldMarkSuspension_WhenExtortionCannotBePaid()
    {
        var definition = InvestmentRegistry.GetByType(InvestmentType.Kiosk);
        var investment = new Investment(InvestmentType.Kiosk, 250, 10, 15, definition!.RiskProfile);

        var calculation = InvestmentResolutionCalculator.Resolve(
            investment,
            definition,
            currentMoney: 0,
            new SequenceRandom(
                doubleValues: [0.99, 0.0],
                intValues: [12]));

        calculation.ShouldSuspend.Should().BeTrue();
        calculation.Resolution.PolicePressureIncrease.Should().Be(2);
        calculation.Resolution.Message.Contains("Operation suspended", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }
}
