using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Replaces the player's lost phone (includes credit refill).
/// </summary>
public sealed class PhoneReplaceCommand
{
#pragma warning disable CA1822
    public (bool Success, string Message) Execute(GameSession gameSession)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.ReplacePhone();
    }
}
