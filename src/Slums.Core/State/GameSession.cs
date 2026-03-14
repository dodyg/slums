using System.Globalization;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Crimes;
using Slums.Core.Endings;
using Slums.Core.Entertainment;
using Slums.Core.Expenses;
using Slums.Core.Events;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;
using EntitiesDb;
using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

namespace Slums.Core.State;

public sealed class GameSession : IDisposable, INarrativeOutcomeTarget
{
    private const int EndOfDayHour = 22;
    private readonly EntityDatabase _database;
    private readonly CrimeService _crimeService = new();
    private readonly RandomEventService _randomEventService = new();
    private readonly PlayerIdentityState _playerIdentity;
    private readonly GameRunState _runState;
    private readonly GameCrimeState _crimeState;
    private readonly GameWorkState _workState;
    private readonly GameNarrativeState _narrativeState;
    private readonly GameInvestmentState _investmentState;
    private readonly RentState _rentState;
    private readonly Random _sharedRandom;
    private readonly LocationPricingService _locationPricingService;
    private readonly Queue<string> _pendingNarrativeScenes;
    private readonly HashSet<string> _storyFlags;
    private readonly Dictionary<string, int> _randomEventHistory;
    private readonly bool _useDynamicDistrictConditions;

    public GameSession(Random? sharedRandom = null)
    {
        Clock = new GameClock();
        _playerIdentity = new PlayerIdentityState();
        Player = new PlayerCharacter(_playerIdentity, new SurvivalStats(), new NutritionState(), new HouseholdCareState(), new SkillState());
        World = new WorldState();
        Relationships = new RelationshipState();
        JobProgress = new JobProgressState();
        Jobs = new JobService();
        _runState = new GameRunState();
        _crimeState = new GameCrimeState();
        _workState = new GameWorkState();
        _narrativeState = new GameNarrativeState();
        _investmentState = new GameInvestmentState();
        _rentState = new RentState();
        _useDynamicDistrictConditions = sharedRandom is not null;
        _sharedRandom = sharedRandom ?? new Random();
        _locationPricingService = new LocationPricingService();
        _pendingNarrativeScenes = _narrativeState.PendingNarrativeScenes;
        _storyFlags = _narrativeState.StoryFlags;
        _randomEventHistory = _narrativeState.RandomEventHistory;
        _database = new EntityDatabase(new EntityDatabaseOptions(16384, int.MaxValue, -1));
        _database.Create(_playerIdentity, Player.Stats, Player.Nutrition, Player.Household, Player.Skills);
        _database.Create(Clock, World);
        _database.Create(Relationships, JobProgress);
        _database.Create(_crimeState);
        _database.Create(_workState);
        _database.Create(_runState, _narrativeState);
        if (_useDynamicDistrictConditions)
        {
            RollDistrictConditionsForCurrentDay(_sharedRandom);
        }
        else
        {
            SetBaselineDistrictConditions();
        }
    }

    public Guid RunId { get => _runState.RunId; private set => _runState.RunId = value; }
    public GameClock Clock { get; }
    public PlayerCharacter Player { get; }
    public WorldState World { get; }
    public RelationshipState Relationships { get; }
    public JobProgressState JobProgress { get; }
    public JobService Jobs { get; }
    public bool IsGameOver { get => _runState.IsGameOver; private set => _runState.IsGameOver = value; }
    public string? GameOverReason { get => _runState.GameOverReason; private set => _runState.GameOverReason = value; }
    public EndingId? EndingId { get => _runState.EndingId; private set => _runState.EndingId = value; }
    public int PolicePressure { get => _crimeState.PolicePressure; private set => _crimeState.PolicePressure = value; }
    public int TotalCrimeEarnings { get => _crimeState.TotalCrimeEarnings; private set => _crimeState.TotalCrimeEarnings = value; }
    public int CrimesCommitted { get => _crimeState.CrimesCommitted; private set => _crimeState.CrimesCommitted = value; }
    public int TotalHonestWorkEarnings { get => _workState.TotalHonestWorkEarnings; private set => _workState.TotalHonestWorkEarnings = value; }
    public int HonestShiftsCompleted { get => _workState.HonestShiftsCompleted; private set => _workState.HonestShiftsCompleted = value; }
    public int DaysSurvived { get => _runState.DaysSurvived; private set => _runState.DaysSurvived = value; }
    public int LastCrimeDay { get => _crimeState.LastCrimeDay; private set => _crimeState.LastCrimeDay = value; }
    public int LastHonestWorkDay { get => _workState.LastHonestWorkDay; private set => _workState.LastHonestWorkDay = value; }
    public int LastPublicFacingWorkDay { get => _workState.LastPublicFacingWorkDay; private set => _workState.LastPublicFacingWorkDay = value; }
    public IReadOnlyCollection<string> StoryFlags => _storyFlags;
    public IReadOnlyDictionary<string, int> RandomEventHistory => _randomEventHistory;
    public bool HasCrimeCommittedToday => CrimeCommittedToday;
    public string? PendingEndingKnot { get => _runState.PendingEndingKnot; private set => _runState.PendingEndingKnot = value; }
    public IReadOnlyList<Investment> ActiveInvestments => _investmentState.ActiveInvestments;
    public int TotalInvestmentEarnings { get => _investmentState.TotalInvestmentEarnings; private set => _investmentState.TotalInvestmentEarnings = value; }
    public int UnpaidRentDays => _rentState.UnpaidRentDays;
    public int AccumulatedRentDebt => _rentState.AccumulatedRentDebt;
    public bool FirstWarningGiven => _rentState.FirstWarningGiven;
    public bool FinalWarningGiven => _rentState.FinalWarningGiven;
    private bool CrimeCommittedToday { get => _crimeState.CrimeCommittedToday; set => _crimeState.CrimeCommittedToday = value; }

    public event EventHandler<GameEventArgs>? GameEvent;

    public IReadOnlyList<InvestmentDefinition> GetCurrentInvestmentOpportunities()
    {
        var reachableNpcs = GetReachableNpcs().ToHashSet();
        var ownedTypes = _investmentState.ActiveInvestments.Select(static investment => investment.Type).ToHashSet();
        var opportunities = new List<InvestmentDefinition>();

        foreach (var definition in InvestmentRegistry.AllDefinitions)
        {
            if (ownedTypes.Contains(definition.Type))
            {
                continue;
            }

            if (definition.OpportunityLocationId != World.CurrentLocationId)
            {
                continue;
            }

            if (definition.OpportunityNpc is NpcId sponsorNpc && !reachableNpcs.Contains(sponsorNpc))
            {
                continue;
            }

            opportunities.Add(definition);
        }

        return opportunities;
    }

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

        var rentResult = _rentState.ProcessDay(RecurringExpenses.DailyRentCost, Player.Stats.Money);
        if (rentResult.Paid)
        {
            Player.Stats.ModifyMoney(-RecurringExpenses.DailyRentCost);
            RaiseEvent($"Paid rent: {RecurringExpenses.DailyRentCost} LE");
        }
        else
        {
            RaiseEvent($"Could not pay rent! Debt: {rentResult.AccumulatedDebt} LE. Unpaid days: {rentResult.CurrentUnpaidDays}.");

            if (rentResult.WarningType == RentWarningType.First)
            {
                RaiseEvent("The landlord's son knocks hard. \"Three days now. My father is patient, but not forever.\"");
            }
            else if (rentResult.WarningType == RentWarningType.Final)
            {
                RaiseEvent("The landlord himself appears. \"Five days. Two more and we put your things on the street.\"");
                TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.EventRentFinalWarningSeen, NarrativeKnots.EventRentFinalWarning));
            }
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

        if (!CrimeCommittedToday && PolicePressure > 0)
        {
            var pressureDecay = ActivityLedgerSystem.GetDailyPolicePressureDecay(Player.BackgroundType);
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
        if (_useDynamicDistrictConditions)
        {
            RollDistrictConditionsForCurrentDay(random ?? _sharedRandom);
        }
        else
        {
            SetBaselineDistrictConditions();
        }
        RaiseEvent("You return home for the night.");

        if (GetCurrentDayOfWeek() == DayOfWeek.Monday && _investmentState.ActiveInvestments.Count > 0)
        {
            ResolveWeeklyInvestments(random ?? _sharedRandom);
        }

        Player.Nutrition.BeginNewDay();
        Player.Household.BeginNewDay();

        foreach (var randomEvent in _randomEventService.RollDailyEvents(this, random ?? _sharedRandom))
        {
            ApplyRandomEvent(randomEvent);
        }

        QueueNarrativeFollowUpScenes();

        ActivityLedgerSystem.BeginNewDay(_crimeState);
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

        var travelCost = GetTravelCost(location);
        var travelEnergyCost = GetTravelEnergyCost(location);

        if (Player.Stats.Money < travelCost)
        {
            RaiseEvent("Not enough money for transport.");
            return false;
        }

        Player.Stats.ModifyMoney(-travelCost);
        Player.Stats.ModifyEnergy(-travelEnergyCost);
        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && location.District == DistrictId.Dokki)
        {
            Player.Stats.ModifyStress(2);
            RaiseEvent("Dokki's questions land harder when your accent gets there before your name does.");
        }

        if (location.District == DistrictId.BulaqAlDakrour && Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 12)
        {
            RaiseEvent("Safaa's route advice spares you one bad transfer and some wasted motion.");
        }

        if (location.District == DistrictId.Shubra && Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 12)
        {
            Player.Stats.ModifyStress(-1);
            RaiseEvent("Iman's directions keep you off the most exhausting side streets in Shubra.");
        }

        AdvanceTime(GetTravelTimeMinutes(location));
        World.TravelTo(locationId);

        RaiseEvent($"Traveled to {location.Name}.");
        return true;
    }

    public bool TryWalkTo(LocationId locationId)
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

        var walkEnergyCost = GetWalkEnergyCost(location);
        var walkTimeMinutes = GetWalkTimeMinutes(location);

        if (Player.Stats.Energy < walkEnergyCost)
        {
            RaiseEvent("You are too exhausted to walk that far.");
            return false;
        }

        Player.Stats.ModifyEnergy(-walkEnergyCost);
        Player.Stats.ModifyStress(3);

        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && location.District == DistrictId.Dokki)
        {
            Player.Stats.ModifyStress(2);
            RaiseEvent("Dokki's stares follow you the entire way on foot.");
        }

        AdvanceTime(walkTimeMinutes);
        World.TravelTo(locationId);

        RaiseEvent($"Walked to {location.Name}. The streets took their toll.");
        return true;
    }

    public bool CanAffordTravel(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(l => l.Id == locationId);
        if (location is null)
        {
            return false;
        }

        return Player.Stats.Money >= GetTravelCost(location);
    }

    private int GetWalkEnergyCost(Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        return GetTravelEnergyCost(destination) * 3;
    }

    private int GetWalkTimeMinutes(Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        return GetTravelTimeMinutes(destination) * 3;
    }

    public IReadOnlyList<EntertainmentActivity> GetAvailableEntertainmentActivities()
    {
        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return [];
        }

        return EntertainmentRegistry.GetActivitiesForLocation(
            location.HasCafe,
            location.HasBar,
            location.HasBilliards).ToArray();
    }

    public bool TryPerformEntertainment(EntertainmentActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (Player.Stats.Money < activity.BaseCost)
        {
            RaiseEvent($"You cannot afford {activity.Name} right now.");
            return false;
        }

        if (Player.Stats.Energy < activity.EnergyCost)
        {
            RaiseEvent($"You are too tired for {activity.Name}.");
            return false;
        }

        var location = World.GetCurrentLocation();
        if (location is null)
        {
            RaiseEvent("You are nowhere.");
            return false;
        }

        var availableActivities = GetAvailableEntertainmentActivities();
        if (!availableActivities.Contains(activity))
        {
            RaiseEvent($"{activity.Name} is not available here.");
            return false;
        }

        Player.Stats.ModifyMoney(-activity.BaseCost);
        Player.Stats.ModifyStress(-activity.StressReduction);
        if (activity.EnergyCost > 0)
        {
            Player.Stats.ModifyEnergy(-activity.EnergyCost);
        }

        AdvanceTime(activity.DurationMinutes);

        RaiseEvent(GetEntertainmentFlavorMessage(activity));
        return true;
    }

    private static string GetEntertainmentFlavorMessage(EntertainmentActivity activity)
    {
        return activity.Type switch
        {
            EntertainmentActivityType.Coffee => "The coffee is strong and bitter. You feel a little lighter.",
            EntertainmentActivityType.Shisha => "Apple smoke curls around you. The afternoon drifts by.",
            EntertainmentActivityType.Billiards => "You win some, you lose some. The company is good.",
            EntertainmentActivityType.BarDrinking => "The drink burns going down. For a while, things feel far away.",
            EntertainmentActivityType.FootballWatching => "The crowd screams at the TV. You scream with them.",
            EntertainmentActivityType.SocialHangout => "Just talking. Just listening. It helps.",
            _ => $"You spent some time on {activity.Name}."
        };
    }

    public JobResult WorkJob(JobShift job, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(job);

        var location = World.GetCurrentLocation();
        if (location is null)
        {
            return JobResult.Failed("You are nowhere.");
        }

        var result = Jobs.PerformJob(job, Player, location, Relationships, JobProgress, Clock.Day, random ?? _sharedRandom);

        if (result.Success)
        {
            ActivityLedgerSystem.RecordWorkShift(_workState, Clock, job, result);
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

        return Jobs.GetAvailableJobs(location, Player, Relationships, JobProgress)
            .Select(ApplyDistrictConditionToJob)
            .ToArray();
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

        if (location.Id == LocationId.Depot &&
            crimes.All(static attempt => attempt.Type != CrimeType.DepotFareSkim) &&
            JobProgress.GetTrack(JobType.MicrobusDispatch).Reliability >= 60)
        {
            crimes.Add(new CrimeAttempt(CrimeType.DepotFareSkim, 78, 28, 14, 0, 16));
        }

        if (location.Id == LocationId.Laundry &&
            crimes.All(static attempt => attempt.Type != CrimeType.ShubraBundleLift) &&
            JobProgress.GetTrack(JobType.LaundryPressing).Reliability >= 60)
        {
            crimes.Add(new CrimeAttempt(CrimeType.ShubraBundleLift, 68, 24, 12, 0, 15));
        }

        return crimes;
    }

    public CrimeResult CommitCrime(CrimeAttempt attempt, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var modifierEvaluation = EvaluateCrimeModifiers(attempt);
        var modifiedAttempt = modifierEvaluation.Attempt;
        ApplyCrimeModifierSideEffects(modifierEvaluation.ActiveModifiers);
        var result = _crimeService.AttemptCrime(modifiedAttempt, Player, PolicePressure, random ?? _sharedRandom);
        Player.Stats.ModifyEnergy(-result.EnergyCost);
        Player.Stats.ModifyStress(result.StressCost);
        ActivityLedgerSystem.RecordCrimeOutcome(_crimeState, Clock, result);

        if (result.Success)
        {
            Player.Stats.ModifyMoney(result.MoneyEarned);
            ApplySkillGain(SkillId.StreetSmarts);
            ModifyFactionReputation(FactionId.ImbabaCrew, 4);
            if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
            {
                ModifyFactionReputation(FactionId.ExPrisonerNetwork, 5);
            }
            TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetFirstSuccessTrigger(_storyFlags));
        }

        TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetRouteSceneTrigger(attempt.Type, result));

        SetPolicePressure(PolicePressure + result.PolicePressureDelta);
        RaiseEvent(result.Message);
        ApplyCrimeContactAftermath(result);

        if (TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetCrimeWarningTrigger(PolicePressure, _storyFlags)))
        {
            RaiseEvent("People are whispering that the police are getting close.");
        }

        CheckGameOverConditions();
        return result;
    }

    public bool BuyFood()
    {
        var foodCost = GetFoodCost();
        if (Player.Stats.Money < foodCost)
        {
            RaiseEvent($"Not enough money. Food costs {foodCost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-foodCost);
        Player.Household.AddStaples(3);
        if (Player.BackgroundType == BackgroundType.SudaneseRefugee)
        {
            Player.Household.AddStaples(1);
            RaiseEvent("A Sudanese women-led kitchen stretches the bread run a little farther for you.");
        }

        RaiseEvent($"Bought food supplies for {foodCost} LE in {DistrictInfo.GetName(World.CurrentDistrict)}. Stockpile: {Player.Household.FoodStockpile}");
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
        var streetFoodCost = GetStreetFoodCost();
        if (Player.Stats.Money < streetFoodCost)
        {
            RaiseEvent($"You do not have enough money for street food. It costs {streetFoodCost} LE here.");
            return false;
        }

        Player.Stats.ModifyMoney(-streetFoodCost);
        Player.Nutrition.Eat(MealQuality.Basic);
        SyncLegacyHunger();
        RaiseEvent($"You grab a cheap meal from the street for {streetFoodCost} LE.");
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

    public MotherClinicVisitResult TakeMotherToClinic()
    {
        var clinicStatus = GetCurrentLocationClinicStatus();
        if (!clinicStatus.HasClinicServices)
        {
            RaiseEvent("There is no clinic service at this location.");
            return new MotherClinicVisitResult(false, 0, 0);
        }

        if (!clinicStatus.IsOpenToday)
        {
            RaiseEvent($"{clinicStatus.LocationName} is closed today. Open days: {clinicStatus.OpenDaysSummary}.");
            return new MotherClinicVisitResult(false, clinicStatus.VisitCost, 0);
        }

        if (Player.Stats.Money < clinicStatus.VisitCost)
        {
            RaiseEvent($"Not enough money. A clinic visit costs {clinicStatus.VisitCost} LE here.");
            return new MotherClinicVisitResult(false, clinicStatus.VisitCost, 0);
        }

        var healthBonus = 0;
        if (Player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            healthBonus += 5;
        }

        if (World.CurrentLocationId == LocationId.Clinic && Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 15)
        {
            healthBonus += 3;
        }

        if (World.CurrentLocationId == LocationId.Pharmacy && Relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 12)
        {
            healthBonus += 2;
        }

        var healthChange = Math.Clamp(15 + healthBonus, 0, 100 - Player.Household.MotherHealth);

        Player.Stats.ModifyMoney(-clinicStatus.VisitCost);
        Player.Household.UpdateMotherHealth(healthChange);
        Player.Stats.ModifyEnergy(-10);
        AdvanceTime(90);
        ApplySkillGain(SkillId.Medical);

        RaiseEvent($"You take your mother into {clinicStatus.LocationName}. The visit costs {clinicStatus.VisitCost} LE. Her health improves by {healthChange}.");
        if (NarrativeSignalRules.HasPendingClinicFirstVisit(_storyFlags))
        {
            TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.MotherClinicFirstVisit, NarrativeKnots.MotherClinicFirstVisit));
        }

        return new MotherClinicVisitResult(true, clinicStatus.VisitCost, healthChange);
    }

#pragma warning disable CA1024
    public int GetFoodCost()
    {
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        var modifiedCost = _locationPricingService.GetFoodCost(World.CurrentDistrict) + (districtCondition?.Effect.FoodCostModifier ?? 0);
        return Math.Max(1, modifiedCost);
    }

    public int GetStreetFoodCost()
    {
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        var modifiedCost = _locationPricingService.GetStreetFoodCost(World.CurrentDistrict) + (districtCondition?.Effect.StreetFoodCostModifier ?? 0);
        return Math.Max(1, modifiedCost);
    }

    public CurrentLocationClinicStatus GetCurrentLocationClinicStatus()
    {
        var location = World.GetCurrentLocation();
        var currentDay = GetCurrentDayOfWeek();
        var currentDayName = currentDay.ToString();

        if (location is null || !location.HasClinicServices)
        {
            return new CurrentLocationClinicStatus(
                HasClinicServices: false,
                IsOpenToday: false,
                VisitCost: 0,
                LocationName: location?.Name ?? "Unknown",
                CurrentDayName: currentDayName,
                OpenDaysSummary: "No clinic here");
        }

        return new CurrentLocationClinicStatus(
            HasClinicServices: true,
            IsOpenToday: location.ClinicOpenDays.Contains(currentDay),
            VisitCost: GetClinicVisitCost(location),
            LocationName: location.Name,
            CurrentDayName: currentDayName,
            OpenDaysSummary: FormatOpenDays(location.ClinicOpenDays));
    }
#pragma warning restore CA1024

#pragma warning disable CA1822
    public IReadOnlyList<Location> GetClinicLocations()
#pragma warning restore CA1822
    {
        return WorldState.AllLocations
            .Where(l => l.HasClinicServices)
            .ToList();
    }

    public ClinicTravelOption GetClinicTravelOption(LocationId clinicLocationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(l => l.Id == clinicLocationId);
        if (location is null || !location.HasClinicServices)
        {
            return new ClinicTravelOption(
                LocationId: clinicLocationId,
                LocationName: "Unknown",
                DistrictName: "Unknown",
                TravelCost: 0,
                ClinicCost: 0,
                TotalCost: 0,
                IsOpenToday: false,
                OpenDaysSummary: "No clinic at this location",
                TravelTimeMinutes: 0,
                CanAfford: false,
                IsValidOption: false);
        }

        var travelCost = GetTravelCost(location);
        var clinicCost = GetClinicVisitCost(location);
        var totalCost = travelCost + clinicCost;
        var currentDay = GetCurrentDayOfWeek();

        return new ClinicTravelOption(
            LocationId: clinicLocationId,
            LocationName: location.Name,
            DistrictName: location.District.ToString(),
            TravelCost: travelCost,
            ClinicCost: clinicCost,
            TotalCost: totalCost,
            IsOpenToday: location.ClinicOpenDays.Contains(currentDay),
            OpenDaysSummary: FormatOpenDays(location.ClinicOpenDays),
            TravelTimeMinutes: GetTravelTimeMinutes(location),
            CanAfford: Player.Stats.Money >= totalCost,
            IsValidOption: true);
    }

    public TravelAndClinicVisitResult TravelAndTakeMotherToClinic(LocationId clinicLocationId)
    {
        var option = GetClinicTravelOption(clinicLocationId);
        if (!option.IsValidOption)
        {
            RaiseEvent("There is no clinic service at that location.");
            return new TravelAndClinicVisitResult(false, 0, 0, 0, 0);
        }

        if (!option.IsOpenToday)
        {
            RaiseEvent($"{option.LocationName} is closed today. Open days: {option.OpenDaysSummary}.");
            return new TravelAndClinicVisitResult(false, option.TravelCost, option.ClinicCost, option.TotalCost, 0);
        }

        if (Player.Stats.Money < option.TotalCost)
        {
            RaiseEvent($"Not enough money. Travel + clinic visit costs {option.TotalCost} LE ({option.TravelCost} LE travel + {option.ClinicCost} LE clinic).");
            return new TravelAndClinicVisitResult(false, option.TravelCost, option.ClinicCost, option.TotalCost, 0);
        }

        var travelEnergyCost = GetTravelEnergyCost(
            WorldState.AllLocations.First(l => l.Id == clinicLocationId));

        Player.Stats.ModifyMoney(-option.TravelCost);
        Player.Stats.ModifyEnergy(-travelEnergyCost);
        AdvanceTime(option.TravelTimeMinutes);
        World.TravelTo(clinicLocationId);

        if (Player.BackgroundType == BackgroundType.SudaneseRefugee)
        {
            var location = WorldState.AllLocations.First(l => l.Id == clinicLocationId);
            if (location.District == DistrictId.Dokki)
            {
                Player.Stats.ModifyStress(2);
                RaiseEvent("Dokki's questions land harder when your accent gets there before your name does.");
            }
        }

        RaiseEvent($"Traveled to {option.LocationName} with your mother.");

        var clinicResult = TakeMotherToClinic();

        return new TravelAndClinicVisitResult(
            clinicResult.Success,
            option.TravelCost,
            clinicResult.TotalCost,
            option.TravelCost + clinicResult.TotalCost,
            clinicResult.HealthChange);
    }

    public int GetMedicineCost()
    {
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        var modifiedCost = _locationPricingService.GetMedicineCost(World.CurrentDistrict, World.CurrentLocationId, Relationships, Player.Skills)
            + (districtCondition?.Effect.MedicineCostModifier ?? 0);
        return Math.Max(1, modifiedCost);
    }

    public JobPreview PreviewJob(JobType jobType)
    {
        return ApplyDistrictConditionToJobPreview(Jobs.PreviewJob(jobType, Player, Relationships, JobProgress));
    }

    public IReadOnlyList<DistrictConditionDefinition> GetDailyDistrictConditions()
    {
        return World.ActiveDistrictConditions
            .Select(static activeCondition => (activeCondition, definition: DistrictConditionRegistry.GetById(activeCondition.ConditionId)))
            .Where(static item => item.definition is not null)
            .OrderBy(static item => item.activeCondition.District)
            .Select(static item => item.definition!)
            .ToArray();
    }

    public DistrictConditionDefinition? GetActiveDistrictConditionDefinition(DistrictId districtId)
    {
        return DistrictConditionRegistry.GetById(World.GetActiveDistrictCondition(districtId)?.ConditionId);
    }

    public int GetTravelCost(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is null ? 0 : GetTravelCost(location);
    }

    public int GetTravelTimeMinutes(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is null ? 0 : GetTravelTimeMinutes(location);
    }

    public int GetWalkTimeMinutes(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        return location is null ? 0 : GetWalkTimeMinutes(location);
    }

    public string? GetTravelConditionSummary(LocationId locationId)
    {
        var location = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == locationId);
        if (location is null)
        {
            return null;
        }

        var districtCondition = GetActiveDistrictConditionDefinition(location.District);
        if (districtCondition is null)
        {
            return null;
        }

        var effect = districtCondition.Effect;
        if (effect.TravelCostModifier == 0 && effect.TravelTimeMinutesModifier == 0 && effect.TravelEnergyModifier == 0)
        {
            return null;
        }

        return $"{districtCondition.Title}: {districtCondition.GameplaySummary}";
    }

    private int GetTravelCost(Location destination)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(destination.District);
        var modifiedCost = _locationPricingService.GetTravelCost(destination, Relationships) + (districtCondition?.Effect.TravelCostModifier ?? 0);
        return Math.Max(1, modifiedCost);
    }

    private int GetClinicVisitCost(Location location)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(location.District);
        var modifiedCost = _locationPricingService.GetClinicVisitCost(location, Relationships, Player.Skills) + (districtCondition?.Effect.ClinicVisitCostModifier ?? 0);
        return Math.Max(1, modifiedCost);
    }

    private int GetTravelEnergyCost(Location destination)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(destination.District);
        var modifiedCost = _locationPricingService.GetTravelEnergyCost(destination, Relationships) + (districtCondition?.Effect.TravelEnergyModifier ?? 0);
        return Math.Max(1, modifiedCost);
    }

    private int GetTravelTimeMinutes(Location destination)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(destination.District);
        var modifiedMinutes = destination.TravelTimeMinutes + (districtCondition?.Effect.TravelTimeMinutesModifier ?? 0);
        return Math.Max(1, modifiedMinutes);
    }

    public int CurrentDay => Clock.Day;

    public IReadOnlyList<NpcId> GetReachableNpcs()
    {
        return NpcRegistry.GetReachableNpcs(World.CurrentLocationId, PolicePressure);
    }

    public void AdjustMoney(int delta)
    {
        Player.Stats.ModifyMoney(delta);
    }

    public void AdjustHealth(int delta)
    {
        Player.Stats.ModifyHealth(delta);
    }

    public void AdjustEnergy(int delta)
    {
        Player.Stats.ModifyEnergy(delta);
    }

    public void AdjustHunger(int delta)
    {
        Player.Nutrition.ModifySatiety(delta);
        SyncLegacyHunger();
    }

    public void AdjustStress(int delta)
    {
        Player.Stats.ModifyStress(delta);
    }

    public void AdjustMotherHealth(int delta)
    {
        Player.Household.UpdateMotherHealth(delta);
    }

    public void AdjustFoodStockpile(int delta)
    {
        if (delta > 0)
        {
            Player.Household.AddFood(delta);
            return;
        }

        for (var i = 0; i < -delta; i++)
        {
            Player.Household.ConsumeFood();
        }
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

    public void RecordFavor(NpcId npcId, bool hasUnpaidDebt)
    {
        Relationships.RecordFavor(npcId, Clock.Day, hasUnpaidDebt);
    }

    public void RecordRefusal(NpcId npcId)
    {
        Relationships.RecordRefusal(npcId, Clock.Day);
    }

    public void SetDebtState(NpcId npcId, bool hasUnpaidDebt)
    {
        Relationships.SetDebtState(npcId, hasUnpaidDebt);
    }

    public void SetEmbarrassedState(NpcId npcId, bool value)
    {
        Relationships.SetEmbarrassedState(npcId, value);
    }

    public void SetHelpedState(NpcId npcId, bool value)
    {
        Relationships.SetHelpedState(npcId, value);
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

    private bool TryQueueNarrativeTrigger(NarrativeSceneTrigger? trigger)
    {
        if (trigger is null || HasStoryFlag(trigger.FlagName))
        {
            return false;
        }

        SetStoryFlag(trigger.FlagName);
        QueueNarrativeScene(trigger.KnotName);
        return true;
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

    public IReadOnlyList<string> PendingNarrativeScenes => [.. _pendingNarrativeScenes];

    public void SetPolicePressure(int value)
    {
        PolicePressure = Math.Clamp(value, 0, 100);
    }

    public CrimeRoutePreview PreviewCrime(CrimeAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var modifierEvaluation = EvaluateCrimeModifiers(attempt);
        var resolution = _crimeService.PreviewCrime(modifierEvaluation.Attempt, Player, PolicePressure);
        return new CrimeRoutePreview(modifierEvaluation.Attempt, resolution, modifierEvaluation.ActiveModifiers);
    }

    public int GetEffectiveRandomEventWeight(RandomEvent randomEvent)
    {
        ArgumentNullException.ThrowIfNull(randomEvent);

        var weight = randomEvent.Weight;
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        if (districtCondition is null)
        {
            return weight;
        }

        if (districtCondition.Effect.BoostedRandomEventIds.Contains(randomEvent.Id, StringComparer.Ordinal))
        {
            weight += 4;
        }

        if (districtCondition.Effect.SuppressedRandomEventIds.Contains(randomEvent.Id, StringComparer.Ordinal))
        {
            weight = Math.Max(1, weight - 3);
        }

        return weight;
    }

    private void ApplyCrimeContactAftermath(CrimeResult result)
    {
        var aftermath = CrimeNarrativePlanner.GetDetectedContactAftermath(World.CurrentLocationId, Relationships, result);
        if (aftermath is null)
        {
            return;
        }

        ReduceCrimeHeat(aftermath.PolicePressureReduction, aftermath.HeatMessage, aftermath.HeatTrigger);

        if (!result.Success && !string.IsNullOrWhiteSpace(aftermath.FailureMessage))
        {
            ApplyCrimeFailureMitigation(
                aftermath.FailureMoneyGain,
                aftermath.FailureStressRelief,
                aftermath.FailureMessage,
                aftermath.FailureTrigger);
        }
    }

    private void ReduceCrimeHeat(int amount, string message, NarrativeSceneTrigger trigger)
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
        TryQueueNarrativeTrigger(trigger);
    }

    private void ApplyCrimeFailureMitigation(int moneyGain, int stressRelief, string message, NarrativeSceneTrigger? trigger)
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
        TryQueueNarrativeTrigger(trigger);
    }

    public void SetRunId(Guid runId)
    {
        RunId = runId;
    }

    public void RestoreRunState(
        Guid runId,
        int daysSurvived,
        bool isGameOver,
        string? gameOverReason,
        EndingId? endingId,
        string? pendingEndingKnot)
    {
        SetRunId(runId);
        SetDaysSurvived(daysSurvived);
        IsGameOver = isGameOver;
        GameOverReason = string.IsNullOrWhiteSpace(gameOverReason) ? null : gameOverReason;
        EndingId = endingId;
        PendingEndingKnot = string.IsNullOrWhiteSpace(pendingEndingKnot) ? null : pendingEndingKnot;
    }

    public void SetDaysSurvived(int daysSurvived)
    {
        DaysSurvived = Math.Max(0, daysSurvived);
    }

    public void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted)
    {
        SetCrimeCounters(totalCrimeEarnings, crimesCommitted, LastCrimeDay);
    }

    public void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay)
    {
        TotalCrimeEarnings = Math.Max(0, totalCrimeEarnings);
        CrimesCommitted = Math.Max(0, crimesCommitted);
        LastCrimeDay = Math.Max(0, lastCrimeDay);
    }

    public void RestoreCrimeState(int policePressure, int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay, bool hasCrimeCommittedToday)
    {
        SetPolicePressure(policePressure);
        SetCrimeCounters(totalCrimeEarnings, crimesCommitted, lastCrimeDay);
        CrimeCommittedToday = hasCrimeCommittedToday;
    }

    public void SetWorkCounters(int totalHonestWorkEarnings, int honestShiftsCompleted, int lastHonestWorkDay, int lastPublicFacingWorkDay)
    {
        TotalHonestWorkEarnings = Math.Max(0, totalHonestWorkEarnings);
        HonestShiftsCompleted = Math.Max(0, honestShiftsCompleted);
        LastHonestWorkDay = Math.Max(0, lastHonestWorkDay);
        LastPublicFacingWorkDay = Math.Max(0, lastPublicFacingWorkDay);
    }

    public void RestoreWorkState(int totalHonestWorkEarnings, int honestShiftsCompleted, int lastHonestWorkDay, int lastPublicFacingWorkDay)
    {
        TotalHonestWorkEarnings = Math.Max(0, totalHonestWorkEarnings);
        HonestShiftsCompleted = Math.Max(0, honestShiftsCompleted);
        LastHonestWorkDay = Math.Max(0, lastHonestWorkDay);
        LastPublicFacingWorkDay = Math.Max(0, lastPublicFacingWorkDay);
    }

    public void RestoreRentState(int unpaidRentDays, int accumulatedRentDebt, bool firstWarningGiven, bool finalWarningGiven)
    {
        _rentState.Restore(unpaidRentDays, accumulatedRentDebt, firstWarningGiven, finalWarningGiven);
    }

    public void RecordEventHistory(string eventId, int count)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        _randomEventHistory[eventId] = Math.Max(0, count);
    }

    public void RestoreNarrativeState(
        IEnumerable<string> storyFlags,
        IEnumerable<KeyValuePair<string, int>> randomEventHistory,
        IEnumerable<string> pendingNarrativeScenes)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        ArgumentNullException.ThrowIfNull(randomEventHistory);
        ArgumentNullException.ThrowIfNull(pendingNarrativeScenes);

        RestoreStoryFlags(storyFlags);
        _randomEventHistory.Clear();
        foreach (var pair in randomEventHistory)
        {
            RecordEventHistory(pair.Key, pair.Value);
        }

        _pendingNarrativeScenes.Clear();
        foreach (var scene in pendingNarrativeScenes.Where(static scene => !string.IsNullOrWhiteSpace(scene)))
        {
            _pendingNarrativeScenes.Enqueue(scene);
        }
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

    private void RollDistrictConditionsForCurrentDay(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var activeConditions = new List<ActiveDistrictCondition>();
        foreach (var districtId in Enum.GetValues<DistrictId>())
        {
            var candidates = DistrictConditionRegistry.GetDefinitionsForDistrict(districtId)
                .Where(definition => definition.IsEligible(Clock.Day, PolicePressure))
                .ToArray();
            if (candidates.Length == 0)
            {
                continue;
            }

            var selected = SelectWeightedDistrictCondition(candidates, random);
            activeConditions.Add(new ActiveDistrictCondition
            {
                District = districtId,
                ConditionId = selected.Id
            });
        }

        World.SetActiveDistrictConditions(activeConditions);
    }

    private void SetBaselineDistrictConditions()
    {
        World.SetActiveDistrictConditions(
        [
            new ActiveDistrictCondition { District = DistrictId.Imbaba, ConditionId = "imbaba_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.Dokki, ConditionId = "dokki_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.ArdAlLiwa, ConditionId = "ardalliwa_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.BulaqAlDakrour, ConditionId = "bulaq_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.Shubra, ConditionId = "shubra_steady_day" }
        ]);
    }

    private static DistrictConditionDefinition SelectWeightedDistrictCondition(
        IReadOnlyList<DistrictConditionDefinition> candidates,
        Random random)
    {
        var totalWeight = candidates.Sum(static definition => definition.Weight);
#pragma warning disable CA5394
        var roll = random.Next(1, totalWeight + 1);
#pragma warning restore CA5394
        var cumulativeWeight = 0;
        foreach (var candidate in candidates)
        {
            cumulativeWeight += candidate.Weight;
            if (roll <= cumulativeWeight)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }

    private JobPreview ApplyDistrictConditionToJobPreview(JobPreview preview)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        if (districtCondition is null)
        {
            return preview;
        }

        var effect = districtCondition.Effect;
        if (effect.WorkPayModifier == 0 && effect.WorkStressModifier == 0)
        {
            return preview;
        }

        var activeModifiers = preview.ActiveModifiers.ToList();
        activeModifiers.Add(BuildWorkDistrictModifierText(districtCondition));

        return new JobPreview(
            ApplyDistrictConditionToJob(preview.Job),
            preview.VariantReason,
            preview.NextUnlockHint,
            activeModifiers,
            preview.RiskWarning);
    }

    private JobShift ApplyDistrictConditionToJob(JobShift job)
    {
        ArgumentNullException.ThrowIfNull(job);

        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        if (districtCondition is null)
        {
            return job;
        }

        var effect = districtCondition.Effect;
        if (effect.WorkPayModifier == 0 && effect.WorkStressModifier == 0)
        {
            return job;
        }

        return CloneJobShift(
            job,
            Math.Max(0, job.BasePay + effect.WorkPayModifier),
            Math.Max(0, job.StressCost + effect.WorkStressModifier));
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
        PendingEndingKnot = EndingService.GetInkKnot(this, ending.Value);
    }

    private void ModifyEmployerTrust(JobType jobType, int delta)
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
            ModifyNpcTrust(npcId.Value, delta);
        }
    }

    private void ApplyWorkCrimeSpillover(JobShift job, JobResult result)
    {
        var publicWorkHeat = WorkNarrativePlanner.GetPublicWorkHeatPlan(Clock.Day, LastCrimeDay, PolicePressure, _storyFlags, job);
        if (publicWorkHeat is not null)
        {
            Player.Stats.ModifyStress(publicWorkHeat.StressDelta);
            ModifyEmployerTrust(job.Type, publicWorkHeat.EmployerTrustDelta);
            RaiseEvent(publicWorkHeat.Message);
            TryQueueNarrativeTrigger(publicWorkHeat.NarrativeTrigger);
        }

        if (WorkNarrativePlanner.ShouldEmbarrassWorkshopBoss(job, result))
        {
            Relationships.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
            Relationships.RecordRefusal(NpcId.WorkshopBossAbuSamir, Clock.Day);
        }
    }

    private void ApplyBackgroundWorkFlavor(JobShift job, JobResult result)
    {
        TryQueueNarrativeTrigger(WorkNarrativePlanner.GetMedicalClinicTrigger(Player, job, result, _storyFlags));

        if (WorkNarrativePlanner.ShouldGrantSalmaMedicineHelp(Player, job, result, Relationships))
        {
            Relationships.RecordFavor(NpcId.NurseSalma, Clock.Day, hasUnpaidDebt: true);
            RaiseEvent("Nurse Salma quietly covers a little medicine for your mother. You owe her now.");
        }
    }

    private void QueueNarrativeFollowUpScenes()
    {
        foreach (var trigger in NarrativeFollowUpPlanner.GetEndOfDayTriggers(
                     CrimeCommittedToday,
                     Player,
                     TotalCrimeEarnings,
                     CrimesCommitted,
                     PolicePressure,
                     Relationships,
                     _storyFlags))
        {
            TryQueueNarrativeTrigger(trigger);
        }
    }

    private CrimeModifierEvaluation EvaluateCrimeModifiers(CrimeAttempt attempt)
    {
        var modifiedAttempt = attempt;
        var activeModifiers = new List<string>();

        if (LastPublicFacingWorkDay == Clock.Day)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Max(5, modifiedAttempt.DetectionRisk - 8),
                PolicePressureIncrease = Math.Max(1, modifiedAttempt.PolicePressureIncrease - 4)
            };
            activeModifiers.Add("Same-day public-facing work gives you a thin alibi: lower risk and lower pressure.");
        }

        if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Min(95, modifiedAttempt.DetectionRisk + 5),
                PolicePressureIncrease = modifiedAttempt.PolicePressureIncrease + 5
            };
            activeModifiers.Add("Released political prisoner background increases scrutiny and pressure.");
        }

        if (Player.Skills.GetLevel(SkillId.StreetSmarts) >= 3)
        {
            activeModifiers.Add("Street Smarts 3 lowers detection chance by 10.");
        }

        if (PolicePressure >= 60)
        {
            activeModifiers.Add("Current police pressure is materially increasing detection risk.");
        }

        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
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

        return new CrimeModifierEvaluation(modifiedAttempt, activeModifiers);
    }

    private void ApplyCrimeModifierSideEffects(IReadOnlyList<string> activeModifiers)
    {
        if (activeModifiers.Contains("Same-day public-facing work gives you a thin alibi: lower risk and lower pressure."))
        {
            RaiseEvent("The shift you worked today gives you a thin alibi and a cleaner reason to be seen moving.");
        }

        if (activeModifiers.Contains("Released political prisoner background increases scrutiny and pressure."))
        {
            TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetPrisonerHeatTrigger(Player.BackgroundType, _storyFlags));
        }
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

        if (NarrativeSignalRules.HasPendingSudaneseSolidarity(Player.BackgroundType, randomEvent.Id, _storyFlags))
        {
            TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.BackgroundSudaneseSolidaritySeen, NarrativeKnots.BackgroundSudaneseSolidarity));
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

    private DayOfWeek GetCurrentDayOfWeek()
    {
        var offset = (Clock.Day - 1) % 7;
        return (DayOfWeek)(((int)DayOfWeek.Saturday + offset) % 7);
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

    private static string FormatOpenDays(IEnumerable<DayOfWeek> openDays)
    {
        return string.Join(", ", openDays.Select(static day => day.ToString()[..3]));
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
            _ => SkillId.StreetSmarts
        };
    }

    private void RaiseEvent(string message)
    {
        GameEvent?.Invoke(this, new GameEventArgs(message));
    }

    public IReadOnlyList<InvestmentDefinition> GetAvailableInvestments()
    {
        var results = new List<InvestmentDefinition>();

        foreach (var definition in GetCurrentInvestmentOpportunities())
        {
            if (!CheckInvestmentEligibility(definition).IsEligible)
            {
                continue;
            }

            results.Add(definition);
        }

        return results;
    }

    public InvestmentEligibility CheckInvestmentEligibility(InvestmentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return InvestmentEligibilityEvaluator.Evaluate(definition, CreateInvestmentEligibilityContext());
    }

    public MakeInvestmentResult MakeInvestment(InvestmentType type)
    {
        var definition = InvestmentRegistry.GetByType(type);
        if (definition is null)
        {
            return new MakeInvestmentResult(false, 0, "Unknown investment type.");
        }

        var eligibility = CheckInvestmentEligibility(definition);
        if (!eligibility.IsEligible)
        {
            return new MakeInvestmentResult(false, 0, string.Join(" ", eligibility.FailureReasons));
        }

        Player.Stats.ModifyMoney(-definition.Cost);

        var investment = new Investment(
            type,
            definition.Cost,
            definition.WeeklyIncomeMin,
            definition.WeeklyIncomeMax,
            definition.RiskProfile);

        _investmentState.ActiveInvestments.Add(investment);

        RaiseEvent($"Invested {definition.Cost} LE in {definition.Name}.");

        return new MakeInvestmentResult(true, definition.Cost, $"Successfully invested in {definition.Name}.");
    }

    public InvestmentResolutionSummary ResolveWeeklyInvestments(Random? random = null)
    {
        var rng = random ?? _sharedRandom;
        var summary = new InvestmentResolutionSummary();

        var toRemove = new List<Investment>();

        foreach (var investment in _investmentState.ActiveInvestments)
        {
            investment.IncrementWeek();

            if (investment.IsSuspended)
            {
                var definition = InvestmentRegistry.GetByType(investment.Type);
                summary.AddResult(new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: false,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 0,
                    InvestedAmountLost: 0,
                    $"{definition?.Name ?? investment.Type.ToString()} is recovering after last week's disruption and pays nothing this week."));
                investment.Unsuspend();
                continue;
            }

            var calculation = InvestmentResolutionCalculator.Resolve(
                investment,
                InvestmentRegistry.GetByType(investment.Type),
                Player.Stats.Money,
                rng);

            if (calculation.ShouldSuspend)
            {
                investment.Suspend();
                TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.EventInvestmentSuspensionSeen, NarrativeKnots.EventInvestmentSuspension));
            }

            var result = calculation.Resolution;
            summary.AddResult(result);

            if (result.WasLost)
            {
                toRemove.Add(investment);
            }

            if (result.Income > 0)
            {
                Player.Stats.ModifyMoney(result.Income);
                TotalInvestmentEarnings += result.Income;
            }

            if (result.ExtortionPaid > 0)
            {
                Player.Stats.ModifyMoney(-result.ExtortionPaid);
            }

            if (result.PolicePressureIncrease > 0)
            {
                SetPolicePressure(PolicePressure + result.PolicePressureIncrease);
            }

            if (!string.IsNullOrWhiteSpace(result.Message) &&
                (result.WasLost || result.ExtortionPaid > 0 || result.PolicePressureIncrease > 0))
            {
                RaiseEvent(result.Message);
            }
        }

        foreach (var investment in toRemove)
        {
            _investmentState.ActiveInvestments.Remove(investment);
        }

        if (summary.TotalIncome > 0 || summary.TotalLosses > 0 || summary.TotalExtortion > 0)
        {
            RaiseEvent($"Weekly investments: +{summary.TotalIncome} LE income, -{summary.TotalExtortion} LE extortion, {summary.LostCount} lost.");
        }

        return summary;
    }

    public void RestoreInvestmentState(
        IEnumerable<InvestmentSnapshot> investments,
        int totalInvestmentEarnings)
    {
        ArgumentNullException.ThrowIfNull(investments);

        _investmentState.ActiveInvestments.Clear();
        foreach (var snapshot in investments)
        {
            var definition = InvestmentRegistry.GetByType(snapshot.Type);
            if (definition is null)
            {
                continue;
            }

            _investmentState.ActiveInvestments.Add(Investment.Restore(snapshot, definition.RiskProfile));
        }

        TotalInvestmentEarnings = totalInvestmentEarnings;
    }

    private InvestmentEligibilityContext CreateInvestmentEligibilityContext()
    {
        return new InvestmentEligibilityContext(
            Player.Stats.Money,
            World.CurrentLocationId,
            GetReachableNpcs().ToHashSet(),
            _investmentState.ActiveInvestments.Select(static investment => investment.Type).ToHashSet(),
            Relationships,
            TotalCrimeEarnings,
            Player.Skills.GetLevel(SkillId.StreetSmarts),
            Player.BackgroundType);
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

    public void Dispose()
    {
        _database.Dispose();
    }
}
