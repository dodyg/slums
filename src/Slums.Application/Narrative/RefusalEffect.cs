using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Records that the player refused the NPC.</summary>
public sealed record RefusalEffect(NpcId Npc) : NarrativeEffect;
