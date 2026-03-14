using Slums.Core.Characters;
using Slums.Core.Relationships;

namespace Slums.Core.Endings;

public static class EndingKnotCatalog
{
    public const string Destitution = "ending_destitution";
    public const string MotherDied = "ending_mother_died";
    public const string CollapseFromExhaustion = "ending_collapse";
    public const string StabilityHonestWork = "ending_stability";
    public const string CrimeKingpin = "ending_crime_kingpin";
    public const string QuitTheLuxorDream = "ending_luxor";
    public const string Arrested = "ending_arrested";
    public const string NetworkShelter = "ending_network_shelter";
    public const string LeavingCrime = "ending_leaving_crime";
    public const string BuriedByHeat = "ending_buried_by_heat";
    public const string Eviction = "ending_eviction";

    public const string StabilityMedical = "ending_stability_medical";
    public const string StabilityPrisoner = "ending_stability_prisoner";
    public const string StabilitySudanese = "ending_stability_sudanese";

    public const string LeavingCrimeMedical = "ending_leaving_crime_medical";
    public const string LeavingCrimePrisoner = "ending_leaving_crime_prisoner";
    public const string LeavingCrimeSudanese = "ending_leaving_crime_sudanese";

    public const string NetworkShelterMona = "ending_network_shelter_mona";
    public const string NetworkShelterSalma = "ending_network_shelter_salma";
    public const string NetworkShelterNadia = "ending_network_shelter_nadia";
    public const string NetworkShelterHanan = "ending_network_shelter_hanan";

    public static string GetDefault(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.Destitution => Destitution,
            EndingId.MotherDied => MotherDied,
            EndingId.CollapseFromExhaustion => CollapseFromExhaustion,
            EndingId.StabilityHonestWork => StabilityHonestWork,
            EndingId.CrimeKingpin => CrimeKingpin,
            EndingId.QuitTheLuxorDream => QuitTheLuxorDream,
            EndingId.Arrested => Arrested,
            EndingId.NetworkShelter => NetworkShelter,
            EndingId.LeavingCrime => LeavingCrime,
            EndingId.BuriedByHeat => BuriedByHeat,
            EndingId.Eviction => Eviction,
            _ => throw new ArgumentOutOfRangeException(nameof(endingId))
        };
    }

    public static string GetStabilityKnot(BackgroundType backgroundType)
    {
        return backgroundType switch
        {
            BackgroundType.MedicalSchoolDropout => StabilityMedical,
            BackgroundType.ReleasedPoliticalPrisoner => StabilityPrisoner,
            BackgroundType.SudaneseRefugee => StabilitySudanese,
            _ => StabilityHonestWork
        };
    }

    public static string GetLeavingCrimeKnot(BackgroundType backgroundType)
    {
        return backgroundType switch
        {
            BackgroundType.MedicalSchoolDropout => LeavingCrimeMedical,
            BackgroundType.ReleasedPoliticalPrisoner => LeavingCrimePrisoner,
            BackgroundType.SudaneseRefugee => LeavingCrimeSudanese,
            _ => LeavingCrime
        };
    }

    public static string GetNetworkShelterKnot(NpcId npcId)
    {
        return npcId switch
        {
            NpcId.NeighborMona => NetworkShelterMona,
            NpcId.NurseSalma => NetworkShelterSalma,
            NpcId.CafeOwnerNadia => NetworkShelterNadia,
            NpcId.FenceHanan => NetworkShelterHanan,
            _ => NetworkShelter
        };
    }
}
