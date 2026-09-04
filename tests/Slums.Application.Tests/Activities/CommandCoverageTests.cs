using FluentAssertions;
using Slums.Application.Activities;
using Slums.Application.Endings;
using Slums.Application.Inventory;
using Slums.Application.News;
using Slums.Application.Technology;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Endings;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.World;
using TUnit;

namespace Slums.Application.Tests.Activities;

internal sealed class CommandCoverageTests
{
    [Test]
    public void AdvanceTimeCommand_CrossingCurfew_EndsAtHomeOnNextDay()
    {
        var session = new GameSession();
        session.Clock.SetTime(1, 21, 30);

        new AdvanceTimeCommand().Execute(session, 60);

        session.Clock.Day.Should().Be(2);
        session.World.CurrentLocationId.Should().Be(LocationId.Home);
    }

    [Test]
    public void ClinicTravelCommand_VisitsAnOpenClinicAndImprovesMotherHealth()
    {
        var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        session.Player.Stats.SetMoney(500);
        session.Player.Household.SetMotherHealth(50);

        var result = new ClinicTravelCommand().Execute(session, LocationId.Clinic);

        result.Success.Should().BeTrue();
        result.HealthChange.Should().BePositive();
    }

    [Test]
    public void TechnologyObligationCommand_RecordsHandsetExposure()
    {
        var session = new GameSession();

        TechnologyObligationCommand.Execute(session, TechnologyObligationAction.RecordHandsetUse).Should().BeTrue();

        session.Technology.HandsetDataExposure.Should().Be(1);
    }

    [Test]
    public void WorkCommand_PerformsTheSuppliedShift()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Bakery);
        session.Player.Stats.SetEnergy(100);

        var result = new WorkCommand().Execute(session, JobRegistry.BakeryWork, new Random(7));

        result.Should().NotBeNull();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void CrimeCommand_ReturnsTheRouteOutcome()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Square);
        session.Player.Stats.SetEnergy(100);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 20, 0, 1, 0, 5);

        var result = new CrimeCommand().Execute(session, attempt, new Random(7));

        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void EndingChoiceCommand_WhenNoEndingIsAvailable_ReturnsFalse()
    {
        EndingChoiceCommand.Execute(new GameSession(), EndingId.StabilityHonestWork).Should().BeFalse();
    }

    [Test]
    public void AcknowledgeNewsCommand_RejectsAnInactiveFlash()
    {
        var result = new AcknowledgeNewsCommand().Execute(new GameSession(), "missing-news");

        result.Success.Should().BeFalse();
    }

    [Test]
    public void AcquireItemCommand_RejectsAnUnknownCatalogItem()
    {
        var result = new AcquireItemCommand().Execute(new GameSession(), "missing-item");

        result.Success.Should().BeFalse();
    }
}
