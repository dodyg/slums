namespace Slums.Core.Skills;

public static class DigitalLiteracyCalculator
{
    public static int GetCreditRefillCost(int skillLevel, int baseCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(baseCost);

        var reduction = skillLevel >= SkillThresholds.FirstMeaningfulLevel ? 1 : 0;
        if (skillLevel >= SkillThresholds.HighLevel)
        {
            reduction++;
        }

        return Math.Max(1, baseCost - reduction);
    }

    public static int GetBiometricAppealSuccessChance(int skillLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skillLevel);
        return skillLevel switch
        {
            >= SkillThresholds.MaximumLevel => 85,
            >= SkillThresholds.MasteryLevel => 75,
            _ => 60
        };
    }
}
