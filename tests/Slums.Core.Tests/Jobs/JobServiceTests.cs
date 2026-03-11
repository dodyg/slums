using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Jobs;

internal sealed class JobServiceTests
{
    [Test]
    public void GetAvailableJobs_ShouldReturnClinicReception_ForClinic()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static location => location.Id == LocationId.Clinic);

        var jobs = service.GetAvailableJobs(location).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Type.Should().Be(JobType.ClinicReception);
    }

    [Test]
    public void GetAvailableJobs_ShouldReturnWorkshopShift_ForWorkshop()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static location => location.Id == LocationId.Workshop);

        var jobs = service.GetAvailableJobs(location).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Type.Should().Be(JobType.WorkshopSewing);
    }

    [Test]
    public void CanPerformJob_ShouldRejectCafeService_OutsideCafe()
    {
        var service = new JobService();
        var player = new PlayerCharacter();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);

        var canPerform = service.CanPerformJob(JobRegistry.CafeService, player, location, out var reason);

        canPerform.Should().BeFalse();
        reason.Should().Contain("not available");
    }
}