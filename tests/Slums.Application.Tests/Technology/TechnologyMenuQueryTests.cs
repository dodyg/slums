using FluentAssertions;
using Slums.Application.Technology;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Technology;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Technology;

internal sealed class TechnologyMenuQueryTests
{
    [Test]
    public void TechnicalRepairMenu_ShouldExplainSkillGateAtHome()
    {
        var session = CreateHomeSession();
        var context = TechnicalRepairMenuContext.Create(session);
        var statuses = new TechnicalRepairMenuQuery().GetStatuses(context);

        statuses.Should().Contain(status => status.Preview.Action.Type == TechnicalRepairActionType.RepairHandset);
        statuses.Single(status => status.Preview.Action.Type == TechnicalRepairActionType.RepairHandset)
            .UnavailabilityReason.Should().Be("Reach Technical Repair 4.");
    }

    [Test]
    public void TechnicalRepairMenu_ShouldShowWorkshopActionsWithPartsAndIncomePreview()
    {
        var session = CreateHomeSession();
        session.World.TravelTo(LocationId.Workshop);
        session.Player.Skills.SetLevel(SkillId.RobotRepair, 8);
        session.Player.Robotics.AddParts(2);
        var context = TechnicalRepairMenuContext.Create(session);

        var statuses = new TechnicalRepairMenuQuery().GetStatuses(context);

        statuses.Should().Contain(status => status.Preview.Action.Type == TechnicalRepairActionType.RestoreSolarStorage);
        statuses.Single(status => status.Preview.Action.Type == TechnicalRepairActionType.TakeRepairBenchContract)
            .Preview.Income.Should().Be(35);
    }

    [Test]
    public void DigitalServiceMenu_ShouldExplainChanceAndPendingObligation()
    {
        var session = CreateHomeSession();
        session.Player.Skills.SetLevel(SkillId.CyberHacking, 6);
        var context = DigitalServiceMenuContext.Create(session);

        var status = new DigitalServiceMenuQuery().GetStatuses(context).Single();

        status.CanPerform.Should().BeTrue();
        status.Preview.SuccessChance.Should().Be(60);
        status.Preview.CreatesObligation.Should().BeTrue();
    }

    private static GameSession CreateHomeSession()
    {
        var session = new GameSession();
        session.Player.Stats.SetMoney(100);
        session.Player.Stats.SetEnergy(100);
        session.World.TravelTo(LocationId.Home);
        session.Clock.SetTime(1, 18, 0);
        return session;
    }
}
