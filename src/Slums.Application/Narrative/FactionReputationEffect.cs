using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Adjusts a faction's reputation by <see cref="Change"/>.</summary>
public sealed record FactionReputationEffect(FactionId Faction, int Change) : NarrativeEffect;
