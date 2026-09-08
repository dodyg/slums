using FluentAssertions;
using Slums.Core.Content;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Randomness;
using Slums.Core.State;
using Slums.Core.World;
using TUnit;

namespace Slums.Core.Tests.Content;

internal sealed class GameContentCatalogTests
{
    [Test]
    public void Constructor_ShouldSnapshotAndHideMutableInputCollections()
    {
        var events = new List<RandomEvent>
        {
            new("event", "Description", new RandomEventEffect(), 1, 1, null)
        };
        var conditions = new List<DistrictConditionDefinition>
        {
            new() { Id = "condition", District = DistrictId.Imbaba, Title = "Condition", Weight = 1, MinDay = 1 }
        };
        var schedules = new List<NpcScheduleDefinition>();
        var catalog = new GameContentCatalog([], [], [], events, conditions, schedules);

        events.Clear();
        conditions.Clear();

        catalog.RandomEvents.Should().ContainSingle();
        catalog.DistrictConditions.Should().ContainSingle();
        catalog.RandomEvents.Should().NotBeAssignableTo<RandomEvent[]>();
        catalog.DistrictConditions.Should().NotBeAssignableTo<DistrictConditionDefinition[]>();
    }

    [Test]
    public void GameSession_ShouldExposeNoScheduleAvailability_WhenCatalogHasNoSchedules()
    {
        var catalog = new GameContentCatalog([], [], [], [], [], []);
        var session = new GameSession(new GameRandom(1), catalog);

        session.HasConfiguredNpcSchedules.Should().BeFalse();
        session.GetNpcAvailability().Should().BeEmpty();
    }

    [Test]
    public void GameSessions_ShouldKeepJobDefinitionsFromTheirOwnCatalog()
    {
        var first = new GameSession(
            new GameRandom(1),
            new GameContentCatalog([], [], [new JobShift { Type = JobType.BakeryWork, Name = "First bakery", BasePay = 1, EnergyCost = 1, StressCost = 1, DurationMinutes = 1 }], [], [], []));
        var second = new GameSession(
            new GameRandom(2),
            new GameContentCatalog([], [], [new JobShift { Type = JobType.BakeryWork, Name = "Second bakery", BasePay = 1, EnergyCost = 1, StressCost = 1, DurationMinutes = 1 }], [], [], []));

        first.Jobs.PreviewJob(JobType.BakeryWork, first.Player, first.Relationships, first.JobProgress).Job.Name.Should().Be("First bakery");
        second.Jobs.PreviewJob(JobType.BakeryWork, second.Player, second.Relationships, second.JobProgress).Job.Name.Should().Be("Second bakery");
    }
}
