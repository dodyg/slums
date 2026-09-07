using Slums.Core.Investments;
using Slums.Core.State;

namespace Slums.Application.Investments;

public sealed class MakeInvestmentCommand
{
    public MakeInvestmentResult Execute(GameSession gameSession, InvestmentType type)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var result = gameSession.MakeInvestment(type);
        gameSession.AddEventMessage(result.Message);
        return result;
    }
}
