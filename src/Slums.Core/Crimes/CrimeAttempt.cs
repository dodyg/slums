namespace Slums.Core.Crimes;

public sealed record CrimeAttempt(
    CrimeType Type,
    int BaseReward,
    int DetectionRisk,
    int PolicePressureIncrease,
    int StreetRepRequired,
    int EnergyCost)
{
    /// <summary>In-game minutes consumed by attempting this route.</summary>
    public int DurationMinutes => Type switch
    {
        CrimeType.PettyTheft => 60,
        CrimeType.Robbery => 180,
        CrimeType.HashishTrade => 120,
        CrimeType.MarketFencing => 90,
        CrimeType.DokkiDrop => 120,
        CrimeType.NetworkErrand => 180,
        CrimeType.DepotFareSkim => 90,
        CrimeType.ShubraBundleLift => 90,
        CrimeType.WorkshopContraband => 120,
        CrimeType.BulaqProtectionRacket => 120,
        _ => 60
    };

    public string Name => Type switch
    {
        CrimeType.PettyTheft => "Petty Theft",
        CrimeType.Robbery => "Robbery",
        CrimeType.HashishTrade => "Hashish Trade",
        CrimeType.MarketFencing => "Market Fencing Route",
        CrimeType.DokkiDrop => "Dokki Drop Route",
        CrimeType.NetworkErrand => "Network Errand",
        CrimeType.DepotFareSkim => "Depot Fare Skim",
        CrimeType.ShubraBundleLift => "Shubra Bundle Lift",
        CrimeType.WorkshopContraband => "Workshop Contraband Run",
        CrimeType.BulaqProtectionRacket => "Protection Collection",
        _ => Type.ToString()
    };
}
