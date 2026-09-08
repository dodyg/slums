using FluentAssertions;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using Slums.TestSupport;

namespace Slums.Core.Tests.Expenses;

/// <summary>
/// Regression guard for the shared price pipeline: the price a screen previews must equal the
/// price the action charges, for every purchase context.
/// </summary>
internal sealed class PriceModifierPipelineTests
{
    [Test]
    public async Task Food_Preview_ShouldEqualCommittedPrice()
    {
        var session = TestSessions.Create(new Random(7));
        session.Player.Stats.SetMoney(10_000);

        var preview = session.GetFoodCost();
        var before = session.Player.Stats.Money;

        session.BuyFood().Should().BeTrue();

        (before - session.Player.Stats.Money).Should().Be(preview);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    [Test]
    public async Task StreetFood_Preview_ShouldEqualCommittedPrice()
    {
        var session = TestSessions.Create(new Random(7));
        session.Player.Stats.SetMoney(10_000);

        var preview = session.GetStreetFoodCost();
        var before = session.Player.Stats.Money;

        session.EatStreetFood().Should().BeTrue();

        (before - session.Player.Stats.Money).Should().Be(preview);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    [Test]
    public async Task Clinic_Preview_ShouldEqualCommittedPrice()
    {
        var session = TestSessions.Create(new Random(7));
        session.World.TravelTo(LocationId.Clinic);
        session.Player.Stats.SetMoney(10_000);
        session.Player.Household.SetMotherHealth(40);
        session.Player.Household.AddMedicine(5);

        var preview = session.GetClinicTravelOption(session.World.CurrentLocationId).ClinicCost;
        var before = session.Player.Stats.Money;

        var result = session.TakeMotherToClinic();

        result.Success.Should().BeTrue();
        (before - session.Player.Stats.Money).Should().Be(preview);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    [Test]
    public async Task Travel_Preview_ShouldEqualCommittedPrice()
    {
        var session = TestSessions.Create(new Random(7));
        session.Player.Stats.SetMoney(10_000);
        var destinationId = session.World.GetTravelableLocations().First().Id;
        var destination = session.World.GetLocationById(destinationId)!;

        var preview = session.GetTravelCost(destinationId);
        var before = session.Player.Stats.Money;

        session.TryTravelTo(destinationId).Should().BeTrue();

        (before - session.Player.Stats.Money).Should().Be(preview);
        destination.District.Should().Be(session.World.CurrentDistrict);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    [Test]
    public async Task TravelCost_ShouldRespectOneLeFloor_WhenModifiersWouldMakeItFree()
    {
        var session = TestSessions.Create(new Random(7));
        var destinationId = session.World.GetTravelableLocations().First().Id;
        var destination = session.World.GetLocationById(destinationId)!;

        session.GetTravelCost(destinationId).Should().BeGreaterThanOrEqualTo(1);
        session.GetTravelTimeMinutes(destinationId).Should().BeGreaterThanOrEqualTo(1);
        session.GetWalkTimeMinutes(destinationId).Should().BeGreaterThanOrEqualTo(1);
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
