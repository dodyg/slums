using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Records that the player did the NPC a favor.</summary>
public sealed record FavorEffect(NpcId Npc) : NarrativeEffect;
