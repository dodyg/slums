namespace Slums.Core.Calendar;

public sealed record SeasonModifiers(
    Season Season,
    string SeasonName,
    int FoodCostModifier,
    int EnergyDrainModifier,
    int StressModifier,
    int RestRecoveryBonus,
    int InvestmentReturnModifierPercent,
    double IllnessEventFrequencyMultiplier,
    int OutdoorWorkStressModifier)
{
    public static SeasonModifiers None { get; } = new(Season.Autumn, "Autumn", 0, 0, 0, 0, 0, 1.0, 0);
}
