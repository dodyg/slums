using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Refills the player's phone credit so messages can be received again.
/// </summary>
public sealed class PhoneRefillCreditCommand
{
    public (bool Success, string Message) Execute(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.RefillPhoneCredit();
    }
}
