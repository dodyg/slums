using Slums.Core.World;

namespace Slums.Core.Heat;

public static class HeatBleedOverTable
{
    public static IReadOnlyList<(DistrictId From, DistrictId To, double Rate)> Relationships { get; } =
    [
        (DistrictId.Imbaba, DistrictId.BulaqAlDakrour, 0.15),
        (DistrictId.BulaqAlDakrour, DistrictId.Imbaba, 0.15),
        (DistrictId.Imbaba, DistrictId.Dokki, 0.05),
        (DistrictId.Dokki, DistrictId.Imbaba, 0.05),
        (DistrictId.ArdAlLiwa, DistrictId.Imbaba, 0.10),
        (DistrictId.Imbaba, DistrictId.ArdAlLiwa, 0.10),
        (DistrictId.ArdAlLiwa, DistrictId.Dokki, 0.05),
        (DistrictId.Dokki, DistrictId.ArdAlLiwa, 0.05)
    ];
}
