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
        Player.Household.ApplyDailyDecay();

        if (Player.Stats.Money >= RecurringExpenses.DailyRentCost)
        {
            Player.Stats.ModifyMoney(-RecurringExpenses.DailyRentCost);
            RaiseEvent($"Paid rent: {RecurringExpenses.DailyRentCost} LE");
        }
        else
        {
            RaiseEvent("Could not pay rent! The landlord is angry.");
        }

        if (Player.Household.HasEnoughFood)
        {
            Player.Household.ConsumeFood();
            Player.Stats.Eat(20);
        }
        else
        {
            Player.Stats.ModifyHunger(-10);
            RaiseEvent("No food at home. Your family goes hungry.");
        }

        if (!_crimeCommittedToday && PolicePressure > 0)
        {
            SetPolicePressure(PolicePressure - 5);
        }

        Clock.AdvanceToNextDay();
        DaysSurvived++;
        World.TravelTo(LocationId.Home);
        RaiseEvent("You return home for the night.");

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

        _crimeCommittedToday = true;
        SetPolicePressure(PolicePressure + result.PolicePressureDelta);
        RaiseEvent(result.Message);

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
        Player.Household.AddFood(3);
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
        Player.Household.UpdateMotherHealth(30);
        ApplySkillGain(SkillId.Medical);
        RaiseEvent($"Bought medicine for {medicineCost} LE. Mother's health improved.");
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
            Player.Stats.ModifyHunger(effect.HungerChange);
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
            $"Hunger: {Player.Stats.Hunger}%",
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
