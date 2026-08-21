namespace Slums.Core.Robotics;

public sealed class OwnedRobot
{
    public Guid Id { get; init; }

    public RobotType Type { get; init; }

    public int AcquiredOnDay { get; init; }

    public int Condition { get; private set; }

    public bool IsOperational => Condition > 0;

    public void Repair(int amount)
    {
        if (amount <= 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        }

        Condition = Math.Min(100, Condition + amount);
    }

    public void Damage(int amount)
    {
        if (amount <= 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        }

        Condition = Math.Max(0, Condition - amount);
    }

    public static OwnedRobot Create(RobotType type, int currentDay)
    {
        return new OwnedRobot
        {
            Id = Guid.NewGuid(),
            Type = type,
            AcquiredOnDay = currentDay,
            Condition = 100
        };
    }

    public static OwnedRobot Restore(Guid id, RobotType type, int acquiredOnDay, int condition)
    {
        return new OwnedRobot
        {
            Id = id,
            Type = type,
            AcquiredOnDay = acquiredOnDay,
            Condition = Math.Clamp(condition, 0, 100)
        };
    }
}
