using FluentAssertions;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Jobs;

internal sealed class WorkSessionServiceTests
{
    [Test]
    public void GetAvailable_ShouldUseTheSessionLocationAndSchedule()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Bakery);

        var jobs = WorkSessionService.GetAvailable(session);

        jobs.Should().ContainSingle(job => job.Type == JobType.BakeryWork);
    }

    [Test]
    public void Work_ShouldRecordTheWorkMutationThroughTheSession()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Bakery);
        var job = WorkSessionService.GetAvailable(session).Single(static candidate => candidate.Type == JobType.BakeryWork);

        var result = WorkSessionService.Work(session, job, new Random(42));

        result.Success.Should().BeTrue();
        session.Mutations[^1].Action.Should().Be("WorkJob");
    }
}
