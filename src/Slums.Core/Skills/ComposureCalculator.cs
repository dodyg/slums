namespace Slums.Core.Skills;

/// <summary>Calculates bounded pressure mitigations granted by Composure.</summary>
public static class ComposureCalculator
{
    public static int GetWorkMistakeStressThreshold(int composureSkillLevel, int baseThreshold)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(composureSkillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(baseThreshold);

        return baseThreshold + (composureSkillLevel >= SkillThresholds.FirstMeaningfulLevel ? 5 : 0);
    }

    public static int GetDebtStressCost(int composureSkillLevel, int baseStress)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(composureSkillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(baseStress);

        var reduction = composureSkillLevel >= SkillThresholds.AdvancedLevel
            ? 1
            : 0;
        if (composureSkillLevel >= SkillThresholds.HighLevel)
        {
            reduction++;
        }

        return Math.Max(0, baseStress - reduction);
    }

    public static int GetCrisisStressRelief(int composureSkillLevel, int baseRelief)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(composureSkillLevel);
        ArgumentOutOfRangeException.ThrowIfNegative(baseRelief);

        var relief = composureSkillLevel >= SkillThresholds.HighLevel ? 2 : 0;
        if (composureSkillLevel >= SkillThresholds.MasteryLevel)
        {
            relief++;
        }

        return Math.Min(baseRelief, relief);
    }
}
