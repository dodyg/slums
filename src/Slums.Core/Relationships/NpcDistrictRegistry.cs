using Slums.Core.World;

namespace Slums.Core.Relationships;

/// <summary>Canonical home and work district membership for recurring NPCs.</summary>
public static class NpcDistrictRegistry
{
    private static readonly Dictionary<NpcId, DistrictId> Districts = new()
    {
        [NpcId.LandlordHajjMahmoud] = DistrictId.Imbaba,
        [NpcId.FixerUmmKarim] = DistrictId.Imbaba,
        [NpcId.NeighborMona] = DistrictId.Imbaba,
        [NpcId.FenceHanan] = DistrictId.Imbaba,
        [NpcId.NurseSalma] = DistrictId.ArdAlLiwa,
        [NpcId.WorkshopBossAbuSamir] = DistrictId.ArdAlLiwa,
        [NpcId.CafeOwnerNadia] = DistrictId.Dokki,
        [NpcId.PharmacistMariam] = DistrictId.BulaqAlDakrour,
        [NpcId.DispatcherSafaa] = DistrictId.BulaqAlDakrour,
        [NpcId.LaundryOwnerIman] = DistrictId.Shubra,
        [NpcId.RunnerYoussef] = DistrictId.DowntownCairo,
        [NpcId.VendorTarek] = DistrictId.DowntownCairo,
        [NpcId.OfficerKhalid] = DistrictId.DowntownCairo
    };

    public static DistrictId GetDistrict(NpcId npcId)
    {
        return Districts.TryGetValue(npcId, out var district)
            ? district
            : throw new ArgumentOutOfRangeException(nameof(npcId), npcId, "NPC has no canonical district.");
    }

    public static IReadOnlyList<NpcId> GetNpcsInDistrict(DistrictId district)
    {
        return Districts
            .Where(pair => pair.Value == district)
            .Select(static pair => pair.Key)
            .Order()
            .ToArray();
    }
}
