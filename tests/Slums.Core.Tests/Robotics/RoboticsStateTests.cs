using FluentAssertions;
using Slums.Core.Robotics;
using TUnit.Core;

namespace Slums.Core.Tests.Robotics;

internal sealed class RoboticsStateTests
{
    [Test]
    public void PurchaseRobot_ShouldCreateOperationalOwnedMachine()
    {
        var state = new RoboticsState();

        state.PurchaseRobot(RobotType.SalvageCrawler, 3).Should().BeTrue();

        state.Robots.Should().ContainSingle();
        state.Robots[0].Condition.Should().Be(100);
        state.Robots[0].IsOperational.Should().BeTrue();
    }

    [Test]
    public void RepairRobot_ShouldConsumePartAndRestoreCondition()
    {
        var state = new RoboticsState();
        state.PurchaseRobot(RobotType.RepairDrone, 1);
        var robot = state.Robots[0];
        robot.Damage(70);
        state.AddParts(1);

        state.TryRepairRobot(robot.Id).Should().BeTrue();

        state.Parts.Should().Be(0);
        robot.Condition.Should().Be(70);
    }

    [Test]
    public void PurchaseRobot_ShouldRejectDuplicateModel()
    {
        var state = new RoboticsState();

        state.PurchaseRobot(RobotType.CargoMule, 1).Should().BeTrue();
        state.PurchaseRobot(RobotType.CargoMule, 2).Should().BeFalse();
    }

    [Test]
    public void CapabilityRules_ShouldGrantOnlyOperationalRobotBenefits()
    {
        var state = new RoboticsState();
        state.PurchaseRobot(RobotType.CargoMule, 1);
        state.PurchaseRobot(RobotType.RepairDrone, 1);
        state.PurchaseRobot(RobotType.SalvageCrawler, 1);

        RobotCapabilityRules.GetTransitEnergyReduction(state).Should().Be(RobotCapabilityRules.TransitEnergyReduction);
        RobotCapabilityRules.GetClinicCostReduction(state).Should().Be(RobotCapabilityRules.ClinicCostReduction);
        RobotCapabilityRules.GetSalvageBonusParts(state).Should().Be(RobotCapabilityRules.SalvageBonusParts);

        state.Robots[0].Damage(100);
        RobotCapabilityRules.GetTransitEnergyReduction(state).Should().Be(0);
    }

    [Test]
    public void CapabilityRules_ShouldKeepTheDocumentedWearCostsPositive()
    {
        RobotCapabilityRules.SalvageWear.Should().BeGreaterThan(0);
        RobotCapabilityRules.ClinicWear.Should().BeGreaterThan(0);
        RobotCapabilityRules.TransitWear.Should().BeGreaterThan(0);
    }
}
