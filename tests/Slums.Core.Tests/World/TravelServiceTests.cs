using FluentAssertions;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.World;

internal sealed class TravelServiceTests
{
    [Test]
    public void TryWalkTo_ShouldKeepMoneyAndRecordTravelMutation()
    {
        var session = new GameSession();
        var moneyBefore = session.Player.Stats.Money;

        TravelService.TryWalkTo(session, LocationId.Market).Should().BeTrue();

        session.World.CurrentLocationId.Should().Be(LocationId.Market);
        session.Player.Stats.Money.Should().Be(moneyBefore);
        session.Mutations[^1].Action.Should().Be("TryWalkTo");
    }

    [Test]
    public void TryTravelTo_ShouldRejectTheCurrentLocationWithoutChangingMoney()
    {
        var session = new GameSession();
        var moneyBefore = session.Player.Stats.Money;

        TravelService.TryTravelTo(session, LocationId.Home).Should().BeFalse();

        session.Player.Stats.Money.Should().Be(moneyBefore);
        session.EventJournal.Entries[^1].Message.Should().Be("You are already at Your Apartment.");
    }

    [Test]
    public void CanAfford_ShouldReturnFalseForAnUnknownLocation()
    {
        var session = new GameSession();

        var unknownLocation = new LocationId("unknown");
        TravelService.CanAfford(session, unknownLocation).Should().BeFalse();
        TravelService.GetTravelCost(session, unknownLocation).Should().Be(0);
    }
}
