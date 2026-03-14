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

    [Test]
    public void RegistryRiskProfiles_ShouldMatchBalanceTargets()
    {
        var foulCart = InvestmentRegistry.GetByType(InvestmentType.FoulCart)!.RiskProfile;
        var kiosk = InvestmentRegistry.GetByType(InvestmentType.Kiosk)!.RiskProfile;
        var courier = InvestmentRegistry.GetByType(InvestmentType.HashishCourier)!.RiskProfile;

        foulCart.WeeklyFailureChance.Should().Be(0.01);
        foulCart.BetrayalChance.Should().Be(0.02);
        kiosk.ExtortionChance.Should().Be(0.04);
        kiosk.PoliceHeatChance.Should().Be(0.03);
        courier.WeeklyFailureChance.Should().Be(0.08);
        courier.PoliceHeatChance.Should().Be(0.12);
    }
}
