using Slums.Core.Investments;

namespace Slums.Application.Investments;

public sealed record InvestmentMenuStatus(
    InvestmentDefinition Definition,
    bool CanInvest,
    IReadOnlyList<string> BlockingReasons,
    string OpportunitySource,
    string WeeklyReturnSummary,
    string UnlockSummary);
