namespace Slums.Core.Crimes;

public sealed record CrimeAttempt(
    CrimeType Type,
    int BaseReward,
    int DetectionRisk,
    int PolicePressureIncrease,
    int StreetRepRequired,
    int EnergyCost)
{
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
        _ => Type.ToString()
    };
}