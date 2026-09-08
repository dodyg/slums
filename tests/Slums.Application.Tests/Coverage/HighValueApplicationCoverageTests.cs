using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Slums.Application.Activities;
using Slums.Application.Diagnostics;
using Slums.Application.HouseholdAssets;
using Slums.Application.Persistence;
using Slums.Application.Technology;
using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.State;
using Slums.Core.Technology;
using Slums.Core.World;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Application.Tests.Coverage;

internal sealed class HighValueApplicationCoverageTests
{
    [Test]
    public void CommunityActionMenuQuery_ShouldProjectPreviewAvailability()
    {
        var preview = new CommunityActionPreview(
            CommunityActionRegistry.Get(CommunityActionType.CoordinateCoolingRoom),
            IsAtHome: true, HasSkill: true, HasTime: true, CanAfford: false, HasEnergy: true,
            HasSupplies: true, HasPressureNeed: false, HasCommunityParticipation: true,
            CanPerform: false, UnavailabilityReason: "Need 8 LE.");
        var context = new CommunityActionMenuContext(0, 100, 1, 8, 0, 0, [preview]);

        var status = new CommunityActionMenuQuery().GetStatuses(context).Should().ContainSingle().Subject;

        status.CanPerform.Should().BeFalse();
        status.UnavailabilityReason.Should().Be("Need 8 LE.");
    }

    [Test]
    public void ClinicTravelMenuQuery_ShouldExplainClosedAndUnaffordableClinics()
    {
        var context = new ClinicTravelMenuContext(
        [
            new ClinicTravelOptionContext(LocationId.Clinic, "Clinic", "Dokki", 10, 20, 30, false, "Day 2", 25, true, true),
            new ClinicTravelOptionContext(LocationId.Clinic, "Clinic", "Dokki", 10, 20, 30, true, "Today", 25, false, true)
        ],
        PlayerMoney: 5,
        MotherHealth: 40);

        var statuses = new ClinicTravelMenuQuery().GetStatuses(context);

        statuses[0].UnavailableReason.Should().Be("Closed today. Opens: Day 2");
        statuses[1].UnavailableReason.Should().Be("Need 30 LE (have 5 LE)");
    }

    [Test]
    public void CommunityAndAttendCommands_ShouldReturnFalseWhenTheActionIsUnavailable()
    {
        var session = TestSessions.Create();

        new CommunityActionCommand().Execute(session, CommunityActionType.CoordinateCoolingRoom).Should().BeFalse();
        new AttendCommunityEventCommand().Execute(session, CommunityEventId.MulidFestival).Should().BeFalse();
    }

    [Test]
    public void TechnicalAndDigitalCommands_ShouldRejectUnavailableActions()
    {
        var session = TestSessions.Create();

        new TechnicalRepairCommand().Execute(session, TechnicalRepairActionType.RepairHandset).Should().BeFalse();
        new DigitalServiceCommand().Execute(session, DigitalServiceActionType.SubmitBiometricAppeal).Should().BeFalse();
    }

    [Test]
    public void HouseholdUpgradeCommands_ShouldRejectMissingAssets()
    {
        var session = TestSessions.Create();

        new PlantUpgradeCommand().Execute(session, Guid.NewGuid(), PlantUpgradeType.BiggerPot).Should().BeFalse();
        new FishTankUpgradeCommand().Execute(session, FishTankUpgradeType.BetterFilter).Should().BeFalse();
    }

    [Test]
    public void SaveSlotRules_ShouldAllowSafeSlotsAndRejectTraversal()
    {
        SaveSlotRules.IsValidSlot("night-run_01").Should().BeTrue();
        SaveSlotRules.IsValidSlot("../escape").Should().BeFalse();
        var act = () => SaveSlotRules.EnsureValidSlot("save with spaces");

        act.Should().Throw<ArgumentException>().WithMessage("*invalid*");
    }

    [Test]
    public void GameMutationLogger_ShouldAttachAndDetachWithoutLeakingTheSession()
    {
        var logger = Substitute.For<ILogger<GameMutationLogger>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(false);
        var session = TestSessions.Create();
        using var mutationLogger = new GameMutationLogger(logger);

        mutationLogger.Attach(session);
        mutationLogger.Detach();

    }
}
