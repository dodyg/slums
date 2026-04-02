using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class ActivityLedgerSystemTests
{
    private static JobShift MakeShift(JobType type) =>
        new() { Type = type, BasePay = 20, EnergyCost = 10, StressCost = 5, DurationMinutes = 60, MinEnergyRequired = 5 };

    [Test]
    public async Task RecordWorkShift_IncrementsEarningsOnSuccess()
    {
        var workState = new GameWorkState();
        var clock = new GameClock();
        var job = MakeShift(JobType.BakeryWork);
        var result = new JobResult { Success = true, MoneyEarned = 25 };

        ActivityLedgerSystem.RecordWorkShift(workState, clock, job, result);

        await Assert.That(workState.TotalHonestWorkEarnings).IsEqualTo(25);
        await Assert.That(workState.HonestShiftsCompleted).IsEqualTo(1);
        await Assert.That(workState.LastHonestWorkDay).IsEqualTo(clock.Day);
    }

    [Test]
    public async Task RecordWorkShift_DoesNothingOnFailure()
    {
        var workState = new GameWorkState();
        var clock = new GameClock();
        var job = MakeShift(JobType.BakeryWork);
        var result = new JobResult { Success = false, MoneyEarned = 0 };

        ActivityLedgerSystem.RecordWorkShift(workState, clock, job, result);

        await Assert.That(workState.TotalHonestWorkEarnings).IsEqualTo(0);
        await Assert.That(workState.HonestShiftsCompleted).IsEqualTo(0);
    }

    [Test]
    public async Task RecordWorkShift_TracksPublicFacingWorkDay()
    {
        var workState = new GameWorkState();
        var clock = new GameClock();
        clock.SetTime(5, 10, 0);
        var job = MakeShift(JobType.CallCenterWork);
        var result = new JobResult { Success = true, MoneyEarned = 30 };

        ActivityLedgerSystem.RecordWorkShift(workState, clock, job, result);

        await Assert.That(workState.LastPublicFacingWorkDay).IsEqualTo(5);
    }

    [Test]
    public async Task RecordWorkShift_DoesNotTrackPublicFacingForPrivateJob()
    {
        var workState = new GameWorkState();
        var clock = new GameClock();
        clock.SetTime(5, 10, 0);
        var job = MakeShift(JobType.BakeryWork);
        var result = new JobResult { Success = true, MoneyEarned = 20 };

        ActivityLedgerSystem.RecordWorkShift(workState, clock, job, result);

        await Assert.That(workState.LastPublicFacingWorkDay).IsEqualTo(0);
    }

    [Test]
    public async Task RecordCrimeOutcome_TracksBasicState()
    {
        var crimeState = new GameCrimeState();
        var clock = new GameClock();
        clock.SetTime(7, 14, 0);
        var result = new CrimeResult { Success = true, MoneyEarned = 50 };

        ActivityLedgerSystem.RecordCrimeOutcome(crimeState, clock, result);

        await Assert.That(crimeState.CrimeCommittedToday).IsTrue();
        await Assert.That(crimeState.LastCrimeDay).IsEqualTo(7);
        await Assert.That(crimeState.TotalCrimeEarnings).IsEqualTo(50);
        await Assert.That(crimeState.CrimesCommitted).IsEqualTo(1);
    }

    [Test]
    public async Task RecordCrimeOutcome_FailureStillTracksDay()
    {
        var crimeState = new GameCrimeState();
        var clock = new GameClock();
        clock.SetTime(3, 14, 0);
        var result = new CrimeResult { Success = false, MoneyEarned = 0 };

        ActivityLedgerSystem.RecordCrimeOutcome(crimeState, clock, result);

        await Assert.That(crimeState.CrimeCommittedToday).IsTrue();
        await Assert.That(crimeState.LastCrimeDay).IsEqualTo(3);
        await Assert.That(crimeState.TotalCrimeEarnings).IsEqualTo(0);
        await Assert.That(crimeState.CrimesCommitted).IsEqualTo(0);
    }

    [Test]
    public async Task GetDailyPolicePressureDecay_Returns2ForPrisoner()
    {
        var decay = ActivityLedgerSystem.GetDailyPolicePressureDecay(BackgroundType.ReleasedPoliticalPrisoner);
        await Assert.That(decay).IsEqualTo(2);
    }

    [Test]
    public async Task GetDailyPolicePressureDecay_Returns5ForDefault()
    {
        var decay = ActivityLedgerSystem.GetDailyPolicePressureDecay(BackgroundType.MedicalSchoolDropout);
        await Assert.That(decay).IsEqualTo(5);
        var decay2 = ActivityLedgerSystem.GetDailyPolicePressureDecay(BackgroundType.SudaneseRefugee);
        await Assert.That(decay2).IsEqualTo(5);
    }

    [Test]
    public async Task BeginNewDay_ResetsCrimeFlag()
    {
        var crimeState = new GameCrimeState();
        var clock = new GameClock();
        var result = new CrimeResult { Success = true, MoneyEarned = 50 };
        ActivityLedgerSystem.RecordCrimeOutcome(crimeState, clock, result);
        await Assert.That(crimeState.CrimeCommittedToday).IsTrue();

        ActivityLedgerSystem.BeginNewDay(crimeState);

        await Assert.That(crimeState.CrimeCommittedToday).IsFalse();
    }

    [Test]
    public async Task IsPublicFacingJob_ReturnsCorrectValues()
    {
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.CallCenterWork)).IsTrue();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.ClinicReception)).IsTrue();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.CafeService)).IsTrue();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.PharmacyStock)).IsTrue();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.MicrobusDispatch)).IsTrue();

        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.BakeryWork)).IsFalse();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.HouseCleaning)).IsFalse();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.WorkshopSewing)).IsFalse();
        await Assert.That(ActivityLedgerSystem.IsPublicFacingJob(JobType.LaundryPressing)).IsFalse();
    }
}
