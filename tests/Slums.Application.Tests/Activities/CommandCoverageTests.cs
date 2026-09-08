using FluentAssertions;
using Slums.Application.Activities;
using Slums.Application.Endings;
using Slums.Application.Inventory;
using Slums.Application.News;
using Slums.Application.Technology;
using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Endings;
using Slums.Core.Entertainment;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.Training;
using Slums.Core.World;
using TUnit;
using Slums.TestSupport;

namespace Slums.Application.Tests.Activities;

internal sealed class CommandCoverageTests
{
    [Test]
    public void AdvanceTimeCommand_CrossingCurfew_EndsAtHomeOnNextDay()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(1, 21, 30);

        new AdvanceTimeCommand().Execute(session, 60);

        session.Clock.Day.Should().Be(2);
        session.World.CurrentLocationId.Should().Be(LocationId.Home);
    }

    [Test]
    public void ClinicTravelCommand_VisitsAnOpenClinicAndImprovesMotherHealth()
    {
        var session = TestSessions.Create();
        session.Player.ApplyBackground(TestContent.Catalog.GetBackground(BackgroundType.SudaneseRefugee));
        session.Player.Stats.SetMoney(500);
        session.Player.Household.SetMotherHealth(50);

        var result = new ClinicTravelCommand().Execute(session, LocationId.Clinic);

        result.Success.Should().BeTrue();
        result.HealthChange.Should().BePositive();
    }

    [Test]
    public void TechnologyObligationCommand_RecordsHandsetExposure()
    {
        var session = TestSessions.Create();

        TechnologyObligationCommand.Execute(session, TechnologyObligationAction.RecordHandsetUse).Should().BeTrue();

        session.Technology.HandsetDataExposure.Should().Be(1);
    }

    [Test]
    public void WorkCommand_PerformsTheSuppliedShift()
    {
        var session = TestSessions.Create();
        session.World.TravelTo(LocationId.Bakery);
        session.Player.Stats.SetEnergy(100);

        var result = new WorkCommand().Execute(session, TestContent.Catalog.GetJob(JobType.BakeryWork), new Random(7));

        result.Should().NotBeNull();
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void CrimeCommand_ReturnsTheRouteOutcome()
    {
        var session = TestSessions.Create();
        session.World.TravelTo(LocationId.Square);
        session.Player.Stats.SetEnergy(100);
        var attempt = new CrimeAttempt(CrimeType.PettyTheft, 20, 0, 1, 0, 5);

        var result = new CrimeCommand().Execute(session, attempt, new Random(7));

        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void TrainingCommand_PerformsAnAvailableEveningActivity()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(1, 18, 0);
        session.Player.Stats.SetEnergy(100);
        var activity = TrainingRegistry.AllActivities.Single(static candidate => candidate.Type == TrainingActivityType.RooftopExercise);

        new TrainingCommand().Execute(session, activity).Should().BeTrue();
    }

    [Test]
    public void EntertainmentCommand_PerformsAnActivityAtTheCurrentCafe()
    {
        var session = TestSessions.Create();
        session.World.TravelTo(LocationId.Cafe);
        session.Player.Stats.SetMoney(100);
        var activity = session.GetAvailableEntertainmentActivities().First(static candidate => candidate.Type == EntertainmentActivityType.Coffee);

        new EntertainmentCommand().Execute(session, activity).Should().BeTrue();
    }

    [Test]
    public void EndingChoiceCommand_WhenNoEndingIsAvailable_ReturnsFalse()
    {
        EndingChoiceCommand.Execute(TestSessions.Create(), EndingId.StabilityHonestWork).Should().BeFalse();
    }

    [Test]
    public void AcknowledgeNewsCommand_RejectsAnInactiveFlash()
    {
        var result = new AcknowledgeNewsCommand().Execute(TestSessions.Create(), "missing-news");

        result.Success.Should().BeFalse();
    }

    [Test]
    public void AcquireItemCommand_RejectsAnUnknownCatalogItem()
    {
        var result = new AcquireItemCommand().Execute(TestSessions.Create(), "missing-item");

        result.Success.Should().BeFalse();
    }
}
