using Slums.Core.Robotics;

namespace Slums.Infrastructure.Persistence;

public sealed record OwnedRobotSnapshot(
    Guid Id,
    RobotType Type,
    int AcquiredOnDay,
    int Condition)
{
    public static OwnedRobotSnapshot Capture(OwnedRobot robot)
    {
        ArgumentNullException.ThrowIfNull(robot);
        return new OwnedRobotSnapshot(robot.Id, robot.Type, robot.AcquiredOnDay, robot.Condition);
    }

    public OwnedRobot Restore()
    {
        return OwnedRobot.Restore(Id, Type, AcquiredOnDay, Condition);
    }
}
