using Slums.Core.Narrative;

namespace Slums.Application.Narrative;

public sealed record CrisisDecisionEffect(CityCrisisDecision Decision) : NarrativeEffect;
