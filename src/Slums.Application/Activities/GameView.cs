using Slums.Core.State;

namespace Slums.Application.Activities;

/// <summary>
/// Read-only projections used by the presentation layer for one render/update observation.
/// </summary>
public sealed record GameView(GameStatusContext Status, GameActionMenuContext Actions)
{
    /// <summary>Builds a presentation projection without granting the caller mutation access.</summary>
    public static GameView Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameView(
            GameStatusContext.Create(gameSession),
            GameActionMenuContext.Create(gameSession));
    }
}
