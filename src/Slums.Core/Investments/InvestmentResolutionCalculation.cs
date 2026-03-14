namespace Slums.Core.Investments;

public sealed record InvestmentResolutionCalculation(
    InvestmentResolution Resolution,
    bool ShouldSuspend);
