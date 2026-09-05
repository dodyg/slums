namespace Slums.Core.Robotics;

public sealed class RoboticsState
{
    private readonly List<OwnedRobot> _robots = [];

    public IReadOnlyList<OwnedRobot> Robots => _robots;

    public int Parts { get; private set; }

    public bool HasAnyRobots => _robots.Count > 0;

    public bool CanPurchaseRobot => _robots.Count < RobotRegistry.MaxOwnedRobots;

    public OwnedRobot? GetRobot(Guid robotId)
    {
        return _robots.FirstOrDefault(robot => robot.Id == robotId);
    }

    public bool CanBuyParts(int quantity)
    {
        return quantity > 0 && Parts + quantity <= RobotRegistry.MaxParts;
    }

    public void AddParts(int quantity)
    {
        if (!CanBuyParts(quantity))
        {
            throw new InvalidOperationException("The parts shelf cannot hold that many spare parts.");
        }

        Parts += quantity;
    }

    public bool TryConsumeParts(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (Parts < quantity)
        {
            return false;
        }

        Parts -= quantity;
        return true;
    }

    public bool PurchaseRobot(RobotType type, int currentDay)
    {
        if (!CanPurchaseRobot || _robots.Any(robot => robot.Type == type))
        {
            return false;
        }

        _robots.Add(OwnedRobot.Create(type, currentDay));
        return true;
    }

    public bool CanRepairRobot(Guid robotId)
    {
        var robot = GetRobot(robotId);
        return robot is not null && robot.Condition < 100 && Parts > 0;
    }

    public bool TryRepairRobot(Guid robotId)
    {
        var robot = GetRobot(robotId);
        if (robot is null || robot.Condition >= 100 || Parts <= 0)
        {
            return false;
        }

        Parts--;
        robot.Repair(RobotRegistry.GetByType(robot.Type).RepairCondition);
        return true;
    }

    public void Restore(IEnumerable<OwnedRobot> robots, int parts)
    {
        ArgumentNullException.ThrowIfNull(robots);
        if (parts < 0 || parts > RobotRegistry.MaxParts)
        {
            throw new ArgumentOutOfRangeException(nameof(parts));
        }

        _robots.Clear();
        _robots.AddRange(robots);
        Parts = parts;
    }
}
