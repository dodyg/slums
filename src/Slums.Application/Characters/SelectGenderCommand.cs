using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.Characters;

/// <summary>
/// Applies the player's chosen gender and its relationship modifiers at character creation.
/// </summary>
public sealed class SelectGenderCommand
{
    public void Execute(GameSession gameSession, Gender gender)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        gameSession.Player.ApplyGender(gender);
        gameSession.ApplyGenderRelationshipModifiers();
    }
}
