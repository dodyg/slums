using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Refills the player's phone credit so messages can be received again.
/// </summary>
public sealed class PhoneRefillCreditCommand
{
#pragma warning disable CA1822
    public (bool Success, string Message) Execute(GameSession gameSession)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.RefillPhoneCredit();
    }
}
