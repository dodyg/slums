using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Crimes;
using Slums.Core.Endings;
using Slums.Core.Expenses;
using Slums.Core.Events;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;

namespace Slums.Core.State;

public sealed class GameState
{
    private const int EndOfDayHour = 22;
    private const int StreetFoodCost = 8;
    private readonly CrimeService _crimeService = new();
    private readonly RandomEventService _randomEventService = new();
    private readonly Queue<string> _pendingNarrativeScenes = new();
    private readonly HashSet<string> _storyFlags = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _randomEventHistory = new(StringComparer.OrdinalIgnoreCase);
    private bool _crimeCommittedToday;

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public GameClock Clock { get; } = new();
    public PlayerCharacter Player { get; } = new();
    public WorldState World { get; } = new();
    public RelationshipState Relationships { get; } = new();
    public JobProgressState JobProgress { get; } = new();
    public JobService Jobs { get; } = new();
    public bool IsGameOver { get; private set; }
    public string? GameOverReason { get; private set; }
    public EndingId? EndingId { get; private set; }
    public int PolicePressure { get; private set; }
    public int TotalCrimeEarnings { get; private set; }
    public int CrimesCommitted { get; private set; }
    public int TotalHonestWorkEarnings { get; private set; }
    public int HonestShiftsCompleted { get; private set; }
    public int DaysSurvived { get; private set; }
    public int LastCrimeDay { get; private set; }
    public int LastHonestWorkDay { get; private set; }
    public int LastPublicFacingWorkDay { get; private set; }
    public IReadOnlyCollection<string> StoryFlags => _storyFlags;
    public IReadOnlyDictionary<string, int> RandomEventHistory => _randomEventHistory;
    public string? PendingEndingKnot { get; private set; }

    public event EventHandler<GameEventArgs>? GameEvent;

    public void AdvanceTime(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        while (minutes > 0)
        {
            var currentMinutes = (Clock.Hour * 60) + Clock.Minute;
            var endOfDayMinutes = EndOfDayHour * 60;

            if (currentMinutes >= endOfDayMinutes)
            {
                EndDay();
                if (IsGameOver)
                {
                    return;
                }

                continue;
            }

            var minutesUntilEndOfDay = endOfDayMinutes - currentMinutes;
            var minutesToAdvance = Math.Min(minutes, minutesUntilEndOfDay);

            Clock.AdvanceMinutes(minutesToAdvance);
            minutes -= minutesToAdvance;

            if (Clock.IsEndOfDay && !IsGameOver)
            {
                EndDay();
                if (IsGameOver)
                {
                    return;
                }
            }
        }
    }

    public void EndDay(Random? random = null)
    {
        Player.Stats.ApplyDailyDecay();

        var nutritionResolution = Player.Nutrition.ResolveDay();
        Player.Stats.ModifyEnergy(nutritionResolution.EnergyDelta);
        Player.Stats.ModifyHealth(nutritionResolution.HealthDelta);
        Player.Stats.ModifyStress(nutritionResolution.StressDelta);
        SyncLegacyHunger();

        var motherCareResolution = Player.Household.ResolveDay();
        Player.Stats.ModifyStress(motherCareResolution.StressDelta);

        if (Player.Stats.Money >= RecurringExpenses.DailyRentCost)
        {
            Player.Stats.ModifyMoney(-RecurringExpenses.DailyRentCost);
            RaiseEvent($"Paid rent: {RecurringExpenses.DailyRentCost} LE");
        }
        else
        {
            RaiseEvent("Could not pay rent! The landlord is angry.");
        }

        if (!Player.Nutrition.AteToday)
        {
            RaiseEvent("You go to sleep hungry.");
        }

        if (!Player.Household.FedMotherToday)
        {
            RaiseEvent("Your mother went without a proper meal today.");
        }

        if (!Player.Household.MedicationGivenToday && Player.Household.MotherNeedsCare)
        {
            RaiseEvent("Your mother needed medicine today and did not get it.");
        }

        if (!_crimeCommittedToday && PolicePressure > 0)
        {
            var pressureDecay = Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner ? 2 : 5;
            SetPolicePressure(PolicePressure - pressureDecay);
        }

        if (Player.BackgroundType == BackgroundType.MedicalSchoolDropout && Player.Household.MotherHealth < 60)
        {
            Player.Stats.ModifyStress(3);
            RaiseEvent("Your training makes it harder to ignore every sign your mother's health is slipping.");
        }

        Clock.AdvanceToNextDay();
        DaysSurvived++;
        World.TravelTo(LocationId.Home);
        RaiseEvent("You return home for the night.");

        Player.Nutrition.BeginNewDay();
        Player.Household.BeginNewDay();

        foreach (var randomEvent in _randomEventService.RollDailyEvents(this, random ?? new Random()))
        {
            ApplyRandomEvent(randomEvent);
        }

        _crimeCommittedToday = false;
        CheckGameOverConditions();
    }

    public bool RestAtHome()
    {
        if (World.CurrentLocationId != LocationId.Home)
        {
            RaiseEvent("You need to go home to rest.");
            return false;
        }

        Player.Stats.Rest();
        AdvanceTime(8 * 60);
        RaiseEvent("You rest at home. 8 hours pass.");
        return true;
    }

    public bool TryTravelTo(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(l => l.Id == locationId);
        if (location is null)
        {
            return false;
        }

        if (World.CurrentLocationId == locationId)
        {
            RaiseEvent($"You are already at {location.Name}.");
            return false;
        }

        if (Player.Stats.Money < RecurringExpenses.TravelCost)
        {
            RaiseEvent("Not enough money for transport.");
            return false;
        }

        Player.Stats.ModifyMoney(-RecurringExpenses.TravelCost);
        Player.Stats.ModifyEnergy(-5);
        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && location.District == DistrictId.Dokki)
        {
            Player.Stats.ModifyStress(2);
            RaiseEvent("Dokki's questions land harder when your accent gets there before your name does.");
        }

        AdvanceTime(location.TravelTimeMinutes);
        World.TravelTo(locationId);

        RaiseEvent($"Traveled to {location.Name}.");
        return true;
    }

    public JobResult WorkJob(JobShift job)
    {
        ArgumentNullException.ThrowIfNull(job);

        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return JobResult.Failed("You are nowhere.");
        }

        var result = Jobs.PerformJob(job, Player, location, Relationships, JobProgress, Clock.Day);
        
        if (result.Success)
        {
            TotalCrimeEarnings += 0;
            TotalHonestWorkEarnings += result.MoneyEarned;
            HonestShiftsCompleted++;
            LastHonestWorkDay = Clock.Day;
            if (IsPublicFacingJob(job.Type))
            {
                LastPublicFacingWorkDay = Clock.Day;
            }
            AdvanceTime(job.DurationMinutes);
            if (!result.MistakeMade)
            {
                ApplySkillGain(GetSkillForJob(job.Type));
                ModifyEmployerTrust(job.Type, 2);
            }
            else
            {
                ModifyEmployerTrust(job.Type, -4);
            }

            ApplyWorkCrimeSpillover(job, result);
            ApplyBackgroundWorkFlavor(job, result);

            RaiseEvent(result.Message);
        }
        else
        {
            RaiseEvent(result.Message);
        }

        CheckGameOverConditions();
        return result;
    }

    public IReadOnlyList<JobShift> GetAvailableJobs()
    {
        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        return Jobs.GetAvailableJobs(location, Player, Relationships, JobProgress).ToArray();
    }

    public IReadOnlyList<CrimeAttempt> GetAvailableCrimes()
    {
        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        var crimes = CrimeRegistry.GetAvailableCrimes(location, Relationships).ToList();

        if (location.Id == LocationId.Square &&
            crimes.All(static attempt => attempt.Type != CrimeType.DokkiDrop) &&
            (JobProgress.GetTrack(JobType.CallCenterWork).Reliability >= 60 || JobProgress.GetTrack(JobType.CafeService).Reliability >= 60))
        {
            crimes.Add(new CrimeAttempt(CrimeType.DokkiDrop, 95, 42, 24, 0, 18));
        }

        if (location.Id == LocationId.Market &&
            crimes.All(static attempt => attempt.Type != CrimeType.NetworkErrand) &&
            Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner &&
            Relationships.GetFactionStanding(FactionId.ExPrisonerNetwork).Reputation >= 10)
        {
            crimes.Add(new CrimeAttempt(CrimeType.NetworkErrand, 130, 48, 28, 0, 24));
        }

        return crimes;
    }

    public CrimeResult CommitCrime(CrimeAttempt attempt, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var modifiedAttempt = ApplyCrimeModifiers(attempt);
        var result = _crimeService.AttemptCrime(modifiedAttempt, Player, PolicePressure, random ?? new Random());
        Player.Stats.ModifyEnergy(-result.EnergyCost);
        Player.Stats.ModifyStress(result.StressCost);
        LastCrimeDay = Clock.Day;

        if (result.Success)
        {
            Player.Stats.ModifyMoney(result.MoneyEarned);
            TotalCrimeEarnings += result.MoneyEarned;
            CrimesCommitted++;
            ApplySkillGain(SkillId.StreetSmarts);
            ModifyFactionReputation(FactionId.ImbabaCrew, 4);
            if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
            {
                ModifyFactionReputation(FactionId.ExPrisonerNetwork, 5);
            }
            if (!HasStoryFlag("crime_first_success"))
            {
                SetStoryFlag("crime_first_success");
                QueueNarrativeScene("crime_first_success");
            }
        }

        QueueContactCrimeScene(attempt, result);

        _crimeCommittedToday = true;
        SetPolicePressure(PolicePressure + result.PolicePressureDelta);
        RaiseEvent(result.Message);
        ApplyCrimeContactAftermath(result);

        if (PolicePressure >= 80 && !HasStoryFlag("crime_warning"))
        {
            SetStoryFlag("crime_warning");
            QueueNarrativeScene("crime_warning");
            RaiseEvent("People are whispering that the police are getting close.");
        }

        CheckGameOverConditions();
        return result;
    }

    public bool BuyFood()
    {
        if (Player.Stats.Money < RecurringExpenses.CheapFoodStockpile)
        {
            RaiseEvent($"Not enough money. Food costs {RecurringExpenses.CheapFoodStockpile} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-RecurringExpenses.CheapFoodStockpile);
        Player.Household.AddStaples(3);
        if (Player.BackgroundType == BackgroundType.SudaneseRefugee)
        {
            Player.Household.AddStaples(1);
            RaiseEvent("A Sudanese women-led kitchen stretches the bread run a little farther for you.");
        }

        RaiseEvent($"Bought food supplies for {RecurringExpenses.CheapFoodStockpile} LE. Stockpile: {Player.Household.FoodStockpile}");
        return true;
    }

    public bool BuyMedicine()
    {
        var medicineCost = GetMedicineCost();
        if (Player.Stats.Money < medicineCost)
        {
            RaiseEvent($"Not enough money. Medicine costs {medicineCost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-medicineCost);
        Player.Household.AddMedicine(2);
        ApplySkillGain(SkillId.Medical);
        RaiseEvent($"Bought medicine for {medicineCost} LE. Medicine stock: {Player.Household.MedicineStock}");
        return true;
    }

    public bool EatAtHome()
    {
        if (!Player.Household.FeedMother())
        {
            RaiseEvent("There is not enough food at home.");
            return false;
        }

        Player.Nutrition.Eat(MealQuality.Basic);
        SyncLegacyHunger();
        RaiseEvent("You eat a simple meal at home and make sure your mother eats too.");
        return true;
    }

    public bool EatStreetFood()
    {
        if (Player.Stats.Money < StreetFoodCost)
        {
            RaiseEvent("You do not have enough money for street food.");
            return false;
        }

        Player.Stats.ModifyMoney(-StreetFoodCost);
        Player.Nutrition.Eat(MealQuality.Basic);
        SyncLegacyHunger();
        RaiseEvent("You grab a cheap meal from the street.");
        return true;
    }

    public void CheckOnMother()
    {
        Player.Household.CheckOnMother();
        RaiseEvent(GetMotherStatusMessage());
    }

    public bool GiveMotherMedicine()
    {
        if (!Player.Household.GiveMedicine())
        {
            RaiseEvent("You have no medicine to give.");
            return false;
        }

        RaiseEvent("You give your mother her medicine.");
        return true;
    }

    public int GetMedicineCost()
    {
        return Player.Skills.GetLevel(SkillId.Medical) >= 3 ? 40 : RecurringExpenses.MedicineCost;
    }

    public IReadOnlyList<NpcId> GetReachableNpcs()
    {
        return NpcRegistry.GetReachableNpcs(World.CurrentLocationId, PolicePressure);
    }

    public void ModifyNpcTrust(NpcId npcId, int delta)
    {
        var adjustedDelta = delta;
        if (delta > 0 && Player.Skills.GetLevel(SkillId.Persuasion) >= 3)
        {
            adjustedDelta += 5;
        }

        var message = RelationshipService.ModifyTrust(Relationships, npcId, adjustedDelta, Clock.Day);
        if (!string.IsNullOrWhiteSpace(message))
        {
            RaiseEvent(message);
        }
    }

    public void ModifyFactionReputation(FactionId factionId, int delta)
    {
        var message = RelationshipService.ModifyReputation(Relationships, factionId, delta);
        if (!string.IsNullOrWhiteSpace(message))
        {
            RaiseEvent(message);
        }
    }

    public void AddEventMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            RaiseEvent(message);
        }
    }

    public void SetStoryFlag(string flag)
    {
        if (!string.IsNullOrWhiteSpace(flag))
        {
            _storyFlags.Add(flag);
        }
    }

    public bool HasStoryFlag(string flag)
    {
        return _storyFlags.Contains(flag);
    }

    public void RestoreStoryFlags(IEnumerable<string> flags)
    {
        ArgumentNullException.ThrowIfNull(flags);

        _storyFlags.Clear();
        foreach (var flag in flags.Where(static flag => !string.IsNullOrWhiteSpace(flag)))
        {
            _storyFlags.Add(flag);
        }
    }

    public void QueueNarrativeScene(string knotName)
    {
        if (!string.IsNullOrWhiteSpace(knotName))
        {
            _pendingNarrativeScenes.Enqueue(knotName);
        }
    }

    public bool TryDequeueNarrativeScene(out string knotName)
    {
        if (_pendingNarrativeScenes.Count > 0)
        {
            knotName = _pendingNarrativeScenes.Dequeue();
            return true;
        }

        knotName = string.Empty;
        return false;
    }

    public bool TryTakePendingEndingKnot(out string knotName)
    {
        if (!string.IsNullOrWhiteSpace(PendingEndingKnot))
        {
            knotName = PendingEndingKnot;
            PendingEndingKnot = null;
            return true;
        }

        knotName = string.Empty;
        return false;
    }

    public void SetPolicePressure(int value)
    {
        PolicePressure = Math.Clamp(value, 0, 100);
    }

    private void ApplyCrimeContactAftermath(CrimeResult result)
    {
        if (!result.Detected)
        {
            return;
        }

        if (World.CurrentLocationId == LocationId.Market && Relationships.GetNpcRelationship(NpcId.FenceHanan).Trust >= 15)
        {
            ReduceCrimeHeat(5, "Hanan quietly shifts attention away from your name. The market heat eases a little.", "crime_hanan_cover", "crime_hanan_cover_seen");

            if (!result.Success)
            {
                ApplyCrimeFailureMitigation(
                    moneyGain: 12,
                    stressRelief: 4,
                    message: "Hanan still manages to move a sliver of the loss. The night hurts less than it should have.",
                    knotName: "crime_hanan_salvage",
                    flagName: "crime_hanan_salvage_seen");
            }
        }

        if (World.CurrentLocationId == LocationId.Square && Relationships.GetNpcRelationship(NpcId.RunnerYoussef).Trust >= 15)
        {
            ReduceCrimeHeat(7, "Youssef tips you off and sends you moving before the wrong questions settle on you.", "crime_youssef_tipoff", "crime_youssef_tipoff_seen");

            if (!result.Success)
            {
                ApplyCrimeFailureMitigation(
                    moneyGain: 0,
                    stressRelief: 6,
                    message: "Youssef gets you clear before panic turns into a worse mistake.",
                    knotName: "crime_youssef_escape",
                    flagName: "crime_youssef_escape_seen");
            }
        }
    }

    private void QueueContactCrimeScene(CrimeAttempt attempt, CrimeResult result)
    {
        var scene = attempt.Type switch
        {
            CrimeType.MarketFencing => GetCrimeScene(result, "crime_hanan_fence_success", "crime_hanan_fence_detected", "crime_hanan_fence_failure"),
            CrimeType.DokkiDrop => GetCrimeScene(result, "crime_youssef_drop_success", "crime_youssef_drop_detected", "crime_youssef_drop_failure"),
            CrimeType.NetworkErrand => GetCrimeScene(result, "crime_ummkarim_errand_success", "crime_ummkarim_errand_detected", "crime_ummkarim_errand_failure"),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(scene))
        {
            return;
        }

        var flagName = $"{scene}_seen";
        if (HasStoryFlag(flagName))
        {
            return;
        }

        SetStoryFlag(flagName);
        QueueNarrativeScene(scene);
    }

    private static string GetCrimeScene(CrimeResult result, string successScene, string detectedSuccessScene, string failureScene)
    {
        if (result.Success && result.Detected)
        {
            return detectedSuccessScene;
        }

        if (result.Success)
        {
            return successScene;
        }

        return failureScene;
    }

    private void ReduceCrimeHeat(int amount, string message, string knotName, string flagName)
    {
        if (amount <= 0)
        {
            return;
        }

        var updatedPressure = Math.Max(0, PolicePressure - amount);
        if (updatedPressure == PolicePressure)
        {
            return;
        }

        SetPolicePressure(updatedPressure);
        RaiseEvent(message);

        if (!HasStoryFlag(flagName))
        {
            SetStoryFlag(flagName);
            QueueNarrativeScene(knotName);
        }
    }

    private void ApplyCrimeFailureMitigation(int moneyGain, int stressRelief, string message, string knotName, string flagName)
    {
        if (moneyGain > 0)
        {
            Player.Stats.ModifyMoney(moneyGain);
        }

        if (stressRelief > 0)
        {
            Player.Stats.ModifyStress(-stressRelief);
        }

        RaiseEvent(message);

        if (!HasStoryFlag(flagName))
        {
            SetStoryFlag(flagName);
            QueueNarrativeScene(knotName);
        }
    }

    public void SetRunId(Guid runId)
    {
        RunId = runId;
    }

    public void SetDaysSurvived(int daysSurvived)
    {
        DaysSurvived = Math.Max(0, daysSurvived);
    }

    public void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted)
    {
        TotalCrimeEarnings = Math.Max(0, totalCrimeEarnings);
        CrimesCommitted = Math.Max(0, crimesCommitted);
    }

    public void SetWorkCounters(int totalHonestWorkEarnings, int honestShiftsCompleted, int lastCrimeDay, int lastHonestWorkDay, int lastPublicFacingWorkDay)
    {
        TotalHonestWorkEarnings = Math.Max(0, totalHonestWorkEarnings);
        HonestShiftsCompleted = Math.Max(0, honestShiftsCompleted);
        LastCrimeDay = Math.Max(0, lastCrimeDay);
        LastHonestWorkDay = Math.Max(0, lastHonestWorkDay);
        LastPublicFacingWorkDay = Math.Max(0, lastPublicFacingWorkDay);
    }

    public void RecordEventHistory(string eventId, int count)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        _randomEventHistory[eventId] = Math.Max(0, count);
    }

    public int GetEventCount(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return 0;
        }

        return _randomEventHistory.GetValueOrDefault(eventId);
    }

    public void RestoreJobTrack(JobType jobType, int reliability, int shiftsCompleted, int lockoutUntilDay)
    {
        JobProgress.RestoreTrack(jobType, reliability, shiftsCompleted, lockoutUntilDay);
    }

    private void CheckGameOverConditions()
    {
        var ending = EndingService.CheckEndings(this);
        if (ending is null)
        {
            return;
        }

        EndingId = ending;
        IsGameOver = true;
        GameOverReason = EndingService.GetMessage(ending.Value);
        PendingEndingKnot = EndingService.GetInkKnot(ending.Value);
    }

    private void ModifyEmployerTrust(JobType jobType, int delta)
    {
        var npcId = jobType switch
        {
            JobType.ClinicReception => NpcId.NurseSalma,
            JobType.WorkshopSewing => NpcId.WorkshopBossAbuSamir,
            JobType.CafeService => NpcId.CafeOwnerNadia,
            _ => (NpcId?)null
        };

        if (npcId.HasValue)
        {
            ModifyNpcTrust(npcId.Value, delta);
        }
    }

    private void ApplyWorkCrimeSpillover(JobShift job, JobResult result)
    {
        if (Clock.Day - LastCrimeDay > 1 || LastCrimeDay == 0)
        {
            return;
        }

        if (PolicePressure >= 60 && IsPublicFacingJob(job.Type))
        {
            Player.Stats.ModifyStress(4);
            ModifyEmployerTrust(job.Type, -2);
            RaiseEvent("The street heat follows you into work. People notice how tense you look.");
        }

        if (result.MistakeMade && job.Type == JobType.WorkshopSewing)
        {
            Relationships.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
            Relationships.RecordRefusal(NpcId.WorkshopBossAbuSamir, Clock.Day);
        }
    }

    private void ApplyBackgroundWorkFlavor(JobShift job, JobResult result)
    {
        if (Player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
            job.Type == JobType.ClinicReception &&
            result.Success &&
            !HasStoryFlag("background_medical_clinic_seen"))
        {
            SetStoryFlag("background_medical_clinic_seen");
            QueueNarrativeScene("background_medical_clinic");
        }

        if (Player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
            job.Type == JobType.ClinicReception &&
            result.Success &&
            Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 12 &&
            Player.Household.MotherHealth < 65)
        {
            Relationships.RecordFavor(NpcId.NurseSalma, Clock.Day, hasUnpaidDebt: true);
            RaiseEvent("Nurse Salma quietly covers a little medicine for your mother. You owe her now.");
        }
    }

    private CrimeAttempt ApplyCrimeModifiers(CrimeAttempt attempt)
    {
        var modifiedAttempt = attempt;

        if (LastPublicFacingWorkDay == Clock.Day)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Max(5, modifiedAttempt.DetectionRisk - 8),
                PolicePressureIncrease = Math.Max(1, modifiedAttempt.PolicePressureIncrease - 4)
            };

            RaiseEvent("The shift you worked today gives you a thin alibi and a cleaner reason to be seen moving.");
        }

        if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Min(95, modifiedAttempt.DetectionRisk + 5),
                PolicePressureIncrease = modifiedAttempt.PolicePressureIncrease + 5
            };

            if (!HasStoryFlag("background_prisoner_heat_seen"))
            {
                SetStoryFlag("background_prisoner_heat_seen");
                QueueNarrativeScene("background_prisoner_heat");
            }
        }

        return modifiedAttempt;
    }

    private static bool IsPublicFacingJob(JobType jobType)
    {
        return jobType is JobType.CallCenterWork or JobType.ClinicReception or JobType.CafeService;
    }

    private void ApplyRandomEvent(RandomEvent randomEvent)
    {
        ArgumentNullException.ThrowIfNull(randomEvent);

        RecordEventHistory(randomEvent.Id, GetEventCount(randomEvent.Id) + 1);

        var effect = randomEvent.Effect;
        if (effect.MoneyChange != 0)
        {
            Player.Stats.ModifyMoney(effect.MoneyChange);
        }

        if (effect.HealthChange != 0)
        {
            Player.Stats.ModifyHealth(effect.HealthChange);
        }

        if (effect.EnergyChange != 0)
        {
            Player.Stats.ModifyEnergy(effect.EnergyChange);
        }

        if (effect.HungerChange != 0)
        {
            Player.Nutrition.ModifySatiety(effect.HungerChange);
            SyncLegacyHunger();
        }

        if (effect.StressChange != 0)
        {
            Player.Stats.ModifyStress(effect.StressChange);
        }

        if (effect.PolicePressureChange != 0)
        {
            SetPolicePressure(PolicePressure + effect.PolicePressureChange);
        }

        if (effect.MotherHealthChange != 0)
        {
            Player.Household.UpdateMotherHealth(effect.MotherHealthChange);
        }

        if (effect.FoodChange > 0)
        {
            Player.Household.AddFood(effect.FoodChange);
        }
        else if (effect.FoodChange < 0)
        {
            for (var i = 0; i < -effect.FoodChange; i++)
            {
                Player.Household.ConsumeFood();
            }
        }

        RaiseEvent(randomEvent.Description);

        if (Player.BackgroundType == BackgroundType.SudaneseRefugee &&
            randomEvent.Id == "NeighborhoodSolidarity" &&
            !HasStoryFlag("background_sudanese_solidarity_seen"))
        {
            SetStoryFlag("background_sudanese_solidarity_seen");
            QueueNarrativeScene("background_sudanese_solidarity");
        }

        if (!string.IsNullOrWhiteSpace(effect.InkKnot))
        {
            QueueNarrativeScene(effect.InkKnot);
        }
    }

    private void ApplySkillGain(SkillId skillId)
    {
        if (SkillService.ApplySkillGain(skillId, this, out var newLevel))
        {
            RaiseEvent($"{skillId} improves to {newLevel}.");
        }
    }

    private void SyncLegacyHunger()
    {
        Player.Stats.SetHunger(Player.Nutrition.Satiety);
    }

    private string GetMotherStatusMessage()
    {
        return Player.Household.MotherCondition switch
        {
            MotherCondition.Stable => "Your mother seems stable today.",
            MotherCondition.Fragile => "Your mother looks fragile and needs attention.",
            MotherCondition.Crisis => "Your mother is in crisis. She needs care immediately.",
            _ => "You check on your mother."
        };
    }

    private static SkillId GetSkillForJob(JobType jobType)
    {
        return jobType switch
        {
            JobType.BakeryWork => SkillId.Physical,
            JobType.HouseCleaning => SkillId.Physical,
            JobType.CallCenterWork => SkillId.Persuasion,
            _ => SkillId.StreetSmarts
        };
    }

    private void RaiseEvent(string message)
    {
        GameEvent?.Invoke(this, new GameEventArgs(message));
    }

    public IReadOnlyList<string> GetStatusSummary()
    {
        return
        [
            $"Day {Clock.Day} - {Clock.TimeOfDay}",
            $"Time: {Clock.Hour:D2}:{Clock.Minute:D2}",
            $"Money: {Player.Stats.Money} LE",
            $"Hunger: {Player.Nutrition.Satiety}%",
            $"Energy: {Player.Stats.Energy}%",
            $"Health: {Player.Stats.Health}%",
            $"Stress: {Player.Stats.Stress}%",
            $"Location: {World.GetCurrentLocation()?.Name ?? "Unknown"}"
        ];
    }
}

public sealed class GameEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}
