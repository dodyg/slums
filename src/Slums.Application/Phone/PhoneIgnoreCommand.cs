using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Performs a player phone dismissal: ignoring a tip or a message.
/// </summary>
public sealed class PhoneIgnoreCommand
{
#pragma warning disable CA1822
    public (bool Success, string Message, int TrustLoss) Execute(GameSession gameSession, string entryId, bool isTip)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return isTip
            ? gameSession.IgnoreTipAction(entryId)
            : gameSession.IgnoreMessage(entryId);
    }
}
