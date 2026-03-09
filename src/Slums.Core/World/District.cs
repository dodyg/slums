namespace Slums.Core.World;

public enum DistrictId
{
    Dokki,
    Imbaba,
    ArdAlLiwa
}

public static class DistrictInfo
{
    public static string GetName(DistrictId district) => district switch
    {
        DistrictId.Dokki => "Dokki",
        DistrictId.Imbaba => "Imbaba",
        DistrictId.ArdAlLiwa => "Ard al-Liwa",
        _ => throw new ArgumentOutOfRangeException(nameof(district))
    };

    public static string GetDescription(DistrictId district) => district switch
    {
        DistrictId.Dokki => "A relatively affluent district with office buildings and shops.",
        DistrictId.Imbaba => "A densely populated working-class neighborhood.",
        DistrictId.ArdAlLiwa => "An informal settlement on the city's edge.",
        _ => throw new ArgumentOutOfRangeException(nameof(district))
    };
}
