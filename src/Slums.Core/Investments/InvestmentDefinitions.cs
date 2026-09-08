using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Investments;

/// <summary>Code-owned default investment definitions used when a catalog is built without JSON content.</summary>
public static class InvestmentDefinitions
{
    /// <summary>Gets the default investment definitions.</summary>
    public static IReadOnlyList<InvestmentDefinition> Defaults { get; } =
    [
        new InvestmentDefinition
        {
            Type = InvestmentType.FoulCart,
            Name = "Foul Cart Partnership",
            Description = "Partner with a local foul cart for weekly returns",
            RiskLabel = "Low",
            Cost = 150,
            WeeklyIncomeMin = 16,
            WeeklyIncomeMax = 22,
            PerkType = InvestmentPerkType.FoodStaplesDiscount,
            PerkDescription = "Food staples cost 1 LE less while this stake is active.",
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
            WeeklyIncomeMin = 22,
            WeeklyIncomeMax = 30,
            PerkType = InvestmentPerkType.LaundryOwnerTrust,
            PerkDescription = "Laundry Owner Iman gains 1 trust each week.",
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
            WeeklyIncomeMin = 26,
            WeeklyIncomeMax = 34,
            PerkType = InvestmentPerkType.SparePartChance,
            PerkDescription = "25% weekly chance to find one spare robot part.",
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
            WeeklyIncomeMin = 32,
            WeeklyIncomeMax = 42,
            PerkType = InvestmentPerkType.PhoneCreditDiscount,
            PerkDescription = "Phone credit refills cost 2 LE less.",
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
            WeeklyIncomeMin = 32,
            WeeklyIncomeMax = 44,
            PerkType = InvestmentPerkType.DistrictFoodDiscount,
            PerkDescription = "Food staples cost 1 LE less in the purchase district.",
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
            WeeklyIncomeMin = 48,
            WeeklyIncomeMax = 64,
            PerkType = InvestmentPerkType.None,
            PerkDescription = "No additional perk; the return carries the risk.",
            RiskProfile = CreateHashishCourierRiskProfile(),
            RequiredRelationshipNpc = NpcId.FenceHanan,
            RequiredRelationshipTrust = 50,
            RequiresCrimePath = true,
            OpportunityNpc = NpcId.FenceHanan,
            OpportunityLocationId = LocationId.Market
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.TeaCart,
            Name = "Tea Cart (Shay Cart)",
            Description = "Set up a small tea cart in the building entrance for neighbors and passersby",
            RiskLabel = "Low",
            Cost = 100,
            WeeklyIncomeMin = 13,
            WeeklyIncomeMax = 18,
            PerkType = InvestmentPerkType.TeaCircleInvitation,
            PerkDescription = "A rooftop tea-circle invitation arrives one day sooner.",
            RiskProfile = CreateTeaCartRiskProfile(),
            RequiredRelationshipNpc = NpcId.NeighborMona,
            RequiredRelationshipTrust = 10,
            OpportunityNpc = NpcId.NeighborMona,
            OpportunityLocationId = LocationId.Home
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.PhoneChargingStation,
            Name = "Phone Charging Station",
            Description = "Sell phone charges and power-bank refills to taxi operators and depot travelers",
            RiskLabel = "Low-Medium",
            Cost = 160,
            WeeklyIncomeMin = 20,
            WeeklyIncomeMax = 27,
            PerkType = InvestmentPerkType.PhoneCreditDiscount,
            PerkDescription = "Phone credit refills cost 2 LE less.",
            RiskProfile = CreatePhoneChargingRiskProfile(),
            RequiredRelationshipNpc = NpcId.DispatcherSafaa,
            RequiredRelationshipTrust = 15,
            OpportunityNpc = NpcId.DispatcherSafaa,
            OpportunityLocationId = LocationId.Depot
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.HerbalRemedyTrade,
            Name = "Herbal Remedy Trade",
            Description = "Prepare and sell traditional herbal remedies from the pharmacy counter",
            RiskLabel = "Medium",
            Cost = 180,
            WeeklyIncomeMin = 24,
            WeeklyIncomeMax = 32,
            PerkType = InvestmentPerkType.MedicineDiscount,
            PerkDescription = "Medicine purchases cost 5 LE less.",
            RiskProfile = CreateHerbalRemedyRiskProfile(),
            RequiredRelationshipNpc = NpcId.PharmacistMariam,
            RequiredRelationshipTrust = 15,
            RequiredMedicalLevel = 2,
            OpportunityNpc = NpcId.PharmacistMariam,
            OpportunityLocationId = LocationId.Pharmacy
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.SewingSideBusiness,
            Name = "Sewing Side Business",
            Description = "Use the workshop after hours for private tailoring and mending jobs",
            RiskLabel = "Medium",
            Cost = 220,
            WeeklyIncomeMin = 30,
            WeeklyIncomeMax = 40,
            PerkType = InvestmentPerkType.SewingPayBonus,
            PerkDescription = "Workshop sewing shifts pay 2 LE more.",
            RiskProfile = CreateSewingSideBusinessRiskProfile(),
            RequiredRelationshipNpc = NpcId.WorkshopBossAbuSamir,
            RequiredRelationshipTrust = 20,
            RequiredPhysicalLevel = 2,
            OpportunityNpc = NpcId.WorkshopBossAbuSamir,
            OpportunityLocationId = LocationId.Workshop
        },
        new InvestmentDefinition
        {
            Type = InvestmentType.CafeSupplyPartnership,
            Name = "Cafe Supply Partnership",
            Description = "Invest in the cafe's supply chain for a share of weekly profits",
            RiskLabel = "Medium",
            Cost = 250,
            WeeklyIncomeMin = 34,
            WeeklyIncomeMax = 45,
            PerkType = InvestmentPerkType.CafeStressRelief,
            PerkDescription = "Cafe entertainment gives 2 additional stress relief.",
            RiskProfile = CreateCafeSupplyRiskProfile(),
            RequiredRelationshipNpc = NpcId.CafeOwnerNadia,
            RequiredRelationshipTrust = 25,
            OpportunityNpc = NpcId.CafeOwnerNadia,
            OpportunityLocationId = LocationId.Cafe
        }
    ];

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

    private static InvestmentRiskProfile CreateTeaCartRiskProfile() => new()
    {
        WeeklyFailureChance = 0.01,
        ExtortionChance = 0.0,
        PoliceHeatChance = 0.0,
        BetrayalChance = 0.01,
        ExtortionAmountMin = 0,
        ExtortionAmountMax = 0
    };

    private static InvestmentRiskProfile CreatePhoneChargingRiskProfile() => new()
    {
        WeeklyFailureChance = 0.02,
        ExtortionChance = 0.02,
        PoliceHeatChance = 0.01,
        BetrayalChance = 0.02,
        ExtortionAmountMin = 5,
        ExtortionAmountMax = 10
    };

    private static InvestmentRiskProfile CreateHerbalRemedyRiskProfile() => new()
    {
        WeeklyFailureChance = 0.03,
        ExtortionChance = 0.03,
        PoliceHeatChance = 0.03,
        BetrayalChance = 0.04,
        ExtortionAmountMin = 8,
        ExtortionAmountMax = 16
    };

    private static InvestmentRiskProfile CreateSewingSideBusinessRiskProfile() => new()
    {
        WeeklyFailureChance = 0.03,
        ExtortionChance = 0.03,
        PoliceHeatChance = 0.02,
        BetrayalChance = 0.03,
        ExtortionAmountMin = 10,
        ExtortionAmountMax = 18
    };

    private static InvestmentRiskProfile CreateCafeSupplyRiskProfile() => new()
    {
        WeeklyFailureChance = 0.03,
        ExtortionChance = 0.04,
        PoliceHeatChance = 0.02,
        BetrayalChance = 0.03,
        ExtortionAmountMin = 12,
        ExtortionAmountMax = 20
    };
}
