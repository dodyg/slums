using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
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
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Type.Should().Be(JobType.ClinicReception);
    }

    [Test]
    public void GetAvailableJobs_ShouldReturnWorkshopShift_ForWorkshop()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static location => location.Id == LocationId.Workshop);
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Type.Should().Be(JobType.WorkshopSewing);
    }

    [Test]
    public void GetAvailableJobs_ShouldReturnPharmacyShift_ForPharmacy()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static location => location.Id == LocationId.Pharmacy);
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Type.Should().Be(JobType.PharmacyStock);
    }

    [Test]
    public void CanPerformJob_ShouldRejectCafeService_OutsideCafe()
    {
        var service = new JobService();
        var player = new PlayerCharacter();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Market);
        var relationships = new RelationshipState();
        var progress = new JobProgressState();

        var canPerform = service.CanPerformJob(JobRegistry.CafeService, player, location, relationships, progress, currentDay: 1, out var reason);

        canPerform.Should().BeFalse();
        reason.Should().Contain("not available");
    }

    [Test]
    public void GetAvailableJobs_ShouldUpgradeClinicTrack_WhenSalmaTrustIsHigh()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Clinic);
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();
        relationships.SetNpcRelationship(NpcId.NurseSalma, 12, 1);

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Name.Should().Be("Clinic Intake Desk");
        jobs[0].BasePay.Should().BeGreaterThan(JobRegistry.ClinicReception.BasePay);
    }

    [Test]
    public void GetAvailableJobs_ShouldUpgradeWorkshopTrack_WhenReliabilityIsHigh()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Workshop);
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();
        progress.RestoreTrack(JobType.WorkshopSewing, reliability: 65, shiftsCompleted: 3, lockoutUntilDay: 0);

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Name.Should().Be("Workshop Finishing Table");
    }

    [Test]
    public void GetAvailableJobs_ShouldUpgradeCallCenterTrack_WhenReliabilityAndPersuasionAreHigh()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.CallCenter);
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();
        progress.RestoreTrack(JobType.CallCenterWork, reliability: 72, shiftsCompleted: 4, lockoutUntilDay: 0);
        player.Skills.SetLevel(SkillId.Persuasion, 2);

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Name.Should().Be("Call Center Retention Queue");
    }

    [Test]
    public void GetAvailableJobs_ShouldUpgradePharmacyTrack_WhenMedicalSkillIsHigh()
    {
        var service = new JobService();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.Pharmacy);
        var player = new PlayerCharacter();
        var relationships = new RelationshipState();
        var progress = new JobProgressState();
        player.Skills.SetLevel(SkillId.Medical, 2);

        var jobs = service.GetAvailableJobs(location, player, relationships, progress).ToList();

        jobs.Should().ContainSingle();
        jobs[0].Name.Should().Be("Pharmacy Restock Run");
    }

    [Test]
    public void CanPerformJob_ShouldRejectLockedOutTrack()
    {
        var service = new JobService();
        var player = new PlayerCharacter();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.CallCenter);
        var relationships = new RelationshipState();
        var progress = new JobProgressState();
        progress.RestoreTrack(JobType.CallCenterWork, reliability: 45, shiftsCompleted: 2, lockoutUntilDay: 3);

        var canPerform = service.CanPerformJob(JobRegistry.CallCenterWork, player, location, relationships, progress, currentDay: 2, out var reason);

        canPerform.Should().BeFalse();
        reason.Should().Contain("shut out");
    }

    [Test]
    public void PerformJob_ShouldApplyMistakePenalty_AndLockout_WhenCallCenterStressIsTooHigh()
    {
        var service = new JobService();
        var player = new PlayerCharacter();
        var location = WorldState.AllLocations.First(static current => current.Id == LocationId.CallCenter);
        var relationships = new RelationshipState();
        var progress = new JobProgressState();
        player.Stats.SetStress(65);

        var result = service.PerformJob(JobRegistry.CallCenterWork, player, location, relationships, progress, currentDay: 1);

        result.Success.Should().BeTrue();
        result.MistakeMade.Should().BeTrue();
        result.LockoutUntilDay.Should().Be(3);
        progress.GetTrack(JobType.CallCenterWork).LockoutUntilDay.Should().Be(3);
        progress.GetTrack(JobType.CallCenterWork).Reliability.Should().BeLessThan(50);
    }
}