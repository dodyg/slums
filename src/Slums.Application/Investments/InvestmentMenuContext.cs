using Slums.Core.Investments;
using Slums.Core.State;

namespace Slums.Application.Investments;

public sealed record InvestmentMenuContext(
    IReadOnlyList<InvestmentDefinition> Opportunities,
    IReadOnlyDictionary<InvestmentType, InvestmentEligibility> EligibilityByType,
    IReadOnlyList<Investment> ActiveInvestments,
    string? LocationName)
{
    public static InvestmentMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var opportunities = gameSession.GetCurrentInvestmentOpportunities();

        return new InvestmentMenuContext(
            opportunities,
            opportunities.ToDictionary(
                static definition => definition.Type,
                definition => gameSession.CheckInvestmentEligibility(definition)),
            gameSession.ActiveInvestments,
            gameSession.World.GetCurrentLocation()?.Name);
    }
}
