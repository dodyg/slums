using System.Globalization;
using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.Weather;
using Slums.Core.World;

namespace Slums.Core.Crimes;

internal static class CrimeSessionService
{
    public static IReadOnlyList<CrimeAttempt> GetAvailableCrimes(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (GetCrimeBlockReason(session) is not null)
        {
            return [];
        }

        var location = session.World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        var crimes = CrimeRegistry.GetAvailableCrimes(location, session.Relationships).ToList();

        if (location.Id == LocationId.Square &&
            crimes.All(static attempt => attempt.Type != CrimeType.DokkiDrop) &&
            (session.JobProgress.GetTrack(JobType.CallCenterWork).Reliability >= 60 || session.JobProgress.GetTrack(JobType.CafeService).Reliability >= 60))
        {
            crimes.Add(new CrimeAttempt(CrimeType.DokkiDrop, 95, 42, 24, 0, 18));
        }

        if (location.Id == LocationId.Market &&
            crimes.All(static attempt => attempt.Type != CrimeType.NetworkErrand) &&
            session.Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner &&
            session.Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation >= 10)
        {
            crimes.Add(new CrimeAttempt(CrimeType.NetworkErrand, 130, 48, 28, 0, 24));
        }

        if (location.Id == LocationId.Depot &&
            crimes.All(static attempt => attempt.Type != CrimeType.DepotFareSkim) &&
            session.JobProgress.GetTrack(JobType.MicrobusDispatch).Reliability >= 60)
        {
            crimes.Add(new CrimeAttempt(CrimeType.DepotFareSkim, 78, 28, 14, 0, 16));
        }

        if (location.Id == LocationId.Laundry &&
            crimes.All(static attempt => attempt.Type != CrimeType.ShubraBundleLift) &&
            session.JobProgress.GetTrack(JobType.LaundryPressing).Reliability >= 60)
        {
            crimes.Add(new CrimeAttempt(CrimeType.ShubraBundleLift, 68, 24, 12, 0, 15));
        }

        return crimes;
    }

    public static string? GetCrimeBlockReason(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.CurrentWeather.BlocksCrime)
        {
            return WeatherActivityRules.GetCrimeBlockReason(session.CurrentWeather);
        }

        return TerritoryDynamicsCalculator.IsCrimeBlocked(session.Territory, session.World.CurrentDistrict)
            ? "The streets are too dangerous for any criminal activity right now."
            : null;
    }

    public static CrimeResult CommitCrime(GameSession session, CrimeAttempt attempt, Random? random)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(attempt);

        var before = session.CaptureStats();
        var blockReason = GetCrimeBlockReason(session);
        if (blockReason is not null)
        {
            var blockedResult = new CrimeResult { Message = blockReason };
            session.RecordMutation(MutationCategories.GuardRejected, "CommitCrime", before, session.CaptureStats(), blockReason);
            session.RaiseEvent(blockReason);
            return blockedResult;
        }

        var modifierEvaluation = EvaluateCrimeModifiers(session, attempt);
        var modifiedAttempt = modifierEvaluation.Attempt;
        ApplyCrimeModifierSideEffects(session, modifierEvaluation.Signals);
        var districtHeat = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        var result = session.CrimeService.AttemptCrime(modifiedAttempt, session.Player, districtHeat, random ?? session.SharedRandom);
        session.Player.Stats.ModifyEnergy(-result.EnergyCost);
        session.Player.Stats.ModifyStress(result.StressCost);
        ActivityLedgerSystem.RecordCrimeOutcome(session.CrimeState, session.Clock, result);

        if (result.Success)
        {
            session.Player.Stats.ModifyMoney(result.MoneyEarned);
            session.ApplySkillGain(GetSkillForCrime(attempt.Type));
            session.ModifyFactionReputation(GetFactionForCurrentCrimeRoute(session), 4);
            if (session.Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
            {
                session.ModifyFactionReputation(FactionId.ExPrisonerNetwork, 5);
            }

            var storyFlags = session.StoryFlags.ToHashSet(StringComparer.Ordinal);
            session.TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetFirstSuccessTrigger(storyFlags));
        }

        session.TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetRouteSceneTrigger(attempt.Type, result));

        session.DistrictHeat.AddHeat(session.World.CurrentDistrict, result.PolicePressureDelta);
        var updatedDistrictHeat = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        var currentStoryFlags = session.StoryFlags.ToHashSet(StringComparer.Ordinal);
        session.TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetPoliceEncounterTrigger(
            session.World.CurrentDistrict,
            districtHeat,
            updatedDistrictHeat,
            currentStoryFlags));
        TerritoryDynamicsCalculator.ApplyCrimeImpact(session.Territory, session.World.CurrentDistrict, null);
        session.RaiseEvent(result.Message);
        ApplyCrimeContactAftermath(session, result);

        session.TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetGangRetaliationTrigger(
            result.Detected,
            session.World.CurrentDistrict,
            session.Territory.GetControl(session.World.CurrentDistrict).ControllingFaction,
            session.Relationships,
            session.StoryFlags.ToHashSet(StringComparer.Ordinal)));

        if (session.TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetCrimeWarningTrigger(session.PolicePressure, session.StoryFlags.ToHashSet(StringComparer.Ordinal))))
        {
            session.RaiseEvent("People are whispering that the police are getting close.");
        }

        session.AdvanceTime(attempt.DurationMinutes);
        session.CheckGameOverConditions();
        session.RecordMutation(MutationCategories.Crime, "CommitCrime", before, session.CaptureStats(), $"{attempt.Type}: success={result.Success}, detected={result.Detected}");
        return result;
    }

    public static CrimeRoutePreview PreviewCrime(GameSession session, CrimeAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(attempt);

        var modifierEvaluation = EvaluateCrimeModifiers(session, attempt);
        var districtHeat = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        var resolution = session.CrimeService.PreviewCrime(modifierEvaluation.Attempt, session.Player, districtHeat);
        return new CrimeRoutePreview(modifierEvaluation.Attempt, resolution, modifierEvaluation.ActiveModifiers);
    }

    public static CrimeModifierEvaluation EvaluateCrimeModifiers(GameSession session, CrimeAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(attempt);

        var modifiedAttempt = attempt;
        var activeModifiers = new List<string>();
        var signals = new HashSet<CrimeModifierSignal>();

        if (session.LastPublicFacingWorkDay == session.Clock.Day)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Max(5, modifiedAttempt.DetectionRisk - 8),
                PolicePressureIncrease = Math.Max(1, modifiedAttempt.PolicePressureIncrease - 4)
            };
            activeModifiers.Add("Same-day public-facing work gives you a thin alibi: lower risk and lower pressure.");
            signals.Add(CrimeModifierSignal.ThinAlibi);
        }

        if (session.Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Min(95, modifiedAttempt.DetectionRisk + 5),
                PolicePressureIncrease = modifiedAttempt.PolicePressureIncrease + 5
            };
            activeModifiers.Add("Released political prisoner background increases scrutiny and pressure.");
            signals.Add(CrimeModifierSignal.PrisonerScrutiny);
        }

        if (session.Player.Skills.GetLevel(SkillId.StreetSmarts) >= 3)
        {
            activeModifiers.Add("Street Smarts 3 lowers detection chance by 10.");
        }

        if (attempt.Type == CrimeType.NetworkErrand && session.Player.Skills.GetLevel(SkillId.CyberHacking) >= 2)
        {
            activeModifiers.Add("Cyber Hacking 2 steadies network errands: higher success chance.");
        }

        if (session.PolicePressure >= 60)
        {
            activeModifiers.Add("Current police pressure is materially increasing detection risk.");
        }

        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        if (districtCondition is not null)
        {
            var effect = districtCondition.Effect;
            if (effect.CrimeDetectionRiskModifier != 0 || effect.CrimeRewardModifier != 0)
            {
                modifiedAttempt = modifiedAttempt with
                {
                    DetectionRisk = Math.Clamp(modifiedAttempt.DetectionRisk + effect.CrimeDetectionRiskModifier, 1, 95),
                    BaseReward = Math.Max(0, modifiedAttempt.BaseReward + effect.CrimeRewardModifier)
                };

                activeModifiers.Add(BuildCrimeDistrictModifierText(districtCondition));
            }
        }

        var schedule = session.GetCurrentSchedule();
        if (schedule.CrimeDetectionModifier != 0)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Clamp(modifiedAttempt.DetectionRisk + schedule.CrimeDetectionModifier, 1, 95)
            };
            activeModifiers.Add($"{schedule.DayName}: crime detection {schedule.CrimeDetectionModifier} (schedule effect).");
        }

        if (session.CurrentWeather.CrimeDetectionModifier != 0)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Clamp(modifiedAttempt.DetectionRisk + session.CurrentWeather.CrimeDetectionModifier, 1, 95)
            };
            activeModifiers.Add($"{WeatherModifiers.GetDisplayName(session.CurrentWeather.Type)} weather: crime detection {session.CurrentWeather.CrimeDetectionModifier:+#;-#;0}.");
        }

        return new CrimeModifierEvaluation(modifiedAttempt, activeModifiers, signals);
    }

    public static void ApplyCrimeModifierSideEffects(GameSession session, IReadOnlySet<CrimeModifierSignal> signals)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(signals);

        if (signals.Contains(CrimeModifierSignal.ThinAlibi))
        {
            session.RaiseEvent("The shift you worked today gives you a thin alibi and a cleaner reason to be seen moving.");
        }

        if (signals.Contains(CrimeModifierSignal.PrisonerScrutiny))
        {
            session.TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetPrisonerHeatTrigger(session.Player.BackgroundType, session.StoryFlags.ToHashSet(StringComparer.Ordinal)));
        }
    }

    public static void AdjustPolicePressure(GameSession session, int delta)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.DistrictHeat.AddHeat(session.World.CurrentDistrict, delta);
    }

    public static void SetPolicePressure(GameSession session, int value)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.DistrictHeat.SetHeatAll(value);
    }

    public static void SetCrimeCounters(GameSession session, int totalCrimeEarnings, int crimesCommitted)
    {
        ArgumentNullException.ThrowIfNull(session);
        SetCrimeCounters(session, totalCrimeEarnings, crimesCommitted, session.LastCrimeDay);
    }

    public static void SetCrimeCounters(GameSession session, int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.CrimeState.TotalCrimeEarnings = Math.Max(0, totalCrimeEarnings);
        session.CrimeState.CrimesCommitted = Math.Max(0, crimesCommitted);
        session.CrimeState.LastCrimeDay = Math.Max(0, lastCrimeDay);
    }

    public static void RestoreCrimeState(GameSession session, int policePressure, int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay, bool hasCrimeCommittedToday)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.DistrictHeat.SetHeatAll(policePressure);
        SetCrimeCounters(session, totalCrimeEarnings, crimesCommitted, lastCrimeDay);
        session.CrimeState.CrimeCommittedToday = hasCrimeCommittedToday;
    }

    private static FactionId GetFactionForCurrentCrimeRoute(GameSession session)
    {
        var controllingFaction = session.Territory.GetControl(session.World.CurrentDistrict).ControllingFaction;
        if (controllingFaction.HasValue)
        {
            return controllingFaction.Value;
        }

        return session.World.CurrentDistrict switch
        {
            DistrictId.Dokki => FactionId.DokkiThugs,
            DistrictId.ArdAlLiwa => FactionId.ExPrisonerNetwork,
            _ => FactionId.ImbabaCrew
        };
    }

    private static void ApplyCrimeContactAftermath(GameSession session, CrimeResult result)
    {
        var aftermath = CrimeNarrativePlanner.GetDetectedContactAftermath(session.World.CurrentLocationId, session.Relationships, result);
        if (aftermath is null)
        {
            return;
        }

        ReduceCrimeHeat(session, aftermath.PolicePressureReduction, aftermath.HeatMessage, aftermath.HeatTrigger);

        if (!result.Success && !string.IsNullOrWhiteSpace(aftermath.FailureMessage))
        {
            ApplyCrimeFailureMitigation(session, aftermath.FailureMoneyGain, aftermath.FailureStressRelief, aftermath.FailureMessage, aftermath.FailureTrigger);
        }
    }

    private static void ReduceCrimeHeat(GameSession session, int amount, string message, NarrativeSceneTrigger trigger)
    {
        if (amount <= 0)
        {
            return;
        }

        var currentHeat = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        var updatedHeat = Math.Max(0, currentHeat - amount);
        if (updatedHeat == currentHeat)
        {
            return;
        }

        session.DistrictHeat.SetHeat(session.World.CurrentDistrict, updatedHeat);
        session.RaiseEvent(message);
        session.TryQueueNarrativeTrigger(trigger);
    }

    private static void ApplyCrimeFailureMitigation(GameSession session, int moneyGain, int stressRelief, string message, NarrativeSceneTrigger? trigger)
    {
        if (moneyGain > 0)
        {
            session.Player.Stats.ModifyMoney(moneyGain);
        }

        if (stressRelief > 0)
        {
            session.Player.Stats.ModifyStress(-stressRelief);
        }

        session.RaiseEvent(message);
        session.TryQueueNarrativeTrigger(trigger);
    }

    private static SkillId GetSkillForCrime(CrimeType crimeType)
    {
        return crimeType == CrimeType.NetworkErrand ? SkillId.CyberHacking : SkillId.StreetSmarts;
    }

    private static string BuildCrimeDistrictModifierText(DistrictConditionDefinition districtCondition)
    {
        var parts = new List<string>();
        if (districtCondition.Effect.CrimeDetectionRiskModifier != 0)
        {
            parts.Add($"detection {FormatSignedValue(districtCondition.Effect.CrimeDetectionRiskModifier)}");
        }

        if (districtCondition.Effect.CrimeRewardModifier != 0)
        {
            parts.Add($"reward {FormatSignedValue(districtCondition.Effect.CrimeRewardModifier)} LE");
        }

        return $"{districtCondition.Title} affects street work today: {string.Join(", ", parts)}.";
    }

    private static string FormatSignedValue(int value)
    {
        return value >= 0
            ? $"+{value.ToString(CultureInfo.InvariantCulture)}"
            : value.ToString(CultureInfo.InvariantCulture);
    }
}
