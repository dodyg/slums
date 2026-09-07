using Slums.Core.State;

namespace Slums.Application.Phone;

/// <summary>
/// Performs a player phone action: acknowledging a tip or responding to a message.
/// </summary>
public sealed class PhoneActionCommand
{
    public (bool Success, string Message) Execute(GameSession gameSession, string entryId, bool isTip)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        return isTip
            ? gameSession.AcknowledgeTip(entryId)
            : gameSession.RespondToMessage(entryId);
    }
}
