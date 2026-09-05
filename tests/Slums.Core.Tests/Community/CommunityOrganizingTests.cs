using FluentAssertions;
using Slums.Core.Community;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Community;

internal sealed class CommunityOrganizingTests
{
    [Test]
    public void Preview_ShouldExplainTheFirstGate()
    {
        var session = new GameSession();

        var preview = session.PreviewCommunityAction(CommunityActionType.CoordinateCoolingRoom);

        preview.CanPerform.Should().BeFalse();
        preview.UnavailabilityReason.Should().Be("Reach Community Organizing 4.");
    }

    [Test]
    public void WaterRationing_ShouldConsumeSuppliesAndShortenWaterDisruption()
    {
        var session = CreateParticipatingSession(4);
        session.Infrastructure.StartDisruption(DistrictId.Imbaba, InfrastructureServiceType.Water, InfrastructureSeverity.Disrupted, 3, 1, "pump-failure");
        var beforeMoney = session.Player.Stats.Money;
        var beforeEnergy = session.Player.Stats.Energy;
        var beforeFood = session.Player.Household.FoodStockpile;
        var preview = session.PreviewCommunityAction(CommunityActionType.OrganizeWaterRationing);

        var result = session.PerformCommunityAction(CommunityActionType.OrganizeWaterRationing);

        result.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(beforeMoney - preview.Action.MoneyCost);
        session.Player.Stats.Energy.Should().Be(beforeEnergy - preview.Action.EnergyCost);
        session.Player.Household.FoodStockpile.Should().Be(beforeFood - 1);
        session.Infrastructure.Get(DistrictId.Imbaba, InfrastructureServiceType.Water).RemainingDays.Should().Be(2);
        session.CommunityAdaptation.WaterReserveUnits.Should().Be(2);
        session.Mutations[^1].Category.Should().Be("Community");
    }

    [Test]
    public void CoolingRoom_ShouldCreateBoundedGroupBenefit()
    {
        var session = CreateParticipatingSession(4);
        var result = session.PerformCommunityAction(CommunityActionType.CoordinateCoolingRoom);

        result.Should().BeTrue();
        session.CommunityAdaptation.CoolingRoomDaysRemaining.Should().Be(2);
        session.CommunityAdaptation.SuccessfulActions.Should().Be(1);
        session.CommunityAdaptation.ShelterContributions.Should().Be(1);
    }

    [Test]
    public void PressureResponse_ShouldNeedMoreParticipationAndReduceTensionOnlyModestly()
    {
        var session = CreateParticipatingSession(8);
        session.EventAttendance.TotalAttended = 2;
        session.Territory.ModifyTension(DistrictId.Imbaba, 30);
        var beforeTension = session.Territory.GetControl(DistrictId.Imbaba).Tension;

        var result = session.PerformCommunityAction(CommunityActionType.NeighborhoodPressureResponse);

        result.Should().BeTrue();
        session.Territory.GetControl(DistrictId.Imbaba).Tension.Should().Be(beforeTension - 5);
        session.CommunityAdaptation.ShelterContributions.Should().Be(2);
        session.EventJournal.Entries.Should().Contain(entry => entry.Message.Contains("factions still own", StringComparison.Ordinal));
    }

    [Test]
    public void PressureResponse_ShouldRemainUnavailableWhenTheNeighborhoodIsCalm()
    {
        var session = CreateParticipatingSession(8);
        session.EventAttendance.TotalAttended = 2;

        var preview = session.PreviewCommunityAction(CommunityActionType.NeighborhoodPressureResponse);

        preview.CanPerform.Should().BeFalse();
        preview.UnavailabilityReason.Should().Contain("elevated territory pressure");
    }

    private static GameSession CreateParticipatingSession(int skillLevel)
    {
        var session = new GameSession();
        session.Player.Skills.SetLevel(SkillId.CommunityOrganizing, skillLevel);
        session.Player.Stats.SetMoney(100);
        session.Player.Stats.SetEnergy(100);
        session.Clock.SetTime(1, 18, 0);
        session.EventAttendance.TotalAttended = 1;
        return session;
    }
}
