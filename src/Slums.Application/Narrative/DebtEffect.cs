using Slums.Core.Relationships;

namespace Slums.Application.Narrative;

/// <summary>Sets the NPC's unpaid-debt state.</summary>
public sealed record DebtEffect(NpcId Npc, bool HasUnpaidDebt) : NarrativeEffect;
