using FluentAssertions;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Technology;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Technology;

internal sealed class TechnicalRepairServiceTests
{
    [Test]
    public void HandsetRepair_ShouldShowPartsCostTimeConditionAndSkillBenefit()
    {
        var session = CreateSession(4, 1, LocationId.Home);

        var preview = session.PreviewTechnicalRepair(TechnicalRepairActionType.RepairHandset);

        preview.CanPerform.Should().BeTrue();
        preview.CurrentCondition.Should().Be(65);
        preview.ConditionGain.Should().Be(25);
        preview.Action.PartsRequired.Should().Be(1);
        preview.Action.MoneyCost.Should().Be(6);
        preview.Action.TimeCostMinutes.Should().Be(90);
    }

    [Test]
    public void HandsetRepair_ShouldConsumePartMoneyEnergyAndTime()
    {
        var session = CreateSession(4, 1, LocationId.Home);
        var beforeMoney = session.Player.Stats.Money;
        var beforeEnergy = session.Player.Stats.Energy;

        var result = session.PerformTechnicalRepair(TechnicalRepairActionType.RepairHandset);

        result.Should().BeTrue();
        session.Phone.HandsetCondition.Should().Be(90);
        session.Player.Robotics.Parts.Should().Be(0);
        session.Player.Stats.Money.Should().Be(beforeMoney - 6);
        session.Player.Stats.Energy.Should().Be(beforeEnergy - 8);
        session.Clock.Minute.Should().Be(30);
        session.Mutations[^1].Category.Should().Be("Technology");
    }

    [Test]
    public void SolarStorageRepair_ShouldImproveLocalServiceAndPreserveScarcity()
    {
        var session = CreateSession(6, 2, LocationId.Workshop);
        session.Infrastructure.StartDisruption(DistrictId.ArdAlLiwa, InfrastructureServiceType.Electricity, InfrastructureSeverity.Disrupted, 3, 1, "storage-failure");

        var result = session.PerformTechnicalRepair(TechnicalRepairActionType.RestoreSolarStorage);

        result.Should().BeTrue();
        session.Technology.MicrogridStorageCondition.Should().Be(85);
        session.Player.Robotics.Parts.Should().Be(0);
        session.Infrastructure.Get(DistrictId.ArdAlLiwa, InfrastructureServiceType.Electricity).RemainingDays.Should().Be(2);
    }

    [Test]
    public void RepairBenchContract_ShouldCreatePaidTechnicalWorkAtHighSkill()
    {
        var session = CreateSession(8, 2, LocationId.Workshop);
        var beforeMoney = session.Player.Stats.Money;

        var preview = session.PreviewTechnicalRepair(TechnicalRepairActionType.TakeRepairBenchContract);
        var result = session.PerformTechnicalRepair(TechnicalRepairActionType.TakeRepairBenchContract);

        preview.Income.Should().Be(35);
        result.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(beforeMoney + 35);
        session.Player.Robotics.Parts.Should().Be(0);
    }

    [Test]
    public void RepairPreview_ShouldExplainTheSkillGate()
    {
        var session = CreateSession(0, 1, LocationId.Home);

        var preview = session.PreviewTechnicalRepair(TechnicalRepairActionType.RepairHandset);

        preview.CanPerform.Should().BeFalse();
        preview.UnavailabilityReason.Should().Be("Reach Technical Repair 4.");
    }

    private static GameSession CreateSession(int skill, int parts, LocationId location)
    {
        var session = new GameSession();
        session.Player.Skills.SetLevel(SkillId.RobotRepair, skill);
        session.Player.Stats.SetMoney(100);
        session.Player.Stats.SetEnergy(100);
        session.Player.Robotics.AddParts(parts);
        session.World.TravelTo(location);
        session.Clock.SetTime(1, 18, 0);
        return session;
    }
}
