namespace Slums.Core.Investments;

public sealed record InvestmentRiskProfile
{
    public double WeeklyFailureChance { get; init; }
    public double ExtortionChance { get; init; }
    public double PoliceHeatChance { get; init; }
    public double BetrayalChance { get; init; }
    public int ExtortionAmountMin { get; init; }
    public int ExtortionAmountMax { get; init; }

    public static readonly InvestmentRiskProfile Low = new()
    {
        WeeklyFailureChance = 0.02,
        ExtortionChance = 0.0,
        PoliceHeatChance = 0.0,
        BetrayalChance = 0.025,
        ExtortionAmountMin = 0,
        ExtortionAmountMax = 0
    };

    public static readonly InvestmentRiskProfile Medium = new()
    {
        WeeklyFailureChance = 0.04,
        ExtortionChance = 0.035,
        PoliceHeatChance = 0.025,
        BetrayalChance = 0.04,
        ExtortionAmountMin = 8,
        ExtortionAmountMax = 15
    };

    public static readonly InvestmentRiskProfile MediumHigh = new()
    {
        WeeklyFailureChance = 0.05,
        ExtortionChance = 0.06,
        PoliceHeatChance = 0.05,
        BetrayalChance = 0.05,
        ExtortionAmountMin = 12,
        ExtortionAmountMax = 22
    };

    public static readonly InvestmentRiskProfile High = new()
    {
        WeeklyFailureChance = 0.08,
        ExtortionChance = 0.10,
        PoliceHeatChance = 0.12,
        BetrayalChance = 0.08,
        ExtortionAmountMin = 18,
        ExtortionAmountMax = 35
    };
}
