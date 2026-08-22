using Slums.Core.State;

namespace Slums.Application.Narrative;

/// <summary>Applies a completed Ink scene outcome through the canonical game-session boundary.</summary>
public static class ApplyNarrativeOutcomeCommand
{
    /// <summary>Applies the outcome and records its source knot and resulting state mutation.</summary>
    public static void Execute(GameSession gameSession, string sourceKnot, NarrativeOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKnot);
        ArgumentNullException.ThrowIfNull(outcome);

        gameSession.ApplyNarrativeOutcome(sourceKnot, outcome.Message, state => state.ApplyOutcome(outcome));
    }
}
