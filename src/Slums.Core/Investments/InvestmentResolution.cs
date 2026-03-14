namespace Slums.Core.Investments;

public sealed record InvestmentResolution(
    InvestmentType Type,
    int Income,
    bool WasLost,
    int ExtortionPaid,
    int PolicePressureIncrease,
    int InvestedAmountLost,
    string Message);
