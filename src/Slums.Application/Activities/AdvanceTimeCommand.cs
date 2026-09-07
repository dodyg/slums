using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class AdvanceTimeCommand
{
    public void Execute(GameSession gameSession, int minutes)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        gameSession.AdvanceTime(minutes);
    }
}
