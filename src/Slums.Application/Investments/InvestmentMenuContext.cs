using Slums.Core.Investments;
using Slums.Core.State;

namespace Slums.Application.Investments;

public sealed record InvestmentMenuContext(
    GameSession GameSession,
    IReadOnlyList<InvestmentDefinition> Opportunities,
    IReadOnlyList<Investment> ActiveInvestments,
    string? LocationName)
{
    public static InvestmentMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new InvestmentMenuContext(
            gameSession,
            gameSession.GetCurrentInvestmentOpportunities(),
            gameSession.ActiveInvestments,
            gameSession.World.GetCurrentLocation()?.Name);
    }
}
