namespace Slums.Core.Robotics;

/// <summary>Computes robot repair bench fees based on the player's Robot Repair skill.</summary>
public static class RobotRepairCostCalculator
{
    /// <summary>Skill level at which the player assists on the bench and halves the repair fee.</summary>
    public const int AssistedRepairLevel = 2;

    /// <summary>Skill level at which the player repairs alone and pays no bench fee.</summary>
    public const int SoloRepairLevel = 5;

    public static int GetRepairCost(int robotRepairSkillLevel, int baseRepairCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(robotRepairSkillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(baseRepairCost);

        if (robotRepairSkillLevel >= SoloRepairLevel)
        {
            return 0;
        }

        if (robotRepairSkillLevel >= AssistedRepairLevel)
        {
            return (baseRepairCost + 1) / 2;
        }

        return baseRepairCost;
    }
}
