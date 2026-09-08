namespace Slums.Core.Robotics;

public sealed class RoboticsState
{
    /// <summary>Maximum number of robots the flat and the alley can support at once.</summary>
    public const int MaxOwnedRobots = 3;

    /// <summary>Cost of one spare robot part.</summary>
    public const int PartsPurchaseCost = 8;

    /// <summary>Maximum number of spare robot parts the flat can hold.</summary>
    public const int MaxParts = 20;

    private readonly List<OwnedRobot> _robots = [];
    private readonly IReadOnlyList<RobotDefinition> _definitions;

    public RoboticsState(IEnumerable<RobotDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = definitions.Where(static definition => definition is not null).ToArray();
    }

    public IReadOnlyList<OwnedRobot> Robots => _robots;

    public int Parts { get; private set; }

    public bool HasAnyRobots => _robots.Count > 0;

    public bool CanPurchaseRobot => _robots.Count < MaxOwnedRobots;

    public OwnedRobot? GetRobot(Guid robotId)
    {
        return _robots.FirstOrDefault(robot => robot.Id == robotId);
    }

    public RobotDefinition GetDefinition(RobotType type)
    {
        return _definitions.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException($"No robot definition configured for {type}.");
    }

    /// <summary>Gets the first owned robot of <paramref name="type"/> that is still operational, or <c>null</c>.</summary>
    public OwnedRobot? GetOperational(RobotType type)
    {
        return _robots.FirstOrDefault(robot => robot.Type == type && robot.Condition > 0);
    }

    public bool CanBuyParts(int quantity)
    {
        return quantity > 0 && Parts + quantity <= MaxParts;
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
        robot.Repair(GetDefinition(robot.Type).RepairCondition);
        return true;
    }

    public void Restore(IEnumerable<OwnedRobot> robots, int parts)
    {
        ArgumentNullException.ThrowIfNull(robots);
        if (parts < 0 || parts > MaxParts)
        {
            throw new ArgumentOutOfRangeException(nameof(parts));
        }

        _robots.Clear();
        _robots.AddRange(robots);
        Parts = parts;
    }
}
