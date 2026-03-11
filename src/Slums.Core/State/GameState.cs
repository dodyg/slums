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
    private bool _crimeCommittedToday;

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public GameClock Clock { get; } = new();
    public PlayerCharacter Player { get; } = new();
    public WorldState World { get; } = new();
    public RelationshipState Relationships { get; } = new();
    public JobService Jobs { get; } = new();
    public bool IsGameOver { get; private set; }
    public string? GameOverReason { get; private set; }
    public EndingId? EndingId { get; private set; }
    public int PolicePressure { get; private set; }
    public int TotalCrimeEarnings { get; private set; }
    public int CrimesCommitted { get; private set; }
    public int DaysSurvived { get; private set; }
    public IReadOnlyCollection<string> StoryFlags => _storyFlags;
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
            SetPolicePressure(PolicePressure - 5);
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

        if (Player.Stats.Money < RecurringExpenses.TravelCost)
        {
            RaiseEvent("Not enough money for transport.");
            return false;
        }

        Player.Stats.ModifyMoney(-RecurringExpenses.TravelCost);
        Player.Stats.ModifyEnergy(-5);
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

        var result = Jobs.PerformJob(job, Player, location);
        
        if (result.Success)
        {
            TotalCrimeEarnings += 0;
            AdvanceTime(job.DurationMinutes);
            ApplySkillGain(GetSkillForJob(job.Type));
            RaiseEvent(result.Message);
        }
        else
        {
            RaiseEvent(result.Message);
        }

        CheckGameOverConditions();
        return result;
    }

    public IReadOnlyList<CrimeAttempt> GetAvailableCrimes()
    {
        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        return CrimeRegistry.GetAvailableCrimes(location, Relationships);
    }

    public CrimeResult CommitCrime(CrimeAttempt attempt, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var result = _crimeService.AttemptCrime(attempt, Player, PolicePressure, random ?? new Random());
        Player.Stats.ModifyEnergy(-result.EnergyCost);
        Player.Stats.ModifyStress(result.StressCost);

        if (result.Success)
        {
            Player.Stats.ModifyMoney(result.MoneyEarned);
            TotalCrimeEarnings += result.MoneyEarned;
            CrimesCommitted++;
            ApplySkillGain(SkillId.StreetSmarts);
            ModifyFactionReputation(FactionId.ImbabaCrew, 4);
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

    private void ApplyRandomEvent(RandomEvent randomEvent)
    {
        ArgumentNullException.ThrowIfNull(randomEvent);

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
