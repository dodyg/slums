using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Performs a player phone action: acknowledging a tip or responding to a message.
/// </summary>
public sealed class PhoneActionCommand
{
#pragma warning disable CA1822
    public (bool Success, string Message) Execute(GameSession gameSession, string entryId, bool isTip)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return isTip
            ? gameSession.AcknowledgeTip(entryId)
            : gameSession.RespondToMessage(entryId);
    }
}
