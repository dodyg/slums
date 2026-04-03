using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Investments;

public sealed record InvestmentEligibilityContext(
    int CurrentMoney,
    LocationId CurrentLocationId,
    IReadOnlySet<NpcId> ReachableNpcs,
    IReadOnlySet<InvestmentType> OwnedInvestmentTypes,
    RelationshipState Relationships,
    int TotalCrimeEarnings,
    int StreetSmartsLevel,
    int MedicalLevel,
    int PhysicalLevel,
    BackgroundType BackgroundType);
