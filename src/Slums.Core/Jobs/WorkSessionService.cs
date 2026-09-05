using System.Globalization;
using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Diagnostics;
using Slums.Core.Information;
using Slums.Core.Relationships;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.Weather;
using Slums.Core.World;
using Slums.Core.World.News;

namespace Slums.Core.Jobs;

/// <summary>Applies session-side work actions, modifiers, ledgers, and work restoration.</summary>
internal static class WorkSessionService
{
    internal static JobResult Work(GameSession session, JobShift job, Random? random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(job);
        var before = session.CaptureStats();
        if (WeatherActivityRules.BlocksJob(session.CurrentWeather, job.Type))
        {
            var reason = WeatherActivityRules.GetJobBlockReason(session.CurrentWeather);
            session.RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, session.CaptureStats(), reason);
            session.RaiseEvent(reason);
            return JobResult.Failed(reason);
        }

        var location = session.World.GetCurrentLocation();
        if (location is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, session.CaptureStats(), "No current location");
            return JobResult.Failed("You are nowhere.");
        }

        var result = session.Jobs.PerformJob(
            job,
            session.Player,
            location,
            session.Relationships,
            session.JobProgress,
            session.Clock.Day,
            random ?? session.SharedRandom,
            NewsImpactCalculator.GetJobPayModifier(session.News, job.Type));

        if (result.Success)
        {
            ActivityLedgerSystem.RecordWorkShift(session.WorkState, session.Clock, job, result);
            if (!result.MistakeMade)
            {
                session.ApplySkillGain(GetSkillForJob(job.Type));
                ModifyEmployerTrust(session, job.Type, 2);
            }
            else
            {
                ModifyEmployerTrust(session, job.Type, -4);
            }

            ApplyWorkCrimeSpillover(session, job, result);
            ApplyBackgroundWorkFlavor(session, job, result);
            if (job.Type == JobType.RoboticsScavenging)
            {
                if (session.Player.Robotics.CanBuyParts(1))
                {
                    session.Player.Robotics.AddParts(1);
                    session.RaiseEvent("You salvage one usable board or actuator from the pile. Robot parts +1.");
                }

                var workingRobot = session.Player.Robotics.Robots.FirstOrDefault(static robot => robot.IsOperational);
                if (workingRobot is not null)
                {
                    workingRobot.Damage(10);
                    session.RaiseEvent($"The {RobotRegistry.GetByType(workingRobot.Type).Name} takes wear on the scavenging run. Condition: {workingRobot.Condition}%.");
                }

                if (RobotCapabilityRules.GetSalvageBonusParts(session.Player.Robotics) > 0 && session.Player.Robotics.CanBuyParts(1))
                {
                    session.Player.Robotics.AddParts(1);
                    session.RaiseEvent("The Salvage Crawler finds one extra usable actuator. Robot parts +1.");
                }
            }

            TerritoryDynamicsCalculator.ApplyHonestWorkImpact(session.Territory, session.World.CurrentDistrict);
            session.RaiseEvent(result.Message);
            session.RecordMutation(MutationCategories.Work, "WorkJob", before, session.CaptureStats(), result.Message);
            session.AdvanceTime(job.DurationMinutes);
        }
        else
        {
            session.RaiseEvent(result.Message);
            session.RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, session.CaptureStats(), result.Message);
        }

        session.CheckGameOverConditions();
        return result;
    }

    internal static IReadOnlyList<JobShift> GetAvailable(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var location = session.World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        var schedule = session.GetCurrentSchedule();
        return session.Jobs.GetAvailableJobs(location, session.Player, session.Relationships, session.JobProgress)
            .Where(job => !schedule.BlockedJobTypes.Contains(job.Type.ToString()))
            .Where(job => !WeatherActivityRules.BlocksJob(session.CurrentWeather, job.Type))
            .Select(job => ApplyDayScheduleToJob(ApplyDistrictConditionToJob(session, job), schedule))
            .ToArray();
    }

    internal static JobPreview Preview(GameSession session, JobType jobType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var preview = ApplyDistrictConditionToJobPreview(session, session.Jobs.PreviewJob(jobType, session.Player, session.Relationships, session.JobProgress));
        var modifiers = preview.ActiveModifiers.ToList();
        var payModifier = NewsImpactCalculator.GetJobPayModifier(session.News, jobType);
        if (payModifier != 0)
        {
            modifiers.Add($"City news changes this shift's pay by {payModifier} LE.");
        }

        var infrastructure = session.Infrastructure.Get(session.World.CurrentDistrict, InfrastructureServiceType.Electricity);
        if (infrastructure.IsActive)
        {
            modifiers.Add($"Electricity is {infrastructure.Severity} here; workshop and office work may be interrupted.");
        }

        return preview with { ActiveModifiers = modifiers };
    }

    internal static void RestoreTrack(GameSession session, JobType jobType, int reliability, int shiftsCompleted, int lockoutUntilDay)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.JobProgress.RestoreTrack(jobType, reliability, shiftsCompleted, lockoutUntilDay);
    }

    private static JobPreview ApplyDistrictConditionToJobPreview(GameSession session, JobPreview preview)
    {
        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        var schedule = session.GetCurrentSchedule();
        var hasDistrictModifiers = districtCondition is not null && (districtCondition.Effect.WorkPayModifier != 0 || districtCondition.Effect.WorkStressModifier != 0);
        var hasScheduleModifiers = schedule.JobPayModifier != 0 || schedule.JobPayOverrides.Count > 0;
        if (!hasDistrictModifiers && !hasScheduleModifiers)
        {
            return preview;
        }

        var activeModifiers = preview.ActiveModifiers.ToList();
        if (hasDistrictModifiers)
        {
            activeModifiers.Add(BuildWorkDistrictModifierText(districtCondition!));
        }
        if (hasScheduleModifiers)
        {
            activeModifiers.Add($"{schedule.DayName}: pay {schedule.JobPayModifier:+#;-#;0} LE (schedule).");
        }
        if (schedule.JobPayOverrides.TryGetValue(preview.Job.Type.ToString(), out var jobPayOverride))
        {
            activeModifiers.Add($"{schedule.DayName}: {preview.Job.Type} pay {jobPayOverride:+#;-#;0} LE (schedule).");
        }

        var job = preview.Job;
        if (hasDistrictModifiers)
        {
            job = ApplyDistrictConditionToJob(session, job);
        }
        if (hasScheduleModifiers)
        {
            job = ApplyDayScheduleToJob(job, schedule);
        }

        return new JobPreview(job, preview.VariantReason, preview.NextUnlockHint, activeModifiers, preview.RiskWarning);
    }

    private static JobShift ApplyDistrictConditionToJob(GameSession session, JobShift job)
    {
        ArgumentNullException.ThrowIfNull(job);
        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        if (districtCondition is null || (districtCondition.Effect.WorkPayModifier == 0 && districtCondition.Effect.WorkStressModifier == 0))
        {
            return job;
        }

        return CloneJobShift(job, Math.Max(0, job.BasePay + districtCondition.Effect.WorkPayModifier), Math.Max(0, job.StressCost + districtCondition.Effect.WorkStressModifier));
    }

    private static JobShift ApplyDayScheduleToJob(JobShift job, DayScheduleModifiers schedule)
    {
        if (schedule.JobPayModifier == 0 && !schedule.JobPayOverrides.TryGetValue(job.Type.ToString(), out _))
        {
            return job;
        }

        var payModifier = schedule.JobPayModifier;
        if (schedule.JobPayOverrides.TryGetValue(job.Type.ToString(), out var jobPayOverride))
        {
            payModifier += jobPayOverride;
        }
        if (payModifier == 0)
        {
            return job;
        }

        return CloneJobShift(job, Math.Max(0, job.BasePay + payModifier), job.StressCost);
    }

    private static JobShift CloneJobShift(JobShift source, int basePay, int stressCost)
    {
        return new JobShift
        {
            Type = source.Type,
            Name = source.Name,
            Description = source.Description,
            BasePay = basePay,
            EnergyCost = source.EnergyCost,
            StressCost = stressCost,
            DurationMinutes = source.DurationMinutes,
            MinEnergyRequired = source.MinEnergyRequired,
            PayVariance = source.PayVariance
        };
    }

    private static string BuildWorkDistrictModifierText(DistrictConditionDefinition districtCondition)
    {
        var parts = new List<string>();
        if (districtCondition.Effect.WorkPayModifier != 0)
        {
            parts.Add($"pay {FormatSignedValue(districtCondition.Effect.WorkPayModifier)} LE");
        }
        if (districtCondition.Effect.WorkStressModifier != 0)
        {
            parts.Add($"stress {FormatSignedValue(districtCondition.Effect.WorkStressModifier)}");
        }

        return $"{districtCondition.Title} affects shifts today: {string.Join(", ", parts)}.";
    }

    private static string FormatSignedValue(int value)
    {
        return value >= 0 ? $"+{value.ToString(CultureInfo.InvariantCulture)}" : value.ToString(CultureInfo.InvariantCulture);
    }

    private static SkillId GetSkillForJob(JobType jobType)
    {
        return jobType switch
        {
            JobType.BakeryWork => SkillId.Physical,
            JobType.HouseCleaning => SkillId.Physical,
            JobType.CallCenterWork => SkillId.Persuasion,
            JobType.PharmacyStock => SkillId.Medical,
            JobType.MicrobusDispatch => SkillId.Persuasion,
            JobType.LaundryPressing => SkillId.Physical,
            JobType.RoboticsScavenging => SkillId.RobotRepair,
            _ => SkillId.StreetSmarts
        };
    }

    private static void ModifyEmployerTrust(GameSession session, JobType jobType, int delta)
    {
        var npcId = jobType switch
        {
            JobType.ClinicReception => NpcId.NurseSalma,
            JobType.WorkshopSewing => NpcId.WorkshopBossAbuSamir,
            JobType.CafeService => NpcId.CafeOwnerNadia,
            JobType.PharmacyStock => NpcId.PharmacistMariam,
            JobType.MicrobusDispatch => NpcId.DispatcherSafaa,
            JobType.LaundryPressing => NpcId.LaundryOwnerIman,
            _ => (NpcId?)null
        };

        if (npcId.HasValue)
        {
            session.ModifyNpcTrust(npcId.Value, delta);
        }
    }

    private static void ApplyWorkCrimeSpillover(GameSession session, JobShift job, JobResult result)
    {
        var publicWorkHeat = WorkNarrativePlanner.GetPublicWorkHeatPlan(session.Clock.Day, session.LastCrimeDay, session.PolicePressure, session.StoryFlags.ToHashSet(), job);
        if (publicWorkHeat is not null)
        {
            session.Player.Stats.ModifyStress(publicWorkHeat.StressDelta);
            ModifyEmployerTrust(session, job.Type, publicWorkHeat.EmployerTrustDelta);
            session.RaiseEvent(publicWorkHeat.Message);
            session.TryQueueNarrativeTrigger(publicWorkHeat.NarrativeTrigger);
        }

        if (WorkNarrativePlanner.ShouldEmbarrassWorkshopBoss(job, result))
        {
            session.Relationships.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
            session.Relationships.RecordRefusal(NpcId.WorkshopBossAbuSamir, session.Clock.Day);
        }
    }

    private static void ApplyBackgroundWorkFlavor(GameSession session, JobShift job, JobResult result)
    {
        session.TryQueueNarrativeTrigger(WorkNarrativePlanner.GetMedicalClinicTrigger(session.Player, job, result, session.StoryFlags.ToHashSet()));
        if (WorkNarrativePlanner.ShouldGrantSalmaMedicineHelp(session.Player, job, result, session.Relationships))
        {
            session.Relationships.RecordFavor(NpcId.NurseSalma, session.Clock.Day, hasUnpaidDebt: true);
            session.RaiseEvent("Nurse Salma quietly covers a little medicine for your mother. You owe her now.");
        }
    }
}
