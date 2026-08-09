using Slums.Core.State;

namespace Slums.Application.Narrative;

/// <summary>
/// Consumes the session's pending narrative queue: queued follow-up scenes and the pending
/// ending knot. Consumption is a mutation, so it is routed through the application boundary.
/// </summary>
public sealed class NarrativeQueueCommand
{
#pragma warning disable CA1822
    public bool TryDequeueScene(GameSession gameSession, out string knotName)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.TryDequeueNarrativeScene(out knotName);
    }

#pragma warning disable CA1822
    public bool TryTakeEndingKnot(GameSession gameSession, out string knotName)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return gameSession.TryTakePendingEndingKnot(out knotName);
    }
}
