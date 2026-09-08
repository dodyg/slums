using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Economy;
using Slums.Core.Heat;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Weather;
using Slums.Core.World;
using Slums.Core.World.News;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Coverage;

[NotInParallel]
internal sealed class HighValueCoreCoverageTests
{
    [Test]
    public void SkillService_ShouldAdvanceSkillLevel()
    {
        var session = TestSessions.Create();

        SkillService.ApplySkillGain(SkillId.Medical, session, out var newLevel).Should().BeTrue();

        newLevel.Should().Be(1);
        session.Player.Skills.GetLevel(SkillId.Medical).Should().Be(1);
    }

    [Test]
    public void DebtService_ShouldRejectAnUnaffordableRepayment()
    {
        var session = TestSessions.Create();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        DebtService.BorrowFromNpc(
            NpcId.NeighborMona, 30, session.Clock.Day, session.Player, session.Relationships,
            session.NpcEconomies, session.PlayerDebts).Success.Should().BeTrue();
        session.Player.Stats.SetMoney(0);

        var result = DebtService.Repay(
            DebtSource.NeighborLoan, 30, session.Player, session.PlayerDebts,
            session.Relationships, session.DistrictHeat, session.World.CurrentDistrict);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Not enough money.");
    }

    [Test]
    public void NewsImpactCalculator_ShouldSumOnlyMatchingActiveEffects()
    {
        var definition = new NewsFlashDefinition
        {
            Id = "coverage_news",
            DurationDays = 2,
            Effects =
            [
                new NewsEffectDefinition { Type = NewsEffectType.FoodPriceModifier, Amount = 4, District = DistrictId.Imbaba },
                new NewsEffectDefinition { Type = NewsEffectType.FoodPriceModifier, Amount = 7, District = DistrictId.Dokki }
            ]
        };
        var state = new NewsState();
        state.Activate(definition, 1);

        NewsImpactCalculator.GetFoodPriceModifier(state, DistrictId.Imbaba, [definition]).Should().Be(4);
        NewsImpactCalculator.GetFoodPriceModifier(state, DistrictId.Dokki, [definition]).Should().Be(7);
    }

    [Test]
    public void WeatherActivityRules_ShouldBlockOutdoorWorkAndFloodTravel()
    {
        var rain = new WeatherState(WeatherType.Rain, 0, 0, 0, 0, 0, 0, true, false, true);

        WeatherActivityRules.BlocksJob(rain, JobType.StreetVending).Should().BeTrue();
        WeatherActivityRules.BlocksJob(rain, JobType.ClinicReception).Should().BeFalse();
        WeatherActivityRules.BlocksTravelTo(rain, DistrictId.Dokki).Should().BeTrue();
        WeatherActivityRules.GetTravelBlockReason(rain, DistrictId.Dokki).Should().Contain("Flooded");
    }

    [Test]
    public void HeatBleedOverTable_ShouldContainSymmetricDistrictRoutes()
    {
        HeatBleedOverTable.Relationships.Should().Contain((DistrictId.Imbaba, DistrictId.Dokki, 0.05));
        HeatBleedOverTable.Relationships.Should().Contain((DistrictId.Dokki, DistrictId.Imbaba, 0.05));
    }

    [Test]
    public void CommunityActionRegistry_ShouldExposeEveryActionWithPositiveCosts()
    {
        CommunityActionRegistry.All.Should().HaveCount(Enum.GetValues<CommunityActionType>().Length);
        CommunityActionRegistry.All.Should().OnlyContain(action => action.TimeCostMinutes > 0 && action.MoneyCost > 0 && action.EnergyCost > 0);
        CommunityActionRegistry.Get(CommunityActionType.OrganizeWaterRationing).Name.Should().Be("Organize Water Rationing");
    }

    [Test]
    public void RobotRegistry_ShouldExposeConfiguredRepairDefinitions()
    {
        TestContent.Catalog.Robots.Should().NotBeEmpty();
        TestContent.Catalog.Robots.Should().OnlyContain(robot => robot.PurchaseCost > robot.RepairCost && robot.RepairCondition > 0 && robot.RepairCondition <= 100);
        TestContent.Catalog.GetRobot(RobotType.RepairDrone).Name.Should().Be("Repair Drone");
    }

    [Test]
    public void NarrativeEntryKnotCatalog_ShouldClassifyAuthoredEntryConventions()
    {
        NarrativeEntryKnotCatalog.IsPlayerVisible("event_heat_warning").Should().BeTrue();
        NarrativeEntryKnotCatalog.IsPlayerVisible("recurring_conversation_12").Should().BeTrue();
        NarrativeEntryKnotCatalog.GetUnclassified(["global decl", "event_heat_warning", "internal_helper"]).Should().ContainSingle().Which.Should().Be("internal_helper");
    }
}
