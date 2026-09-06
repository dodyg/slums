using Slums.Core.Investments;

namespace Slums.Application.Investments;

public sealed record InvestmentMenuStatus(
    InvestmentDefinition Definition,
    bool CanInvest,
    IReadOnlyList<string> BlockingReasons,
    string OpportunitySource,
    string WeeklyReturnSummary,
    int ExpectedWeeklyIncome,
    int MidpointPaybackWeeks,
    string PerkSummary,
    string UnlockSummary,
    IReadOnlyList<string> RiskBreakdown,
    string? OwnedStateSummary,
    IReadOnlyList<string> CurrentStateNotes);
