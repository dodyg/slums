using Slums.Core.Relationships;

namespace Slums.Core.Economy;

public sealed record NpcEconomyDefinition(NpcId Npc, NpcWealthLevel StartingWealth, int Generosity, int HardshipChance, int WindfallChance);
