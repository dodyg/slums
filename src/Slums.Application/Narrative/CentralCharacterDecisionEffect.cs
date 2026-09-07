using Slums.Core.Characters;
using Slums.Core.Narrative;

namespace Slums.Application.Narrative;

public sealed record CentralCharacterDecisionEffect(CentralCharacterId Character, CentralArcDecision Decision) : NarrativeEffect;
