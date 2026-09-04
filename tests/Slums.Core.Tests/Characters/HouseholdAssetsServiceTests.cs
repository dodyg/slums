using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Robotics;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class HouseholdAssetsServiceTests
{
    [Test]
    public void BuyPlant_ShouldApplyPurchaseThroughTheSessionBoundary()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.PlantShop);
        var moneyBefore = session.Player.Stats.Money;
        var definition = PlantRegistry.GetByType(PlantType.Basil);

        HouseholdAssetsService.BuyPlant(session, PlantType.Basil).Should().BeTrue();

        session.Player.HouseholdAssets.Plants.Should().ContainSingle();
        session.Player.Stats.Money.Should().Be(moneyBefore - definition.OneTimeCost);
        session.Mutations[^1].Action.Should().Be("BuyPlant");
    }

    [Test]
    public void BuyRobot_ShouldRejectPurchaseOutsideTheWorkshop()
    {
        var session = new GameSession();

        HouseholdAssetsService.BuyRobot(session, RobotType.RepairDrone).Should().BeFalse();

        session.Mutations[^1].Category.Should().Be("GuardRejected");
        session.EventJournal.Entries[^1].Message.Should().Be("Abu Samir only sells machines from the workshop bench.");
    }

    [Test]
    public void ResolveWeekly_ShouldApplyNeglectPenaltyToTheExistingSessionState()
    {
        var session = new GameSession();
        session.Clock.SetTime(8, 12, 0);
        var assets = session.Player.HouseholdAssets;
        var stressBefore = session.Player.Stats.Stress;
        HouseholdAssetsService.Restore(
            session,
            [OwnedPet.Restore(PetType.Cat, session.CurrentDay, 0, false, false, 0, 0)],
            [],
            false,
            0,
            0,
            null,
            0);

        HouseholdAssetsService.ResolveWeekly(session);

        session.Player.HouseholdAssets.Should().BeSameAs(assets);
        session.Player.Stats.Stress.Should().Be(stressBefore + 2);
        session.EventJournal.Entries[^1].Message.Should().Be("[Day 8] Skipping household care all week weighs on your mother. Stress +2.");
    }
}
