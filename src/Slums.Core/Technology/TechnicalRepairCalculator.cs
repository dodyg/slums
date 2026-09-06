namespace Slums.Core.Technology;

using Slums.Core.Skills;

public static class TechnicalRepairCalculator
{
    public static int GetConditionGain(TechnicalRepairActionType actionType, int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        return actionType switch
        {
            TechnicalRepairActionType.RepairHandset => skillLevel >= 8 ? 30 : 25,
            TechnicalRepairActionType.RestoreSolarStorage => skillLevel >= 8 ? 20 : 15,
            TechnicalRepairActionType.RestoreWaterPump => skillLevel >= 8 ? 25 : 18,
            TechnicalRepairActionType.TakeRepairBenchContract => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(actionType))
        };
    }

    public static int GetContractIncome(int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        return skillLevel >= SkillThresholds.MaximumLevel ? 40 : 35;
    }

    public static int GetInfrastructureRecoveryDays(int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        return skillLevel switch
        {
            >= SkillThresholds.MasteryLevel => 3,
            >= SkillThresholds.HighLevel => 2,
            _ => 1
        };
    }
}
