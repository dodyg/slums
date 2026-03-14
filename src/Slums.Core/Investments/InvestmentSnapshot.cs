namespace Slums.Core.Investments;

public sealed record InvestmentSnapshot(
    InvestmentType Type,
    int InvestedAmount,
    int WeeklyIncomeMin,
    int WeeklyIncomeMax,
    int WeeksActive,
    bool IsSuspended);
