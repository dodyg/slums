using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.Characters;

/// <summary>
/// Applies the player's chosen background at character creation.
/// </summary>
public sealed class SelectBackgroundCommand
{
#pragma warning disable CA1822
    public void Execute(GameSession gameSession, Background background)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(background);

        gameSession.Player.ApplyBackground(background);
    }
}
