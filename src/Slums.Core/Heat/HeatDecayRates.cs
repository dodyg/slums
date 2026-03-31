using Slums.Core.World;

namespace Slums.Core.Heat;

public static class HeatDecayRates
{
    public static int GetDecayRate(DistrictId district) => district switch
    {
        DistrictId.Dokki => 6,
        DistrictId.Imbaba => 4,
        DistrictId.BulaqAlDakrour => 3,
        DistrictId.Shubra => 3,
        DistrictId.ArdAlLiwa => 3,
        _ => 4
    };
}
