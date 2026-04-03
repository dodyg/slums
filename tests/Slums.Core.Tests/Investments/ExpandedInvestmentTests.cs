using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Investments;

internal sealed class ExpandedInvestmentTests
{
    [Test]
    public void Registry_ShouldContainAllNewInvestmentTypes()
    {
        InvestmentRegistry.GetByType(InvestmentType.TeaCart).Should().NotBeNull();
        InvestmentRegistry.GetByType(InvestmentType.PhoneChargingStation).Should().NotBeNull();
        InvestmentRegistry.GetByType(InvestmentType.HerbalRemedyTrade).Should().NotBeNull();
        InvestmentRegistry.GetByType(InvestmentType.SewingSideBusiness).Should().NotBeNull();
        InvestmentRegistry.GetByType(InvestmentType.CafeSupplyPartnership).Should().NotBeNull();
    }

    [Test]
    public void Registry_ShouldContainElevenInvestmentTypes()
    {
        InvestmentRegistry.AllDefinitions.Should().HaveCount(11);
    }

    [Test]
    public void TeaCart_ShouldHaveCorrectDefinition()
    {
        var def = InvestmentRegistry.GetByType(InvestmentType.TeaCart)!;

        def.Name.Should().Be("Tea Cart (Shay Cart)");
        def.Cost.Should().Be(100);
        def.WeeklyIncomeMin.Should().Be(5);
        def.WeeklyIncomeMax.Should().Be(8);
        def.RiskLabel.Should().Be("Low");
        def.OpportunityLocationId.Should().Be(LocationId.Home);
        def.OpportunityNpc.Should().Be(NpcId.NeighborMona);
        def.RequiredRelationshipNpc.Should().Be(NpcId.NeighborMona);
        def.RequiredRelationshipTrust.Should().Be(10);
        def.RequiresCrimePath.Should().BeFalse();
        def.RequiresStreetSmartsOrExPrisoner.Should().BeFalse();
        def.RequiredMedicalLevel.Should().BeNull();
        def.RequiredPhysicalLevel.Should().BeNull();
    }

    [Test]
    public void PhoneChargingStation_ShouldHaveCorrectDefinition()
    {
        var def = InvestmentRegistry.GetByType(InvestmentType.PhoneChargingStation)!;

        def.Name.Should().Be("Phone Charging Station");
        def.Cost.Should().Be(160);
        def.WeeklyIncomeMin.Should().Be(8);
        def.WeeklyIncomeMax.Should().Be(14);
        def.RiskLabel.Should().Be("Low-Medium");
        def.OpportunityLocationId.Should().Be(LocationId.Depot);
        def.OpportunityNpc.Should().Be(NpcId.DispatcherSafaa);
        def.RequiredRelationshipNpc.Should().Be(NpcId.DispatcherSafaa);
        def.RequiredRelationshipTrust.Should().Be(15);
        def.RequiredMedicalLevel.Should().BeNull();
        def.RequiredPhysicalLevel.Should().BeNull();
    }

    [Test]
    public void HerbalRemedyTrade_ShouldHaveCorrectDefinition()
    {
        var def = InvestmentRegistry.GetByType(InvestmentType.HerbalRemedyTrade)!;

        def.Name.Should().Be("Herbal Remedy Trade");
        def.Cost.Should().Be(180);
        def.WeeklyIncomeMin.Should().Be(10);
        def.WeeklyIncomeMax.Should().Be(16);
        def.RiskLabel.Should().Be("Medium");
        def.OpportunityLocationId.Should().Be(LocationId.Pharmacy);
        def.OpportunityNpc.Should().Be(NpcId.PharmacistMariam);
        def.RequiredRelationshipNpc.Should().Be(NpcId.PharmacistMariam);
        def.RequiredRelationshipTrust.Should().Be(15);
        def.RequiredMedicalLevel.Should().Be(2);
        def.RequiredPhysicalLevel.Should().BeNull();
    }

    [Test]
    public void SewingSideBusiness_ShouldHaveCorrectDefinition()
    {
        var def = InvestmentRegistry.GetByType(InvestmentType.SewingSideBusiness)!;

        def.Name.Should().Be("Sewing Side Business");
        def.Cost.Should().Be(220);
        def.WeeklyIncomeMin.Should().Be(14);
        def.WeeklyIncomeMax.Should().Be(20);
        def.RiskLabel.Should().Be("Medium");
        def.OpportunityLocationId.Should().Be(LocationId.Workshop);
        def.OpportunityNpc.Should().Be(NpcId.WorkshopBossAbuSamir);
        def.RequiredRelationshipNpc.Should().Be(NpcId.WorkshopBossAbuSamir);
        def.RequiredRelationshipTrust.Should().Be(20);
        def.RequiredMedicalLevel.Should().BeNull();
        def.RequiredPhysicalLevel.Should().Be(2);
    }

    [Test]
    public void CafeSupplyPartnership_ShouldHaveCorrectDefinition()
    {
        var def = InvestmentRegistry.GetByType(InvestmentType.CafeSupplyPartnership)!;

        def.Name.Should().Be("Cafe Supply Partnership");
        def.Cost.Should().Be(250);
        def.WeeklyIncomeMin.Should().Be(16);
        def.WeeklyIncomeMax.Should().Be(24);
        def.RiskLabel.Should().Be("Medium");
        def.OpportunityLocationId.Should().Be(LocationId.Cafe);
        def.OpportunityNpc.Should().Be(NpcId.CafeOwnerNadia);
        def.RequiredRelationshipNpc.Should().Be(NpcId.CafeOwnerNadia);
        def.RequiredRelationshipTrust.Should().Be(25);
        def.RequiresCrimePath.Should().BeFalse();
        def.RequiredMedicalLevel.Should().BeNull();
        def.RequiredPhysicalLevel.Should().BeNull();
    }

    [Test]
    public void TeaCart_ShouldBeEligible_WhenAtHomeWithMonaTrust10AndMoney()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(100);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 10, 1);

        var definition = InvestmentRegistry.GetByType(InvestmentType.TeaCart)!;
        var eligibility = gameState.CheckInvestmentEligibility(definition);

        eligibility.IsEligible.Should().BeTrue();
    }

    [Test]
    public void TeaCart_ShouldBeBlocked_WhenMonaTrustTooLow()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(100);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 5, 1);

        var definition = InvestmentRegistry.GetByType(InvestmentType.TeaCart)!;
        var eligibility = gameState.CheckInvestmentEligibility(definition);

        eligibility.IsEligible.Should().BeFalse();
        eligibility.FailureReasons.Should().Contain(r => r.Contains("10 trust with Mona", StringComparison.Ordinal));
    }

    [Test]
    public void TeaCart_ShouldBeAvailableAtHome()
    {
        using var gameState = new GameSession();

        var types = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToArray();

        types.Should().Contain(InvestmentType.TeaCart);
    }

    [Test]
    public void PhoneChargingStation_ShouldBeAvailableAtDepot()
    {
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Depot);

        var types = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToArray();

        types.Should().Contain(InvestmentType.PhoneChargingStation);
    }

    [Test]
    public void PhoneChargingStation_ShouldNotBeAvailableAtHome()
    {
        using var gameState = new GameSession();

        var types = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToArray();

        types.Should().NotContain(InvestmentType.PhoneChargingStation);
    }

    [Test]
    public void HerbalRemedyTrade_ShouldRequireMedicalSkill()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        gameState.World.TravelTo(LocationId.Pharmacy);
        gameState.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);

        var definition = InvestmentRegistry.GetByType(InvestmentType.HerbalRemedyTrade)!;

        var blocked = gameState.CheckInvestmentEligibility(definition);
        blocked.IsEligible.Should().BeFalse();
        blocked.FailureReasons.Should().Contain(r => r.Contains("medical knowledge", StringComparison.Ordinal));

        gameState.Player.Skills.SetLevel(SkillId.Medical, 2);

        var eligible = gameState.CheckInvestmentEligibility(definition);
        eligible.IsEligible.Should().BeTrue();
    }

    [Test]
    public void SewingSideBusiness_ShouldRequirePhysicalSkill()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(250);
        gameState.World.TravelTo(LocationId.Workshop);
        gameState.Relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 20, 1);

        var definition = InvestmentRegistry.GetByType(InvestmentType.SewingSideBusiness)!;

        var blocked = gameState.CheckInvestmentEligibility(definition);
        blocked.IsEligible.Should().BeFalse();
        blocked.FailureReasons.Should().Contain(r => r.Contains("physical capability", StringComparison.Ordinal));

        gameState.Player.Skills.SetLevel(SkillId.Physical, 2);

        var eligible = gameState.CheckInvestmentEligibility(definition);
        eligible.IsEligible.Should().BeTrue();
    }

    [Test]
    public void CafeSupplyPartnership_ShouldBeAvailableAtCafe()
    {
        using var gameState = new GameSession();
        gameState.World.TravelTo(LocationId.Cafe);

        var types = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToArray();

        types.Should().Contain(InvestmentType.CafeSupplyPartnership);
    }

    [Test]
    public void CafeSupplyPartnership_ShouldRequireNadiaTrust25()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(300);
        gameState.World.TravelTo(LocationId.Cafe);
        gameState.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 20, 1);

        var definition = InvestmentRegistry.GetByType(InvestmentType.CafeSupplyPartnership)!;
        var blocked = gameState.CheckInvestmentEligibility(definition);

        blocked.IsEligible.Should().BeFalse();
        blocked.FailureReasons.Should().Contain(r => r.Contains("25 trust with Nadia", StringComparison.Ordinal));

        gameState.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 25, 1);

        var eligible = gameState.CheckInvestmentEligibility(definition);
        eligible.IsEligible.Should().BeTrue();
    }

    [Test]
    public void MakeInvestment_ShouldSucceedForTeaCart()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(100);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 10, 1);

        var result = gameState.MakeInvestment(InvestmentType.TeaCart);

        result.Success.Should().BeTrue();
        result.AmountInvested.Should().Be(100);
        gameState.Player.Stats.Money.Should().Be(0);
        gameState.ActiveInvestments.Should().ContainSingle();
        gameState.ActiveInvestments[0].Type.Should().Be(InvestmentType.TeaCart);
    }

    [Test]
    public void MakeInvestment_ShouldSucceedForPhoneChargingStation()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        gameState.World.TravelTo(LocationId.Depot);
        gameState.Relationships.SetNpcRelationship(NpcId.DispatcherSafaa, 15, 1);

        var result = gameState.MakeInvestment(InvestmentType.PhoneChargingStation);

        result.Success.Should().BeTrue();
        result.AmountInvested.Should().Be(160);
        gameState.Player.Stats.Money.Should().Be(40);
        gameState.ActiveInvestments.Should().ContainSingle();
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldPayTeaCartIncome()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(100);
        gameState.Relationships.SetNpcRelationship(NpcId.NeighborMona, 10, 1);

        gameState.MakeInvestment(InvestmentType.TeaCart);

        var summary = gameState.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [6]));

        summary.TotalIncome.Should().Be(6);
        gameState.ActiveInvestments[0].WeeksActive.Should().Be(1);
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldPayHerbalRemedyIncome()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(200);
        gameState.Player.Skills.SetLevel(SkillId.Medical, 2);
        gameState.World.TravelTo(LocationId.Pharmacy);
        gameState.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);

        gameState.MakeInvestment(InvestmentType.HerbalRemedyTrade);

        var summary = gameState.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [13]));

        summary.TotalIncome.Should().Be(13);
        gameState.ActiveInvestments[0].WeeksActive.Should().Be(1);
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldPaySewingSideBusinessIncome()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(250);
        gameState.Player.Skills.SetLevel(SkillId.Physical, 2);
        gameState.World.TravelTo(LocationId.Workshop);
        gameState.Relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 20, 1);

        gameState.MakeInvestment(InvestmentType.SewingSideBusiness);

        var summary = gameState.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [17]));

        summary.TotalIncome.Should().Be(17);
    }

    [Test]
    public void ResolveWeeklyInvestments_ShouldPayCafeSupplyPartnershipIncome()
    {
        using var gameState = new GameSession();
        gameState.Player.Stats.SetMoney(300);
        gameState.World.TravelTo(LocationId.Cafe);
        gameState.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 25, 1);

        gameState.MakeInvestment(InvestmentType.CafeSupplyPartnership);

        var summary = gameState.ResolveWeeklyInvestments(
            new SequenceRandom(
                doubleValues: [0.99, 0.99, 0.99, 0.99],
                intValues: [20]));

        summary.TotalIncome.Should().Be(20);
    }

    [Test]
    public void NewInvestments_ShouldHaveCorrectRiskProfiles()
    {
        var teaCart = InvestmentRegistry.GetByType(InvestmentType.TeaCart)!.RiskProfile;
        var phoneCharging = InvestmentRegistry.GetByType(InvestmentType.PhoneChargingStation)!.RiskProfile;
        var herbal = InvestmentRegistry.GetByType(InvestmentType.HerbalRemedyTrade)!.RiskProfile;
        var sewing = InvestmentRegistry.GetByType(InvestmentType.SewingSideBusiness)!.RiskProfile;
        var cafe = InvestmentRegistry.GetByType(InvestmentType.CafeSupplyPartnership)!.RiskProfile;

        teaCart.WeeklyFailureChance.Should().Be(0.01);
        teaCart.ExtortionChance.Should().Be(0.0);
        teaCart.BetrayalChance.Should().Be(0.01);

        phoneCharging.WeeklyFailureChance.Should().Be(0.02);
        phoneCharging.ExtortionChance.Should().Be(0.02);
        phoneCharging.PoliceHeatChance.Should().Be(0.01);

        herbal.WeeklyFailureChance.Should().Be(0.03);
        herbal.ExtortionChance.Should().Be(0.03);
        herbal.PoliceHeatChance.Should().Be(0.03);

        sewing.WeeklyFailureChance.Should().Be(0.03);
        sewing.ExtortionChance.Should().Be(0.03);
        sewing.ExtortionAmountMin.Should().Be(10);
        sewing.ExtortionAmountMax.Should().Be(18);

        cafe.WeeklyFailureChance.Should().Be(0.03);
        cafe.ExtortionChance.Should().Be(0.04);
        cafe.ExtortionAmountMin.Should().Be(12);
        cafe.ExtortionAmountMax.Should().Be(20);
    }

    [Test]
    public void EligibilityEvaluator_ShouldRejectMedicalGate_WhenSkillTooLow()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);
        var definition = InvestmentRegistry.GetByType(InvestmentType.HerbalRemedyTrade)!;

        var context = new InvestmentEligibilityContext(
            CurrentMoney: 200,
            CurrentLocationId: LocationId.Pharmacy,
            ReachableNpcs: new HashSet<NpcId> { NpcId.PharmacistMariam },
            OwnedInvestmentTypes: new HashSet<InvestmentType>(),
            Relationships: relationships,
            TotalCrimeEarnings: 0,
            StreetSmartsLevel: 0,
            MedicalLevel: 1,
            PhysicalLevel: 0,
            BackgroundType: BackgroundType.MedicalSchoolDropout);

        var eligibility = InvestmentEligibilityEvaluator.Evaluate(definition, context);

        eligibility.IsEligible.Should().BeFalse();
        eligibility.FailureReasons.Should().Contain(r => r.Contains("medical knowledge", StringComparison.Ordinal));
    }

    [Test]
    public void EligibilityEvaluator_ShouldAcceptMedicalGate_WhenSkillMet()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, 1);
        var definition = InvestmentRegistry.GetByType(InvestmentType.HerbalRemedyTrade)!;

        var context = new InvestmentEligibilityContext(
            CurrentMoney: 200,
            CurrentLocationId: LocationId.Pharmacy,
            ReachableNpcs: new HashSet<NpcId> { NpcId.PharmacistMariam },
            OwnedInvestmentTypes: new HashSet<InvestmentType>(),
            Relationships: relationships,
            TotalCrimeEarnings: 0,
            StreetSmartsLevel: 0,
            MedicalLevel: 2,
            PhysicalLevel: 0,
            BackgroundType: BackgroundType.MedicalSchoolDropout);

        var eligibility = InvestmentEligibilityEvaluator.Evaluate(definition, context);

        eligibility.IsEligible.Should().BeTrue();
    }

    [Test]
    public void EligibilityEvaluator_ShouldRejectPhysicalGate_WhenSkillTooLow()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 20, 1);
        var definition = InvestmentRegistry.GetByType(InvestmentType.SewingSideBusiness)!;

        var context = new InvestmentEligibilityContext(
            CurrentMoney: 250,
            CurrentLocationId: LocationId.Workshop,
            ReachableNpcs: new HashSet<NpcId> { NpcId.WorkshopBossAbuSamir },
            OwnedInvestmentTypes: new HashSet<InvestmentType>(),
            Relationships: relationships,
            TotalCrimeEarnings: 0,
            StreetSmartsLevel: 0,
            MedicalLevel: 0,
            PhysicalLevel: 1,
            BackgroundType: BackgroundType.MedicalSchoolDropout);

        var eligibility = InvestmentEligibilityEvaluator.Evaluate(definition, context);

        eligibility.IsEligible.Should().BeFalse();
        eligibility.FailureReasons.Should().Contain(r => r.Contains("physical capability", StringComparison.Ordinal));
    }

    [Test]
    public void InvestmentsAtNewLocations_ShouldNotAppearAtWrongLocations()
    {
        using var gameState = new GameSession();

        var homeTypes = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToHashSet();
        homeTypes.Should().NotContain(InvestmentType.PhoneChargingStation);
        homeTypes.Should().NotContain(InvestmentType.HerbalRemedyTrade);
        homeTypes.Should().NotContain(InvestmentType.SewingSideBusiness);
        homeTypes.Should().NotContain(InvestmentType.CafeSupplyPartnership);

        gameState.World.TravelTo(LocationId.Depot);
        var depotTypes = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToHashSet();
        depotTypes.Should().Contain(InvestmentType.PhoneChargingStation);
        depotTypes.Should().NotContain(InvestmentType.TeaCart);
        depotTypes.Should().NotContain(InvestmentType.HerbalRemedyTrade);

        gameState.World.TravelTo(LocationId.Workshop);
        var workshopTypes = gameState.GetCurrentInvestmentOpportunities().Select(static d => d.Type).ToHashSet();
        workshopTypes.Should().Contain(InvestmentType.SewingSideBusiness);
        workshopTypes.Should().NotContain(InvestmentType.TeaCart);
        workshopTypes.Should().NotContain(InvestmentType.CafeSupplyPartnership);
    }
}
