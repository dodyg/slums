using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Rumors;

public static class RumorGenerator
{
    public static Rumor OnCrimeSuccess(DistrictId district, int day)
    {
        return CreateRumor(RumorId.CrimeSuccess, "Successful crime nearby",
            district, day, intensity: 5, isPositive: false,
            GetNpcsInDistrict(district), trustModifier: -2);
    }

    public static Rumor OnCrimeDetected(DistrictId district, int day)
    {
        return CreateRumor(RumorId.CrimeDetected, "People are asking questions",
            district, day, intensity: 8, isPositive: false,
            GetNpcsInDistrict(district), trustModifier: -4);
    }

    public static Rumor OnRentUnpaid(int day)
    {
        return CreateRumor(RumorId.RentUnpaid, "Falling behind on rent",
            DistrictId.Imbaba, day, intensity: 3, isPositive: false,
            GetNpcsInDistrict(DistrictId.Imbaba), trustModifier: -1);
    }

    public static Rumor OnExpensivePurchase(DistrictId district, int cost, int day)
    {
        var intensity = cost > 100 ? 7 : 5;
        return CreateRumor(RumorId.ExpensivePurchase, "Where did she get that money?",
            district, day, intensity, isPositive: false,
            GetNpcsInDistrict(district), trustModifier: -2);
    }

    public static Rumor OnSkippingCommunityEvents(int consecutiveSkips, int day)
    {
        return CreateRumor(RumorId.SkippingCommunityEvents, "She thinks she's too good for the neighborhood",
            DistrictId.Imbaba, day, intensity: 4, isPositive: false,
            GetNpcsInDistrict(DistrictId.Imbaba), trustModifier: -2);
    }

    private static Rumor CreateRumor(
        RumorId id, string sourceAction, DistrictId district, int day,
        int intensity, bool isPositive, IReadOnlySet<NpcId> affectedNpcs, int trustModifier)
    {
        return new Rumor(id, sourceAction, district, day, intensity, isPositive,
            affectedNpcs, trustModifier, []);
    }

    private static HashSet<NpcId> GetNpcsInDistrict(DistrictId district)
    {
        return district switch
        {
            DistrictId.Imbaba => new HashSet<NpcId> { NpcId.LandlordHajjMahmoud, NpcId.FixerUmmKarim, NpcId.NeighborMona, NpcId.NurseSalma, NpcId.CafeOwnerNadia },
            DistrictId.Dokki => new HashSet<NpcId> { NpcId.WorkshopBossAbuSamir, NpcId.FenceHanan, NpcId.RunnerYoussef, NpcId.PharmacistMariam },
            DistrictId.BulaqAlDakrour => new HashSet<NpcId> { NpcId.DispatcherSafaa, NpcId.LaundryOwnerIman },
            _ => new HashSet<NpcId>()
        };
    }
}
