namespace Slums.Core.World;

public enum DistrictId
{
    Dokki,
    Imbaba,
    ArdAlLiwa,
    BulaqAlDakrour,
    Shubra
}

public static class DistrictInfo
{
    public static string GetName(DistrictId district) => district switch
    {
        DistrictId.Dokki => "Dokki",
        DistrictId.Imbaba => "Imbaba",
        DistrictId.ArdAlLiwa => "Ard al-Liwa",
        DistrictId.BulaqAlDakrour => "Bulaq al-Dakrour",
        DistrictId.Shubra => "Shubra",
        _ => throw new ArgumentOutOfRangeException(nameof(district))
    };

    public static string GetDescription(DistrictId district) => district switch
    {
        DistrictId.Dokki => "A relatively affluent district with office buildings and shops.",
        DistrictId.Imbaba => "A densely populated working-class neighborhood.",
        DistrictId.ArdAlLiwa => "An informal settlement on the city's edge.",
        DistrictId.BulaqAlDakrour => "A sprawling working-class district where transport, cheap clinics, and long commutes shape the day.",
        DistrictId.Shubra => "A crowded northern district of workshops, laundries, apartment blocks, and relentless neighborhood trade.",
        _ => throw new ArgumentOutOfRangeException(nameof(district))
    };
}
