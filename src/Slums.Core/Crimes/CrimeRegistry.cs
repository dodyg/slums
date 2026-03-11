using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Crimes;

public static class CrimeRegistry
{
    private static readonly CrimeAttempt PettyTheftDefault = new(CrimeType.PettyTheft, 25, 20, 10, 0, 10);
    private static readonly CrimeAttempt RobberyDefault = new(CrimeType.Robbery, 70, 55, 25, 10, 25);
    private static readonly CrimeAttempt HashishTradeDefault = new(CrimeType.HashishTrade, 45, 35, 15, 5, 15);

    public static IReadOnlyList<CrimeAttempt> GetAvailableCrimes(Location location, RelationshipState relationshipState)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(relationshipState);

        if (!location.HasCrimeOpportunities)
        {
            return [];
        }

        var currentStreetRep = relationshipState.GetFactionStanding(FactionId.ImbabaCrew).Reputation;
        IReadOnlyList<CrimeAttempt> crimes = location.District switch
        {
            DistrictId.Dokki =>
            [
                PettyTheftDefault with { BaseReward = 35, DetectionRisk = 30 },
                RobberyDefault with { BaseReward = 90, DetectionRisk = 65 }
            ],
            _ =>
            [
                PettyTheftDefault,
                HashishTradeDefault,
                RobberyDefault
            ]
        };

        return crimes.Where(crime => currentStreetRep >= crime.StreetRepRequired).ToArray();
    }
}