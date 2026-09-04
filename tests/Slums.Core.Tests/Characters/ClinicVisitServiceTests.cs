using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class ClinicVisitServiceTests
{
    [Test]
    public void CheckOnMother_ShouldUseTheClinicServiceMutationBoundary()
    {
        var session = new GameSession();

        ClinicVisitService.CheckOnMother(session);

        session.Player.Household.CheckedOnMotherToday.Should().BeTrue();
        session.Mutations[^1].Action.Should().Be("CheckOnMother");
    }

    [Test]
    public void GetClinicTravelOption_ShouldDescribeNonClinicLocationsAsInvalid()
    {
        var session = new GameSession();

        var option = ClinicVisitService.GetClinicTravelOption(session, LocationId.Market);

        option.IsValidOption.Should().BeFalse();
        option.OpenDaysSummary.Should().Be("No clinic at this location");
    }

    [Test]
    public void TravelAndTakeMotherToClinic_ShouldComposeTravelAndVisit()
    {
        var session = new GameSession();
        session.Player.Household.SetMotherHealth(50);

        var result = ClinicVisitService.TravelAndTakeMotherToClinic(session, LocationId.Clinic);

        result.Success.Should().BeTrue();
        session.World.CurrentLocationId.Should().Be(LocationId.Clinic);
        session.Player.Household.MotherHealth.Should().BeGreaterThan(50);
        session.Mutations[^1].Action.Should().Be("TravelAndTakeMotherToClinic");
    }
}
