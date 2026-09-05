using FluentAssertions;
using Slums.Core.Robotics;
using TUnit.Core;

namespace Slums.Core.Tests.Robotics;

internal sealed class RobotRepairCostCalculatorTests
{
    [Test]
    public void GetRepairCost_ShouldChargeFullFee_BelowAssistedLevel()
    {
        RobotRepairCostCalculator.GetRepairCost(0, 18).Should().Be(18);
        RobotRepairCostCalculator.GetRepairCost(1, 18).Should().Be(18);
    }

    [Test]
    public void GetRepairCost_ShouldHalveFee_AtAssistedLevel()
    {
        RobotRepairCostCalculator.GetRepairCost(2, 18).Should().Be(9);
        RobotRepairCostCalculator.GetRepairCost(4, 30).Should().Be(15);
    }

    [Test]
    public void GetRepairCost_ShouldRoundUpOddFees_AtAssistedLevel()
    {
        RobotRepairCostCalculator.GetRepairCost(2, 25).Should().Be(13);
    }

    [Test]
    public void GetRepairCost_ShouldWaiveFee_AtSoloLevel()
    {
        RobotRepairCostCalculator.GetRepairCost(5, 18).Should().Be(0);
        RobotRepairCostCalculator.GetRepairCost(10, 30).Should().Be(0);
    }

    [Test]
    public void GetRepairCost_ShouldRejectNegativeInputs()
    {
        FluentActions.Invoking(() => RobotRepairCostCalculator.GetRepairCost(-1, 18))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => RobotRepairCostCalculator.GetRepairCost(0, -1))
            .Should().Throw<ArgumentOutOfRangeException>();
    }
}
