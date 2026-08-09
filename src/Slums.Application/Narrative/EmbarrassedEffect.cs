using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Sets the NPC's embarrassment memory.</summary>
public sealed record EmbarrassedEffect(NpcId Npc, bool Value) : NarrativeEffect;
