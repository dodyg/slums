using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Adjusts an NPC's trust by <see cref="Change"/>.</summary>
public sealed record NpcTrustEffect(NpcId Npc, int Change) : NarrativeEffect;
