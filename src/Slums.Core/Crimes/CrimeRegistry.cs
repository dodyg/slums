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
            DistrictId.Dokki => GetDokkiCrimes(location, relationshipState),
            _ => GetImbabaCrimes(location, relationshipState)
        };

        return crimes.Where(crime => currentStreetRep >= crime.StreetRepRequired).ToArray();
    }

    private static IReadOnlyList<CrimeAttempt> GetImbabaCrimes(Location location, RelationshipState relationshipState)
    {
        var hananTrust = relationshipState.GetNpcRelationship(NpcId.FenceHanan).Trust;
        if (location.Id != LocationId.Market || hananTrust < 10)
        {
            return [PettyTheftDefault, HashishTradeDefault, RobberyDefault];
        }

        return
        [
            PettyTheftDefault with
            {
                BaseReward = PettyTheftDefault.BaseReward + 10,
                DetectionRisk = Math.Max(5, PettyTheftDefault.DetectionRisk - 5)
            },
            HashishTradeDefault with
            {
                BaseReward = HashishTradeDefault.BaseReward + 15,
                DetectionRisk = Math.Max(5, HashishTradeDefault.DetectionRisk - 5)
            },
            RobberyDefault
        ];
    }

    private static List<CrimeAttempt> GetDokkiCrimes(Location location, RelationshipState relationshipState)
    {
        var youssefTrust = relationshipState.GetNpcRelationship(NpcId.RunnerYoussef).Trust;
        var dokkiReputation = relationshipState.GetFactionStanding(FactionId.DokkiThugs).Reputation;
        var riskReduction = youssefTrust >= 15 ? 5 : 0;

        var crimes = new List<CrimeAttempt>
        {
            PettyTheftDefault with
            {
                BaseReward = 35,
                DetectionRisk = Math.Max(5, 30 - riskReduction)
            },
            RobberyDefault with
            {
                BaseReward = 90,
                DetectionRisk = Math.Max(5, 65 - riskReduction)
            }
        };

        if (location.Id == LocationId.Square && (youssefTrust >= 10 || dokkiReputation >= 10))
        {
            crimes.Add(HashishTradeDefault with
            {
                BaseReward = 55,
                DetectionRisk = Math.Max(5, 40 - riskReduction),
                PolicePressureIncrease = 12
            });
        }

        return crimes;
    }
}