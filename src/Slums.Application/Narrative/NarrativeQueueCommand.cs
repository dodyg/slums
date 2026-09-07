using Slums.Core.State;

namespace Slums.Application.Narrative;

/// <summary>
/// Consumes the session's pending narrative queue: queued follow-up scenes and the pending
/// ending knot. Consumption is a mutation, so it is routed through the application boundary.
/// </summary>
public sealed class NarrativeQueueCommand
{
    public bool TryDequeueScene(GameSession gameSession, out string knotName)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.TryDequeueNarrativeScene(out knotName);
    }

    public bool TryTakeEndingKnot(GameSession gameSession, out string knotName)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.TryTakePendingEndingKnot(out knotName);
    }
}
