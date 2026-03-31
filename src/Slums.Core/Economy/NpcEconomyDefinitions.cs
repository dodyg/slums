using Slums.Core.Relationships;

namespace Slums.Core.Economy;

public sealed record NpcEconomyDefinition(NpcId Npc, NpcWealthLevel StartingWealth, int Generosity, int HardshipChance, int WindfallChance);

public static class NpcEconomyDefinitions
{
    public static IReadOnlyList<NpcEconomyDefinition> All =>
    [
        new(NpcId.LandlordHajjMahmoud, NpcWealthLevel.Comfortable, 3, 20, 10),
        new(NpcId.NeighborMona, NpcWealthLevel.Poor, 7, 30, 15),
        new(NpcId.FixerUmmKarim, NpcWealthLevel.Stable, 5, 25, 15),
        new(NpcId.NurseSalma, NpcWealthLevel.Stable, 6, 25, 15),
        new(NpcId.WorkshopBossAbuSamir, NpcWealthLevel.Stable, 2, 20, 10),
        new(NpcId.CafeOwnerNadia, NpcWealthLevel.Comfortable, 3, 20, 15),
        new(NpcId.FenceHanan, NpcWealthLevel.Comfortable, 2, 25, 20),
        new(NpcId.RunnerYoussef, NpcWealthLevel.Poor, 4, 35, 10),
        new(NpcId.PharmacistMariam, NpcWealthLevel.Stable, 4, 25, 15),
        new(NpcId.OfficerKhalid, NpcWealthLevel.Comfortable, 1, 20, 10),
        new(NpcId.DispatcherSafaa, NpcWealthLevel.Poor, 5, 30, 10),
        new(NpcId.LaundryOwnerIman, NpcWealthLevel.Stable, 3, 25, 15)
    ];
}
