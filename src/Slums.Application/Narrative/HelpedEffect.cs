using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Sets the NPC's helped memory.</summary>
public sealed record HelpedEffect(NpcId Npc, bool Value) : NarrativeEffect;
