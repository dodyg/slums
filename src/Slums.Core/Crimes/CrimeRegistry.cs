using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Crimes;

public static class CrimeRegistry
{
    private static readonly CrimeAttempt PettyTheftDefault = new(CrimeType.PettyTheft, 25, 20, 10, 0, 10);
    private static readonly CrimeAttempt RobberyDefault = new(CrimeType.Robbery, 70, 55, 25, 10, 25);
    private static readonly CrimeAttempt HashishTradeDefault = new(CrimeType.HashishTrade, 45, 35, 15, 5, 15);
    private static readonly CrimeAttempt HananFencingRoute = new(CrimeType.MarketFencing, 60, 18, 8, 0, 14);
    private static readonly CrimeAttempt YoussefDropRoute = new(CrimeType.DokkiDrop, 95, 42, 24, 0, 18);
    private static readonly CrimeAttempt UmmKarimNetworkErrand = new(CrimeType.NetworkErrand, 130, 50, 30, 0, 24);

    public static IReadOnlyList<CrimeAttempt> GetAvailableCrimes(Location location, RelationshipState relationshipState)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(relationshipState);

        if (!location.HasCrimeOpportunities)
        {
            return [];
        }

        var currentStreetRep = GetStreetReputation(location.District, relationshipState);
        IReadOnlyList<CrimeAttempt> crimes = location.District switch
        {
            DistrictId.Dokki => GetDokkiCrimes(location, relationshipState),
            _ => GetImbabaCrimes(location, relationshipState)
        };

        return crimes.Where(crime => currentStreetRep >= crime.StreetRepRequired).ToArray();
    }

    public static IReadOnlyList<CrimeOpportunityStatus> GetCrimeOpportunityStatuses(Location location, RelationshipState relationshipState)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(relationshipState);

        if (!location.HasCrimeOpportunities)
        {
            return [];
        }

        var currentStreetRep = GetStreetReputation(location.District, relationshipState);

        return location.District switch
        {
            DistrictId.Dokki => GetDokkiCrimeStatuses(location, relationshipState, currentStreetRep),
            _ => GetImbabaCrimeStatuses(location, relationshipState, currentStreetRep)
        };
    }

    private static List<CrimeAttempt> GetImbabaCrimes(Location location, RelationshipState relationshipState)
    {
        var hananTrust = relationshipState.GetNpcRelationship(NpcId.FenceHanan).Trust;
        var ummKarimTrust = relationshipState.GetNpcRelationship(NpcId.FixerUmmKarim).Trust;
        var imbabaReputation = relationshipState.GetFactionStanding(FactionId.ImbabaCrew).Reputation;

        var crimes = new List<CrimeAttempt>
        {
            PettyTheftDefault,
            HashishTradeDefault,
            RobberyDefault
        };

        if (location.Id == LocationId.Market && hananTrust >= 10)
        {
            crimes.Add(HananFencingRoute);
        }

        if (location.Id == LocationId.Market && ummKarimTrust >= 12 && imbabaReputation >= 15)
        {
            crimes.Add(UmmKarimNetworkErrand);
        }

        return crimes;
    }

    private static List<CrimeAttempt> GetDokkiCrimes(Location location, RelationshipState relationshipState)
    {
        var youssefTrust = relationshipState.GetNpcRelationship(NpcId.RunnerYoussef).Trust;
        var dokkiReputation = relationshipState.GetFactionStanding(FactionId.DokkiThugs).Reputation;

        var crimes = new List<CrimeAttempt>
        {
            PettyTheftDefault with
            {
                BaseReward = 35,
                DetectionRisk = 30
            },
            RobberyDefault with
            {
                BaseReward = 90,
                DetectionRisk = 65
            }
        };

        if (location.Id == LocationId.Square && (youssefTrust >= 10 || dokkiReputation >= 10))
        {
            crimes.Add(HashishTradeDefault with
            {
                BaseReward = 55,
                DetectionRisk = 40,
                PolicePressureIncrease = 12
            });
        }

        if (location.Id == LocationId.Square && (youssefTrust >= 15 || dokkiReputation >= 15))
        {
            crimes.Add(YoussefDropRoute);
        }

        return crimes;
    }

    private static int GetStreetReputation(DistrictId districtId, RelationshipState relationshipState)
    {
        var factionId = districtId switch
        {
            DistrictId.Dokki => FactionId.DokkiThugs,
            DistrictId.ArdAlLiwa => FactionId.ExPrisonerNetwork,
            _ => FactionId.ImbabaCrew
        };

        return relationshipState.GetFactionStanding(factionId).Reputation;
    }

    private static IReadOnlyList<CrimeOpportunityStatus> GetImbabaCrimeStatuses(Location location, RelationshipState relationshipState, int currentStreetRep)
    {
        var hananTrust = relationshipState.GetNpcRelationship(NpcId.FenceHanan).Trust;
        var ummKarimTrust = relationshipState.GetNpcRelationship(NpcId.FixerUmmKarim).Trust;
        var imbabaReputation = relationshipState.GetFactionStanding(FactionId.ImbabaCrew).Reputation;

        return
        [
            CreateStatus(PettyTheftDefault, currentStreetRep, null),
            CreateStatus(HashishTradeDefault, currentStreetRep, null),
            CreateStatus(RobberyDefault, currentStreetRep, null),
            CreateStatus(
                HananFencingRoute,
                currentStreetRep,
                location.Id != LocationId.Market
                    ? "Only runs out of the market."
                    : hananTrust < 10
                        ? $"Requires Hanan trust 10. Current: {hananTrust}."
                        : null),
            CreateStatus(
                UmmKarimNetworkErrand,
                currentStreetRep,
                location.Id != LocationId.Market
                    ? "Only runs out of the market."
                    : ummKarimTrust < 12
                        ? $"Requires Umm Karim trust 12. Current: {ummKarimTrust}."
                        : imbabaReputation < 15
                            ? $"Requires Imbaba standing 15. Current: {imbabaReputation}."
                            : null)
        ];
    }

    private static IReadOnlyList<CrimeOpportunityStatus> GetDokkiCrimeStatuses(Location location, RelationshipState relationshipState, int currentStreetRep)
    {
        var youssefTrust = relationshipState.GetNpcRelationship(NpcId.RunnerYoussef).Trust;
        var dokkiReputation = relationshipState.GetFactionStanding(FactionId.DokkiThugs).Reputation;

        var pettyTheft = PettyTheftDefault with
        {
            BaseReward = 35,
            DetectionRisk = 30
        };

        var robbery = RobberyDefault with
        {
            BaseReward = 90,
            DetectionRisk = 65
        };

        var hashishTrade = HashishTradeDefault with
        {
            BaseReward = 55,
            DetectionRisk = 40,
            PolicePressureIncrease = 12
        };

        return
        [
            CreateStatus(pettyTheft, currentStreetRep, null),
            CreateStatus(robbery, currentStreetRep, null),
            CreateStatus(
                hashishTrade,
                currentStreetRep,
                location.Id != LocationId.Square
                    ? "Only available around the square."
                    : youssefTrust < 10 && dokkiReputation < 10
                        ? $"Requires Youssef trust 10 or Dokki standing 10. Current: {youssefTrust}/{dokkiReputation}."
                        : null),
            CreateStatus(
                YoussefDropRoute,
                currentStreetRep,
                location.Id != LocationId.Square
                    ? "Only available around the square."
                    : youssefTrust < 15 && dokkiReputation < 15
                        ? $"Requires Youssef trust 15 or Dokki standing 15. Current: {youssefTrust}/{dokkiReputation}."
                        : null)
        ];
    }

    private static CrimeOpportunityStatus CreateStatus(CrimeAttempt attempt, int currentStreetRep, string? routeRequirementReason)
    {
        if (!string.IsNullOrEmpty(routeRequirementReason))
        {
            return new CrimeOpportunityStatus(attempt, false, routeRequirementReason);
        }

        if (currentStreetRep < attempt.StreetRepRequired)
        {
            return new CrimeOpportunityStatus(
                attempt,
                false,
                $"Requires street rep {attempt.StreetRepRequired}. Current: {currentStreetRep}.");
        }

        return new CrimeOpportunityStatus(attempt, true, null);
    }
}