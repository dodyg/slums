using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.Characters;

/// <summary>
/// Applies the player's chosen background at character creation.
/// </summary>
public sealed class SelectBackgroundCommand
{
    public void Execute(GameSession gameSession, Background background)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(background);

        gameSession.Player.ApplyBackground(background);
    }
}
