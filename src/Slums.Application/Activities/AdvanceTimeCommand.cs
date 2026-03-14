using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class AdvanceTimeCommand
{
#pragma warning disable CA1822
    public void Execute(GameSession gameSession, int minutes)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        gameSession.AdvanceTime(minutes);
    }
}
