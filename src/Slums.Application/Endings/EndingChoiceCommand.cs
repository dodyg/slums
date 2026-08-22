using Slums.Core.Endings;
using Slums.Core.State;

namespace Slums.Application.Endings;

public static class EndingChoiceCommand
{
    public static bool Execute(GameSession gameSession, EndingId endingId)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return gameSession.TryChooseEnding(endingId);
    }
}
