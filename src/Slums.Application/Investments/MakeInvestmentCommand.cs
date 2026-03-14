using Slums.Core.Investments;
using Slums.Core.State;

namespace Slums.Application.Investments;

public sealed class MakeInvestmentCommand
{
#pragma warning disable CA1822
    public MakeInvestmentResult Execute(GameSession gameSession, InvestmentType type)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var result = gameSession.MakeInvestment(type);
        gameSession.AddEventMessage(result.Message);
        return result;
    }
}
