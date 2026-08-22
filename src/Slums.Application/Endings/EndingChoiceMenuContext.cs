using Slums.Core.Endings;
using Slums.Core.State;

namespace Slums.Application.Endings;

public sealed record EndingChoiceMenuContext(IReadOnlyList<EndingId> AvailableEndings)
{
    public static EndingChoiceMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new EndingChoiceMenuContext(gameSession.GetAvailableEndingChoices());
    }
}
