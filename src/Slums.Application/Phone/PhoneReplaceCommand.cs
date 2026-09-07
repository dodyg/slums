using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Replaces the player's lost phone (includes credit refill).
/// </summary>
public sealed class PhoneReplaceCommand
{
    public (bool Success, string Message) Execute(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.ReplacePhone();
    }
}
