using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Investments;

public static class InvestmentRegistry
{
    private static readonly InvestmentDefinition[] DefaultDefinitions =
    [
        new InvestmentDefinition
        {
            Type = InvestmentType.FoulCart,
            Name = "Foul Cart Partnership",
            Description = "Partner with a local foul cart for weekly returns",
            RiskLabel = "Low",
            Cost = 150,
            WeeklyIncomeMin = 8,
            WeeklyIncomeMax = 12,
            RiskProfile = CreateFoulCartRiskProfile(),
            RequiredRelationshipNpc = NpcId.LandlordHajjMahmoud,
            RequiredRelationshipTrust = 30,
            OpportunityNpc = NpcId.LandlordHajjMahmoud,
            OpportunityLocationId = LocationId.Home
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.MicroLaundry,
            Name = "Micro-Laundry Service",
            Description = "Invest in a small neighborhood laundry operation",
            RiskLabel = "Low",
            Cost = 200,
            WeeklyIncomeMin = 10,
            WeeklyIncomeMax = 15,
            RiskProfile = CreateMicroLaundryRiskProfile(),
            RequiredRelationshipTrust = 0,
            OpportunityNpc = NpcId.LaundryOwnerIman,
            OpportunityLocationId = LocationId.Laundry
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.ScrapCollection,
            Name = "Scrap Collection Crew",
            Description = "Back a crew collecting and sorting scrap metal",
            RiskLabel = "Medium",
            Cost = 180,
            WeeklyIncomeMin = 15,
            WeeklyIncomeMax = 22,
            RiskProfile = CreateScrapCollectionRiskProfile(),
            RequiresStreetSmartsOrExPrisoner = true,
            OpportunityNpc = NpcId.FixerUmmKarim,
            OpportunityLocationId = LocationId.Market
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.Kiosk,
            Name = "Kiosk Share (Koshk)",
            Description = "Buy into a neighborhood kiosk operation",
            RiskLabel = "Medium",
            Cost = 250,
            WeeklyIncomeMin = 18,
            WeeklyIncomeMax = 25,
            RiskProfile = CreateKioskRiskProfile(),
            RequiredRelationshipNpc = NpcId.FixerUmmKarim,
            RequiredRelationshipTrust = 40,
            OpportunityNpc = NpcId.FixerUmmKarim,
            OpportunityLocationId = LocationId.Market
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.MarketStall,
            Name = "Informal Market Stall",
            Description = "Stake a share in Dokki's informal market",
            RiskLabel = "Medium-High",
            Cost = 220,
            WeeklyIncomeMin = 20,
            WeeklyIncomeMax = 30,
            RiskProfile = CreateMarketStallRiskProfile(),
            RequiredRelationshipNpc = NpcId.RunnerYoussef,
            RequiredRelationshipTrust = 25,
            OpportunityNpc = NpcId.RunnerYoussef,
            OpportunityLocationId = LocationId.Square
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.HashishCourier,
            Name = "Hashish Courier Stake",
            Description = "Silent stake in a courier operation - high risk, high return",
            RiskLabel = "High",
            Cost = 300,
            WeeklyIncomeMin = 35,
            WeeklyIncomeMax = 50,
            RiskProfile = CreateHashishCourierRiskProfile(),
            RequiredRelationshipNpc = NpcId.FenceHanan,
            RequiredRelationshipTrust = 50,
            RequiresCrimePath = true,
            OpportunityNpc = NpcId.FenceHanan,
            OpportunityLocationId = LocationId.Market
        }
    ];

    private static IReadOnlyList<InvestmentDefinition> _definitions = DefaultDefinitions;

    public static IReadOnlyList<InvestmentDefinition> AllDefinitions => _definitions;

    public static void Configure(IEnumerable<InvestmentDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var configuredDefinitions = definitions.Where(static definition => definition is not null).ToArray();
        if (configuredDefinitions.Length > 0)
        {
            _definitions = configuredDefinitions;
        }
    }

    private static InvestmentRiskProfile CreateFoulCartRiskProfile() => new()
    {
        WeeklyFailureChance = 0.01,
        ExtortionChance = 0.0,
        PoliceHeatChance = 0.0,
        BetrayalChance = 0.02,
        ExtortionAmountMin = 0,
        ExtortionAmountMax = 0
    };

    private static InvestmentRiskProfile CreateMicroLaundryRiskProfile() => new()
    {
        WeeklyFailureChance = 0.02,
        ExtortionChance = 0.0,
        PoliceHeatChance = 0.0,
        BetrayalChance = 0.03,
        ExtortionAmountMin = 0,
        ExtortionAmountMax = 0
    };

    private static InvestmentRiskProfile CreateScrapCollectionRiskProfile() => new()
    {
        WeeklyFailureChance = 0.04,
        ExtortionChance = 0.03,
        PoliceHeatChance = 0.02,
        BetrayalChance = 0.05,
        ExtortionAmountMin = 8,
        ExtortionAmountMax = 15
    };

    private static InvestmentRiskProfile CreateKioskRiskProfile() => new()
    {
        WeeklyFailureChance = 0.03,
        ExtortionChance = 0.04,
        PoliceHeatChance = 0.03,
        BetrayalChance = 0.04,
        ExtortionAmountMin = 10,
        ExtortionAmountMax = 18
    };

    private static InvestmentRiskProfile CreateMarketStallRiskProfile() => new()
    {
        WeeklyFailureChance = 0.05,
        ExtortionChance = 0.06,
        PoliceHeatChance = 0.05,
        BetrayalChance = 0.05,
        ExtortionAmountMin = 12,
        ExtortionAmountMax = 22
    };

    private static InvestmentRiskProfile CreateHashishCourierRiskProfile() => new()
    {
        WeeklyFailureChance = 0.08,
        ExtortionChance = 0.10,
        PoliceHeatChance = 0.12,
        BetrayalChance = 0.08,
        ExtortionAmountMin = 18,
        ExtortionAmountMax = 35
    };

    public static InvestmentDefinition? GetByType(InvestmentType type)
    {
        return _definitions.FirstOrDefault(d => d.Type == type);
    }
}
