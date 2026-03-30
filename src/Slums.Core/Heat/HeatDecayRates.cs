using Slums.Core.World;

namespace Slums.Core.Heat;

public static class HeatDecayRates
{
    public static int GetDecayRate(DistrictId district) => district switch
    {
        DistrictId.Dokki => 5,
        DistrictId.Imbaba => 3,
        DistrictId.BulaqAlDakrour => 2,
        DistrictId.Shubra => 2,
        DistrictId.ArdAlLiwa => 2,
        _ => 3
    };
}
