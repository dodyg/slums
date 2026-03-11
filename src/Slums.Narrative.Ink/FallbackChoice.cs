using Slums.Application.Narrative;

namespace Slums.Narrative.Ink;

internal sealed record FallbackChoice(string Text, string NextNodeId, NarrativeOutcome? Outcome = null);