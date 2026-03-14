namespace Slums.Core.Investments;

public sealed record InvestmentEligibility(bool IsEligible, IReadOnlyList<string> FailureReasons)
{
    public static readonly InvestmentEligibility Eligible = new(true, []);
}
