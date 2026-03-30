using Slums.Core.Characters;
using Slums.Core.Clock;

namespace Slums.Core.Community;

public sealed class CommunityEventRegistry
{
    private static readonly CommunityEventDefinition[] Events =
    [
        new CommunityEventDefinition(
            CommunityEventId.FridayRooftopGathering,
            "Friday Rooftop Gathering",
            "Weekly gathering on the rooftop. Neighbors share tea, news, and complaints.",
            TimeCostMinutes: 120,
            MoneyCost: 0,
            StressChange: -5,
            TrustGainCount: 3,
            TrustGainAmount: 2,
            ProvidesFoodAccess: false,
            ProvidesInformationTips: true,
            RequiresFriday: true,
            RequiresRamadan: false,
            RequiresNpcInvitation: false,
            IsSeasonal: false,
            HasPickpocketRisk: false),
        new CommunityEventDefinition(
            CommunityEventId.RamadanIftarSharing,
            "Ramadan Iftar Sharing",
            "Community iftar with shared food. The long table stretches across the alley.",
            TimeCostMinutes: 180,
            MoneyCost: 10,
            StressChange: -10,
            TrustGainCount: 5,
            TrustGainAmount: 2,
            ProvidesFoodAccess: true,
            ProvidesInformationTips: false,
            RequiresFriday: false,
            RequiresRamadan: true,
            RequiresNpcInvitation: false,
            IsSeasonal: false,
            HasPickpocketRisk: false),
        new CommunityEventDefinition(
            CommunityEventId.NeighborhoodCleanup,
            "Neighborhood Cleanup",
            "Monthly effort to clean the building and alley. Everyone pitches in.",
            TimeCostMinutes: 180,
            MoneyCost: 0,
            StressChange: -3,
            TrustGainCount: 4,
            TrustGainAmount: 1,
            ProvidesFoodAccess: false,
            ProvidesInformationTips: false,
            RequiresFriday: false,
            RequiresRamadan: false,
            RequiresNpcInvitation: false,
            IsSeasonal: false,
            HasPickpocketRisk: false),
        new CommunityEventDefinition(
            CommunityEventId.RooftopTeaCircle,
            "Rooftop Tea Circle",
            "An informal gathering by invitation. Quiet conversation and useful information.",
            TimeCostMinutes: 90,
            MoneyCost: 0,
            StressChange: -8,
            TrustGainCount: 2,
            TrustGainAmount: 1,
            ProvidesFoodAccess: false,
            ProvidesInformationTips: true,
            RequiresFriday: false,
            RequiresRamadan: false,
            RequiresNpcInvitation: true,
            IsSeasonal: false,
            HasPickpocketRisk: false),
        new CommunityEventDefinition(
            CommunityEventId.MulidFestival,
            "Mulid Festival",
            "Saint's festival in Imbaba streets. Music, crowds, food stalls, and pickpockets.",
            TimeCostMinutes: 240,
            MoneyCost: 10,
            StressChange: -15,
            TrustGainCount: 5,
            TrustGainAmount: 2,
            ProvidesFoodAccess: true,
            ProvidesInformationTips: true,
            RequiresFriday: false,
            RequiresRamadan: false,
            RequiresNpcInvitation: false,
            IsSeasonal: true,
            HasPickpocketRisk: true)
    ];

    public static IReadOnlyList<CommunityEventDefinition> AllEvents => Events;

    public static CommunityEventDefinition? GetById(CommunityEventId id)
    {
        return Array.Find(Events, e => e.Id == id);
    }
}
