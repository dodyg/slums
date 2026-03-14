using Slums.Core.Characters;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Core.Jobs;

public sealed class JobService
{
#pragma warning disable CA1822
    public JobPreview PreviewJob(JobType jobType, PlayerCharacter player, RelationshipState relationshipState, JobProgressState jobProgressState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relationshipState);
        ArgumentNullException.ThrowIfNull(jobProgressState);

        var resolvedJob = ResolveShift(jobType, player, relationshipState, jobProgressState);
        var track = jobProgressState.GetTrack(jobType);
        var activeModifiers = GetActiveModifiers(resolvedJob, player);
        var riskWarning = ShouldApplyMistake(resolvedJob, player)
            ? GetRiskWarning(resolvedJob, player)
            : null;

        return new JobPreview(
            resolvedJob,
            GetVariantReason(jobType, player, relationshipState, track),
            GetNextUnlockHint(jobType, player, relationshipState, track),
            activeModifiers,
            riskWarning);
    }

    public JobResult PerformJob(JobShift job, PlayerCharacter player, Location currentLocation, RelationshipState relationshipState, JobProgressState jobProgressState, int currentDay, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(currentLocation);
        ArgumentNullException.ThrowIfNull(relationshipState);
        ArgumentNullException.ThrowIfNull(jobProgressState);
        random ??= new Random();

        if (!CanPerformJob(job, player, currentLocation, relationshipState, jobProgressState, currentDay, out var reason))
        {
            return JobResult.Failed(reason);
        }

        var resolvedJob = job;

        if (ShouldApplyMistake(resolvedJob, player))
        {
            return PerformMistakeShift(resolvedJob, player, jobProgressState, currentDay, random);
        }

        var pay = resolvedJob.CalculatePay(random);
        var energyCost = resolvedJob.EnergyCost;
        if (player.Skills.GetLevel(SkillId.Physical) >= 3 &&
            (resolvedJob.Type == JobType.BakeryWork || resolvedJob.Type == JobType.HouseCleaning || resolvedJob.Type == JobType.WorkshopSewing))
        {
            energyCost = Math.Max(0, energyCost - 5);
        }

        player.Stats.ModifyMoney(pay);
        player.Stats.ModifyEnergy(-energyCost);
        player.Stats.ModifyStress(resolvedJob.StressCost);

        var reliabilityGain = GetReliabilityGain(resolvedJob.Type, jobProgressState.GetTrack(resolvedJob.Type));
        jobProgressState.RecordSuccessfulShift(resolvedJob.Type, reliabilityGain);

        return JobResult.SuccessWork(
            pay,
            energyCost,
            resolvedJob.StressCost,
            $"Worked {resolvedJob.Name}. Earned {pay} LE. Reliability +{reliabilityGain}. (-{energyCost} Energy, +{resolvedJob.StressCost} Stress)",
            reliabilityGain);
    }

#pragma warning disable CA1822 // Methods don't access instance data but this is a service pattern
    public bool CanPerformJob(JobShift job, PlayerCharacter player, Location location, RelationshipState relationshipState, JobProgressState jobProgressState, int currentDay, out string reason)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(relationshipState);
        ArgumentNullException.ThrowIfNull(jobProgressState);

        var resolvedJob = ResolveShift(job.Type, player, relationshipState, jobProgressState);
        var track = jobProgressState.GetTrack(job.Type);

        if (!location.HasJobOpportunities)
        {
            reason = "No work available at this location.";
            return false;
        }

        if (track.IsLockedOut(currentDay))
        {
            reason = $"You are shut out of {resolvedJob.Name} until day {track.LockoutUntilDay + 1} after the last mistake.";
            return false;
        }

        if (player.Stats.Energy < resolvedJob.MinEnergyRequired)
        {
            reason = $"Too tired. Need at least {resolvedJob.MinEnergyRequired} Energy (you have {player.Stats.Energy}).";
            return false;
        }

        if (!IsJobAvailableAtLocation(resolvedJob.Type, location.Id))
        {
            reason = $"{resolvedJob.Name} is not available at {location.Name}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

#pragma warning disable CA1822
    public IEnumerable<JobShift> GetAvailableJobs(Location location, PlayerCharacter player, RelationshipState relationshipState, JobProgressState jobProgressState)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relationshipState);
        ArgumentNullException.ThrowIfNull(jobProgressState);

        if (!location.HasJobOpportunities)
        {
            return [];
        }

        return location.Id switch
        {
            _ when location.Id == LocationId.Bakery => [ResolveShift(JobType.BakeryWork, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Market => [ResolveShift(JobType.HouseCleaning, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.CallCenter => [ResolveShift(JobType.CallCenterWork, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Clinic => [ResolveShift(JobType.ClinicReception, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Workshop => [ResolveShift(JobType.WorkshopSewing, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Cafe => [ResolveShift(JobType.CafeService, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Pharmacy => [ResolveShift(JobType.PharmacyStock, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Depot => [ResolveShift(JobType.MicrobusDispatch, player, relationshipState, jobProgressState)],
            _ when location.Id == LocationId.Laundry => [ResolveShift(JobType.LaundryPressing, player, relationshipState, jobProgressState)],
            _ => []
        };
    }

    private static JobResult PerformMistakeShift(JobShift job, PlayerCharacter player, JobProgressState jobProgressState, int currentDay, Random random)
    {
#pragma warning disable CA5394 // Random is sufficient for gameplay mechanics
        var reducedPay = Math.Max(0, (job.BasePay / 2) + random.Next(0, Math.Max(2, job.PayVariance)));
#pragma warning restore CA5394
        var stressCost = job.StressCost + GetMistakeStressPenalty(job.Type);
        var lockoutDays = GetLockoutDays(job.Type);
        var lockoutUntilDay = lockoutDays > 0 ? currentDay + lockoutDays : 0;
        var reliabilityLoss = GetMistakeReliabilityLoss(job.Type);

        player.Stats.ModifyMoney(reducedPay);
        player.Stats.ModifyEnergy(-job.EnergyCost);
        player.Stats.ModifyStress(stressCost);
        jobProgressState.RecordMistake(job.Type, reliabilityLoss, lockoutUntilDay);

        var lockoutText = lockoutDays > 0
            ? $" The employer shuts you out until day {lockoutUntilDay + 1}."
            : string.Empty;

        return JobResult.SuccessWork(
            reducedPay,
            job.EnergyCost,
            stressCost,
            $"{job.Name} goes badly. You only keep {reducedPay} LE and your reliability drops by {-reliabilityLoss}.{lockoutText}",
            reliabilityLoss,
            lockoutUntilDay,
            mistakeMade: true);
    }

    private static JobShift ResolveShift(JobType jobType, PlayerCharacter player, RelationshipState relationshipState, JobProgressState jobProgressState)
    {
        var track = jobProgressState.GetTrack(jobType);

        return jobType switch
        {
            JobType.BakeryWork => ResolveBakeryShift(player, track),
            JobType.HouseCleaning => ResolveHouseCleaningShift(track),
            JobType.CallCenterWork => ResolveCallCenterShift(player, track),
            JobType.ClinicReception => ResolveClinicShift(player, relationshipState, track),
            JobType.WorkshopSewing => ResolveWorkshopShift(relationshipState, track),
            JobType.CafeService => ResolveCafeShift(player, relationshipState, track),
            JobType.PharmacyStock => ResolvePharmacyShift(player, relationshipState, track),
            JobType.MicrobusDispatch => ResolveDepotShift(player, relationshipState, track),
            JobType.LaundryPressing => ResolveLaundryShift(relationshipState, track),
            _ => JobRegistry.GetJobByType(jobType) ?? throw new ArgumentOutOfRangeException(nameof(jobType))
        };
    }

    private static JobShift ResolveBakeryShift(PlayerCharacter player, JobTrackProgress track)
    {
        var baseShift = JobRegistry.BakeryWork;
        if (track.Reliability >= 75 && player.Skills.GetLevel(SkillId.Physical) >= 3)
        {
            return CreateShiftVariant(baseShift, "Bakery Dough Prep", "Start before dawn shaping and loading trays for the first rush.", 9, 5, 2, 30, 5);
        }

        if (track.Reliability >= 55 || player.Skills.GetLevel(SkillId.Physical) >= 2)
        {
            return CreateShiftVariant(baseShift, "Bakery Oven Shift", "Take the hotter early run near the ovens for steadier pay.", 5, 3, 1, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveHouseCleaningShift(JobTrackProgress track)
    {
        var baseShift = JobRegistry.HouseCleaning;
        if (track.Reliability >= 80)
        {
            return CreateShiftVariant(baseShift, "Full Apartment Cleaning", "A full-day flat cleaning for repeat clients who pay a little more and judge every detail.", 7, 4, 2, 60, 5);
        }

        if (track.Reliability >= 60)
        {
            return CreateShiftVariant(baseShift, "Regular Client Cleaning", "A steadier home cleaning route from households willing to call you back.", 4, 0, -1, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveCallCenterShift(PlayerCharacter player, JobTrackProgress track)
    {
        var baseShift = JobRegistry.CallCenterWork;
        if (track.Reliability >= 70 && player.Skills.GetLevel(SkillId.Persuasion) >= 2)
        {
            return CreateShiftVariant(baseShift, "Call Center Retention Queue", "Handle angry callers and the harder scripts that pay a little better.", 10, 0, 6, 0, 0);
        }

        if (track.Reliability >= 55)
        {
            return CreateShiftVariant(baseShift, "Call Center Follow-Up Shift", "Take the longer callbacks and survey queue when supervisors stop hovering.", 5, 0, 3, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveClinicShift(PlayerCharacter player, RelationshipState relationshipState, JobTrackProgress track)
    {
        var baseShift = JobRegistry.ClinicReception;
        var salmaTrust = relationshipState.GetNpcRelationship(NpcId.NurseSalma).Trust;
        if (salmaTrust >= 20 && track.Reliability >= 70)
        {
            var payDelta = player.BackgroundType == BackgroundType.MedicalSchoolDropout ? 13 : 11;
            var stressDelta = player.BackgroundType == BackgroundType.MedicalSchoolDropout ? -5 : -3;
            return CreateShiftVariant(baseShift, "Clinic Triage Support", "Cover intake and basic triage support when the hallway starts overflowing.", payDelta, 0, stressDelta, 30, 0);
        }

        if (salmaTrust >= 10 || player.Skills.GetLevel(SkillId.Medical) >= 2 || player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            var payDelta = player.BackgroundType == BackgroundType.MedicalSchoolDropout ? 8 : 6;
            var stressDelta = player.BackgroundType == BackgroundType.MedicalSchoolDropout ? -4 : -2;
            return CreateShiftVariant(baseShift, "Clinic Intake Desk", "Take the intake desk and patient forms for the busier part of the morning.", payDelta, 0, stressDelta, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveWorkshopShift(RelationshipState relationshipState, JobTrackProgress track)
    {
        var baseShift = JobRegistry.WorkshopSewing;
        var abuSamirTrust = relationshipState.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).Trust;
        if (abuSamirTrust >= 20 && track.Reliability >= 75)
        {
            return CreateShiftVariant(baseShift, "Workshop Rush Table", "Take the better table when Abu Samir has a rush order breathing down his neck.", 10, -2, 3, 0, 0);
        }

        if (abuSamirTrust >= 10 || track.Reliability >= 60)
        {
            return CreateShiftVariant(baseShift, "Workshop Finishing Table", "Work the cleaner finishing line instead of the worst packing pile.", 6, -3, 0, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveCafeShift(PlayerCharacter player, RelationshipState relationshipState, JobTrackProgress track)
    {
        var baseShift = JobRegistry.CafeService;
        var nadiaTrust = relationshipState.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust;
        if (nadiaTrust >= 20 && track.Reliability >= 70)
        {
            return CreateShiftVariant(baseShift, "Cafe Front Tables", "Handle the customers Nadia trusts to tip, complain, and test you in equal measure.", 9, 0, 4, 0, 0);
        }

        if (nadiaTrust >= 10 || player.Skills.GetLevel(SkillId.Persuasion) >= 2)
        {
            var rushShift = CreateShiftVariant(baseShift, "Cafe Rush Tables", "Take the evening rush when the tea keeps moving and the room never quiets down.", 5, 0, 2, 0, 0);
            return ApplySudanesePenalty(player, rushShift);
        }

        return ApplySudanesePenalty(player, baseShift);
    }

    private static JobShift ResolvePharmacyShift(PlayerCharacter player, RelationshipState relationshipState, JobTrackProgress track)
    {
        var baseShift = JobRegistry.PharmacyStock;
        var mariamTrust = relationshipState.GetNpcRelationship(NpcId.PharmacistMariam).Trust;
        if (mariamTrust >= 20 && track.Reliability >= 70)
        {
            return CreateShiftVariant(baseShift, "Pharmacy Counter Support", "Handle the counter during the afternoon crush while Mariam checks scripts and shortages.", 9, 0, 2, 0, 0);
        }

        if (mariamTrust >= 10 || player.Skills.GetLevel(SkillId.Medical) >= 2 || player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            return CreateShiftVariant(baseShift, "Pharmacy Restock Run", "Rotate deliveries, sort invoices, and keep cheap painkillers from vanishing too fast.", 5, 0, -1, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveDepotShift(PlayerCharacter player, RelationshipState relationshipState, JobTrackProgress track)
    {
        var baseShift = JobRegistry.MicrobusDispatch;
        var safaaTrust = relationshipState.GetNpcRelationship(NpcId.DispatcherSafaa).Trust;
        if (safaaTrust >= 20 && track.Reliability >= 70)
        {
            return CreateShiftVariant(baseShift, "Depot Route Board", "Keep the board moving, settle queue fights, and fill the hungriest route first.", 10, -2, 3, 0, 0);
        }

        if (safaaTrust >= 10 || player.Skills.GetLevel(SkillId.Persuasion) >= 2)
        {
            return CreateShiftVariant(baseShift, "Platform Caller", "Shout routes and keep impatient drivers from losing the line before noon.", 5, 0, 1, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ResolveLaundryShift(RelationshipState relationshipState, JobTrackProgress track)
    {
        var baseShift = JobRegistry.LaundryPressing;
        var imanTrust = relationshipState.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust;
        if (imanTrust >= 20 && track.Reliability >= 75)
        {
            return CreateShiftVariant(baseShift, "Laundry Front Counter", "Take customer handoffs and the better pressing table when Iman trusts you not to lose a ticket.", 9, -3, 2, 0, 0);
        }

        if (imanTrust >= 10 || track.Reliability >= 60)
        {
            return CreateShiftVariant(baseShift, "Laundry Sorting Table", "Sort linens and finished bundles instead of standing at the hottest iron all shift.", 5, -4, 0, 0, 0);
        }

        return baseShift;
    }

    private static JobShift ApplySudanesePenalty(PlayerCharacter player, JobShift shift)
    {
        if (player.BackgroundType != BackgroundType.SudaneseRefugee)
        {
            return shift;
        }

        return new JobShift
        {
            Type = shift.Type,
            Name = shift.Name,
            Description = $"{shift.Description} The room reads your accent before it reads your effort.",
            BasePay = Math.Max(0, shift.BasePay - 3),
            EnergyCost = shift.EnergyCost,
            StressCost = shift.StressCost + 2,
            DurationMinutes = shift.DurationMinutes,
            MinEnergyRequired = shift.MinEnergyRequired,
            PayVariance = shift.PayVariance
        };
    }

    private static JobShift CreateShiftVariant(JobShift baseShift, string name, string description, int payDelta, int energyDelta, int stressDelta, int durationDelta, int minEnergyDelta)
    {
        return new JobShift
        {
            Type = baseShift.Type,
            Name = name,
            Description = description,
            BasePay = Math.Max(0, baseShift.BasePay + payDelta),
            EnergyCost = Math.Max(0, baseShift.EnergyCost + energyDelta),
            StressCost = Math.Max(0, baseShift.StressCost + stressDelta),
            DurationMinutes = Math.Max(60, baseShift.DurationMinutes + durationDelta),
            MinEnergyRequired = Math.Max(0, baseShift.MinEnergyRequired + minEnergyDelta),
            PayVariance = baseShift.PayVariance
        };
    }

    private static int GetReliabilityGain(JobType jobType, JobTrackProgress track)
    {
        return jobType switch
        {
            JobType.CallCenterWork when track.Reliability >= 70 => 4,
            JobType.ClinicReception when track.Reliability >= 70 => 4,
            _ => 6
        };
    }

    private static bool ShouldApplyMistake(JobShift job, PlayerCharacter player)
    {
        return job.Type switch
        {
            JobType.CallCenterWork => player.Stats.Stress >= 60,
            JobType.ClinicReception => player.Stats.Stress >= 65 || player.Stats.Energy <= job.MinEnergyRequired + 4,
            JobType.CafeService => player.Stats.Stress >= 65,
            JobType.PharmacyStock => player.Stats.Stress >= 60 || player.Stats.Energy <= job.MinEnergyRequired + 4,
            JobType.MicrobusDispatch => player.Stats.Stress >= 62,
            JobType.LaundryPressing => player.Stats.Energy <= job.MinEnergyRequired + 5,
            JobType.WorkshopSewing => player.Stats.Energy <= job.MinEnergyRequired + 5,
            JobType.BakeryWork => player.Stats.Energy <= job.MinEnergyRequired + 5,
            JobType.HouseCleaning => player.Stats.Energy <= job.MinEnergyRequired + 3,
            _ => false
        };
    }

    private static int GetMistakeStressPenalty(JobType jobType)
    {
        return jobType switch
        {
            JobType.CallCenterWork => 10,
            JobType.ClinicReception => 8,
            JobType.CafeService => 8,
            JobType.MicrobusDispatch => 9,
            JobType.PharmacyStock => 8,
            _ => 6
        };
    }

    private static int GetLockoutDays(JobType jobType)
    {
        return jobType switch
        {
            JobType.CallCenterWork => 2,
            JobType.ClinicReception => 1,
            JobType.WorkshopSewing => 1,
            JobType.CafeService => 1,
            JobType.BakeryWork => 1,
            JobType.PharmacyStock => 1,
            JobType.MicrobusDispatch => 1,
            JobType.LaundryPressing => 1,
            _ => 0
        };
    }

    private static int GetMistakeReliabilityLoss(JobType jobType)
    {
        return jobType switch
        {
            JobType.CallCenterWork => -15,
            JobType.ClinicReception => -12,
            JobType.PharmacyStock => -12,
            _ => -10
        };
    }

    private static string GetVariantReason(JobType jobType, PlayerCharacter player, RelationshipState relationshipState, JobTrackProgress track)
    {
        return jobType switch
        {
            JobType.BakeryWork when track.Reliability >= 75 && player.Skills.GetLevel(SkillId.Physical) >= 3 => "Unlocked by reliability 75 and Physical 3.",
            JobType.BakeryWork when track.Reliability >= 55 => "Unlocked by reliability 55.",
            JobType.BakeryWork when player.Skills.GetLevel(SkillId.Physical) >= 2 => "Unlocked by Physical 2.",
            JobType.HouseCleaning when track.Reliability >= 80 => "Unlocked by reliability 80.",
            JobType.HouseCleaning when track.Reliability >= 60 => "Unlocked by reliability 60.",
            JobType.CallCenterWork when track.Reliability >= 70 && player.Skills.GetLevel(SkillId.Persuasion) >= 2 => "Unlocked by reliability 70 and Persuasion 2.",
            JobType.CallCenterWork when track.Reliability >= 55 => "Unlocked by reliability 55.",
            JobType.ClinicReception when relationshipState.GetNpcRelationship(NpcId.NurseSalma).Trust >= 20 && track.Reliability >= 70 => "Unlocked by Nurse Salma trust 20 and reliability 70.",
            JobType.ClinicReception when relationshipState.GetNpcRelationship(NpcId.NurseSalma).Trust >= 10 => "Unlocked by Nurse Salma trust 10.",
            JobType.ClinicReception when player.Skills.GetLevel(SkillId.Medical) >= 2 => "Unlocked by Medical 2.",
            JobType.ClinicReception when player.BackgroundType == BackgroundType.MedicalSchoolDropout => "Unlocked by your medical-school background.",
            JobType.WorkshopSewing when relationshipState.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).Trust >= 20 && track.Reliability >= 75 => "Unlocked by Abu Samir trust 20 and reliability 75.",
            JobType.WorkshopSewing when relationshipState.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).Trust >= 10 => "Unlocked by Abu Samir trust 10.",
            JobType.WorkshopSewing when track.Reliability >= 60 => "Unlocked by reliability 60.",
            JobType.CafeService when relationshipState.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust >= 20 && track.Reliability >= 70 => "Unlocked by Nadia trust 20 and reliability 70.",
            JobType.CafeService when relationshipState.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust >= 10 => "Unlocked by Nadia trust 10.",
            JobType.CafeService when player.Skills.GetLevel(SkillId.Persuasion) >= 2 => "Unlocked by Persuasion 2.",
            JobType.PharmacyStock when relationshipState.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 20 && track.Reliability >= 70 => "Unlocked by Mariam trust 20 and reliability 70.",
            JobType.PharmacyStock when relationshipState.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 10 => "Unlocked by Mariam trust 10.",
            JobType.PharmacyStock when player.Skills.GetLevel(SkillId.Medical) >= 2 => "Unlocked by Medical 2.",
            JobType.PharmacyStock when player.BackgroundType == BackgroundType.MedicalSchoolDropout => "Unlocked by your medical-school background.",
            JobType.MicrobusDispatch when relationshipState.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 20 && track.Reliability >= 70 => "Unlocked by Safaa trust 20 and reliability 70.",
            JobType.MicrobusDispatch when relationshipState.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 10 => "Unlocked by Safaa trust 10.",
            JobType.MicrobusDispatch when player.Skills.GetLevel(SkillId.Persuasion) >= 2 => "Unlocked by Persuasion 2.",
            JobType.LaundryPressing when relationshipState.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 20 && track.Reliability >= 75 => "Unlocked by Iman trust 20 and reliability 75.",
            JobType.LaundryPressing when relationshipState.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 10 => "Unlocked by Iman trust 10.",
            JobType.LaundryPressing when track.Reliability >= 60 => "Unlocked by reliability 60.",
            _ => "Base shift."
        };
    }

    private static string? GetNextUnlockHint(JobType jobType, PlayerCharacter player, RelationshipState relationshipState, JobTrackProgress track)
    {
        return jobType switch
        {
            JobType.BakeryWork when track.Reliability < 55 && player.Skills.GetLevel(SkillId.Physical) < 2 => "Reach reliability 55 or Physical 2 for Bakery Oven Shift.",
            JobType.BakeryWork when track.Reliability < 75 || player.Skills.GetLevel(SkillId.Physical) < 3 => "Reach reliability 75 and Physical 3 for Bakery Dough Prep.",
            JobType.HouseCleaning when track.Reliability < 60 => "Reach reliability 60 for Regular Client Cleaning.",
            JobType.HouseCleaning when track.Reliability < 80 => "Reach reliability 80 for Full Apartment Cleaning.",
            JobType.CallCenterWork when track.Reliability < 55 => "Reach reliability 55 for Call Center Follow-Up Shift.",
            JobType.CallCenterWork when track.Reliability < 70 || player.Skills.GetLevel(SkillId.Persuasion) < 2 => "Reach reliability 70 and Persuasion 2 for Call Center Retention Queue.",
            JobType.ClinicReception when relationshipState.GetNpcRelationship(NpcId.NurseSalma).Trust < 10 && player.Skills.GetLevel(SkillId.Medical) < 2 && player.BackgroundType != BackgroundType.MedicalSchoolDropout => "Reach Nurse Salma trust 10, Medical 2, or use the medical-dropout background for Clinic Intake Desk.",
            JobType.ClinicReception when relationshipState.GetNpcRelationship(NpcId.NurseSalma).Trust < 20 || track.Reliability < 70 => "Reach Nurse Salma trust 20 and reliability 70 for Clinic Triage Support.",
            JobType.WorkshopSewing when relationshipState.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).Trust < 10 && track.Reliability < 60 => "Reach Abu Samir trust 10 or reliability 60 for Workshop Finishing Table.",
            JobType.WorkshopSewing when relationshipState.GetNpcRelationship(NpcId.WorkshopBossAbuSamir).Trust < 20 || track.Reliability < 75 => "Reach Abu Samir trust 20 and reliability 75 for Workshop Rush Table.",
            JobType.CafeService when relationshipState.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust < 10 && player.Skills.GetLevel(SkillId.Persuasion) < 2 => "Reach Nadia trust 10 or Persuasion 2 for Cafe Rush Tables.",
            JobType.CafeService when relationshipState.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust < 20 || track.Reliability < 70 => "Reach Nadia trust 20 and reliability 70 for Cafe Front Tables.",
            JobType.PharmacyStock when relationshipState.GetNpcRelationship(NpcId.PharmacistMariam).Trust < 10 && player.Skills.GetLevel(SkillId.Medical) < 2 && player.BackgroundType != BackgroundType.MedicalSchoolDropout => "Reach Mariam trust 10, Medical 2, or use the medical-dropout background for Pharmacy Restock Run.",
            JobType.PharmacyStock when relationshipState.GetNpcRelationship(NpcId.PharmacistMariam).Trust < 20 || track.Reliability < 70 => "Reach Mariam trust 20 and reliability 70 for Pharmacy Counter Support.",
            JobType.MicrobusDispatch when relationshipState.GetNpcRelationship(NpcId.DispatcherSafaa).Trust < 10 && player.Skills.GetLevel(SkillId.Persuasion) < 2 => "Reach Safaa trust 10 or Persuasion 2 for Platform Caller.",
            JobType.MicrobusDispatch when relationshipState.GetNpcRelationship(NpcId.DispatcherSafaa).Trust < 20 || track.Reliability < 70 => "Reach Safaa trust 20 and reliability 70 for Depot Route Board.",
            JobType.LaundryPressing when relationshipState.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust < 10 && track.Reliability < 60 => "Reach Iman trust 10 or reliability 60 for Laundry Sorting Table.",
            JobType.LaundryPressing when relationshipState.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust < 20 || track.Reliability < 75 => "Reach Iman trust 20 and reliability 75 for Laundry Front Counter.",
            _ => null
        };
    }

    private static List<string> GetActiveModifiers(JobShift resolvedJob, PlayerCharacter player)
    {
        var modifiers = new List<string>();

        if (player.Skills.GetLevel(SkillId.Physical) >= 3 &&
            resolvedJob.Type is JobType.BakeryWork or JobType.HouseCleaning or JobType.WorkshopSewing)
        {
            modifiers.Add("Physical 3 reduces energy cost by 5.");
        }

        if (player.BackgroundType == BackgroundType.SudaneseRefugee && resolvedJob.Type == JobType.CafeService)
        {
            modifiers.Add("Sudanese refugee background applies cafe friction: lower pay, higher stress.");
        }

        if (player.BackgroundType == BackgroundType.MedicalSchoolDropout && resolvedJob.Type == JobType.ClinicReception)
        {
            modifiers.Add("Medical-dropout background improves clinic shift outcomes.");
        }

        if (player.BackgroundType == BackgroundType.MedicalSchoolDropout && resolvedJob.Type == JobType.PharmacyStock)
        {
            modifiers.Add("Medical-dropout background helps you read stock and scripts faster.");
        }

        return modifiers;
    }

    private static string GetRiskWarning(JobShift resolvedJob, PlayerCharacter player)
    {
        return resolvedJob.Type switch
        {
            JobType.CallCenterWork or JobType.CafeService when player.Stats.Stress >= 60 => "High mistake risk from stress.",
            JobType.ClinicReception when player.Stats.Stress >= 65 => "High mistake risk from stress.",
            JobType.PharmacyStock when player.Stats.Stress >= 60 => "High mistake risk from stress.",
            JobType.MicrobusDispatch when player.Stats.Stress >= 62 => "High mistake risk from stress.",
            JobType.ClinicReception or JobType.WorkshopSewing or JobType.BakeryWork or JobType.HouseCleaning when player.Stats.Energy <= resolvedJob.MinEnergyRequired + 5 => "High mistake risk from low energy.",
            JobType.PharmacyStock or JobType.LaundryPressing when player.Stats.Energy <= resolvedJob.MinEnergyRequired + 5 => "High mistake risk from low energy.",
            _ => "High mistake risk under current conditions."
        };
    }

    private static bool IsJobAvailableAtLocation(JobType jobType, LocationId locationId)
    {
        return jobType switch
        {
            JobType.BakeryWork => locationId == LocationId.Bakery,
            JobType.HouseCleaning => locationId == LocationId.Market,
            JobType.CallCenterWork => locationId == LocationId.CallCenter,
            JobType.ClinicReception => locationId == LocationId.Clinic,
            JobType.WorkshopSewing => locationId == LocationId.Workshop,
            JobType.CafeService => locationId == LocationId.Cafe,
            JobType.PharmacyStock => locationId == LocationId.Pharmacy,
            JobType.MicrobusDispatch => locationId == LocationId.Depot,
            JobType.LaundryPressing => locationId == LocationId.Laundry,
            _ => false
        };
    }
}
