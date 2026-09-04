using Slums.Core.Characters;
using Slums.Core.Relationships;

namespace Slums.Core.Endings;

public static class EndingKnotCatalog
{
    public const string MotherDied = "ending_mother_died";
    public const string Arrested = "ending_arrested";
    public const string Eviction = "ending_eviction";
    public const string Destitution = "ending_destitution";
    public const string DestitutionMedical = "ending_destitution_medical";
    public const string DestitutionPrisoner = "ending_destitution_prisoner";
    public const string DestitutionSudanese = "ending_destitution_sudanese";
    public const string StabilityHonestWork = "ending_stability";
    public const string CrimeKingpin = "ending_crime_kingpin";
    public const string QuitTheLuxorDream = "ending_luxor";
    public const string NetworkShelter = "ending_network_shelter";

    public const string StabilityMedical = "ending_stability_medical";
    public const string StabilityPrisoner = "ending_stability_prisoner";
    public const string StabilitySudanese = "ending_stability_sudanese";

    public const string LuxorMedical = "ending_luxor_medical";
    public const string LuxorPrisoner = "ending_luxor_prisoner";
    public const string LuxorSudanese = "ending_luxor_sudanese";

    public const string NetworkShelterMona = "ending_network_shelter_mona";
    public const string NetworkShelterSalma = "ending_network_shelter_salma";
    public const string NetworkShelterNadia = "ending_network_shelter_nadia";
    public const string NetworkShelterHanan = "ending_network_shelter_hanan";
    public const string CrisisReflection = "ending_crisis_reflection";
    public const string Commitment = "ending_commitment";

    /// <summary>Gets every ending knot that is reachable from the documented ending catalog.</summary>
    public static IReadOnlySet<string> AllKnownKnots { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        MotherDied,
        Arrested,
        Eviction,
        Destitution,
        DestitutionMedical,
        DestitutionPrisoner,
        DestitutionSudanese,
        StabilityHonestWork,
        CrimeKingpin,
        QuitTheLuxorDream,
        NetworkShelter,
        StabilityMedical,
        StabilityPrisoner,
        StabilitySudanese,
        LuxorMedical,
        LuxorPrisoner,
        LuxorSudanese,
        NetworkShelterMona,
        NetworkShelterSalma,
        NetworkShelterNadia,
        NetworkShelterHanan
        , CrisisReflection,
        Commitment
    };

    /// <summary>Rejects compiled ending content that is missing or not selected by the catalog.</summary>
    public static void ValidateKnownKnots(IReadOnlySet<string> knotNames)
    {
        ArgumentNullException.ThrowIfNull(knotNames);

        var missing = AllKnownKnots.Where(knot => !knotNames.Contains(knot)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Ending catalog references missing Ink knots: {string.Join(", ", missing)}.");
        }

        var orphaned = knotNames
            .Where(static knot => knot.StartsWith("ending_", StringComparison.Ordinal))
            .Where(knot => !AllKnownKnots.Contains(knot))
            .OrderBy(static knot => knot, StringComparer.Ordinal)
            .ToArray();
        if (orphaned.Length > 0)
        {
            throw new InvalidOperationException($"Ink story contains orphan ending knots: {string.Join(", ", orphaned)}.");
        }
    }

    public static string GetDefault(EndingId endingId)
    {
        return endingId switch
        {
            EndingId.MotherDied => MotherDied,
            EndingId.Arrested => Arrested,
            EndingId.Eviction => Eviction,
            EndingId.Destitution => Destitution,
            EndingId.StabilityHonestWork => StabilityHonestWork,
            EndingId.CrimeKingpin => CrimeKingpin,
            EndingId.QuitTheLuxorDream => QuitTheLuxorDream,
            EndingId.NetworkShelter => NetworkShelter,
            _ => throw new ArgumentOutOfRangeException(nameof(endingId))
        };
    }

    public static string GetDestitutionKnot(BackgroundType backgroundType)
    {
        return backgroundType switch
        {
            BackgroundType.MedicalSchoolDropout => DestitutionMedical,
            BackgroundType.ReleasedPoliticalPrisoner => DestitutionPrisoner,
            BackgroundType.SudaneseRefugee => DestitutionSudanese,
            _ => Destitution
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

    public static string GetLuxorKnot(BackgroundType backgroundType)
    {
        return backgroundType switch
        {
            BackgroundType.MedicalSchoolDropout => LuxorMedical,
            BackgroundType.ReleasedPoliticalPrisoner => LuxorPrisoner,
            BackgroundType.SudaneseRefugee => LuxorSudanese,
            _ => QuitTheLuxorDream
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
