namespace Slums.Core.Robotics;

public static class RobotCapabilityRules
{
    public const int SalvageBonusParts = 1;
    public const int SalvageWear = 10;
    public const int ClinicCostReduction = 6;
    public const int ClinicWear = 5;
    public const int TransitEnergyReduction = 2;
    public const int TransitWear = 3;

    public static bool HasOperationalRobot(RoboticsState robotics, RobotType type)
    {
        ArgumentNullException.ThrowIfNull(robotics);
        return robotics.Robots.Any(robot => robot.Type == type && robot.IsOperational);
    }

    public static int GetClinicCostReduction(RoboticsState robotics)
    {
        return HasOperationalRobot(robotics, RobotType.RepairDrone) ? ClinicCostReduction : 0;
    }

    public static int GetTransitEnergyReduction(RoboticsState robotics)
    {
        return HasOperationalRobot(robotics, RobotType.CargoMule) ? TransitEnergyReduction : 0;
    }

    public static int GetSalvageBonusParts(RoboticsState robotics)
    {
        return HasOperationalRobot(robotics, RobotType.SalvageCrawler) ? SalvageBonusParts : 0;
    }
}
