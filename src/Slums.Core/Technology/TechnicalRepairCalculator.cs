namespace Slums.Core.Technology;

public static class TechnicalRepairCalculator
{
    public static int GetConditionGain(TechnicalRepairActionType actionType, int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        return actionType switch
        {
            TechnicalRepairActionType.RepairHandset => skillLevel >= 8 ? 30 : 25,
            TechnicalRepairActionType.RestoreSolarStorage => skillLevel >= 8 ? 20 : 15,
            TechnicalRepairActionType.TakeRepairBenchContract => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(actionType))
        };
    }

    public static int GetContractIncome(int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        return skillLevel >= 10 ? 40 : 35;
    }
}
