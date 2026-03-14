using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Crimes;
using Slums.Core.Endings;
using Slums.Core.Entertainment;
using Slums.Core.Expenses;
using Slums.Core.Events;
using Slums.Core.Investments;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.World;
using EntitiesDb;

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
    private readonly Queue<string> _pendingNarrativeScenes;
    private readonly HashSet<string> _storyFlags;
    private readonly Dictionary<string, int> _randomEventHistory;

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
        _sharedRandom = sharedRandom ?? new Random();
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

        AdvanceTime(location.TravelTimeMinutes);
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

    private static int GetWalkTimeMinutes(Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        return destination.TravelTimeMinutes * 3;
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
            if (!HasStoryFlag("crime_first_success"))
            {
                SetStoryFlag("crime_first_success");
                QueueNarrativeScene("crime_first_success");
            }
        }

        QueueContactCrimeScene(attempt, result);

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

        if (!HasStoryFlag("mother_clinic_first_visit"))
        {
            SetStoryFlag("mother_clinic_first_visit");
            QueueNarrativeScene("mother_clinic_first_visit");
        }

        return new MotherClinicVisitResult(true, clinicStatus.VisitCost, healthChange);
    }

#pragma warning disable CA1024
    public int GetFoodCost()
    {
        return World.CurrentDistrict switch
        {
            DistrictId.Dokki => 20,
            DistrictId.Imbaba => 15,
            DistrictId.ArdAlLiwa => 13,
            DistrictId.BulaqAlDakrour => 14,
            DistrictId.Shubra => 17,
            _ => RecurringExpenses.CheapFoodStockpile
        };
    }

    public int GetStreetFoodCost()
    {
        return World.CurrentDistrict switch
        {
            DistrictId.Dokki => 10,
            DistrictId.Imbaba => 8,
            DistrictId.ArdAlLiwa => 7,
            DistrictId.BulaqAlDakrour => 7,
            DistrictId.Shubra => 9,
            _ => 8
        };
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
            TravelTimeMinutes: location.TravelTimeMinutes,
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
        var districtCost = World.CurrentDistrict switch
        {
            DistrictId.Dokki => 58,
            DistrictId.Imbaba => 50,
            DistrictId.ArdAlLiwa => 42,
            DistrictId.BulaqAlDakrour => 46,
            DistrictId.Shubra => 52,
            _ => RecurringExpenses.MedicineCost
        };

        if (World.CurrentLocationId == LocationId.Pharmacy && Relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 12)
        {
            districtCost = Math.Max(30, districtCost - 6);
        }

        return Player.Skills.GetLevel(SkillId.Medical) >= 3
            ? Math.Max(32, districtCost - 8)
            : districtCost;
    }

    private int GetTravelCost(Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var travelCost = RecurringExpenses.TravelCost;
        if (destination.District == DistrictId.BulaqAlDakrour && Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 12)
        {
            travelCost = Math.Max(1, travelCost - 1);
        }

        return travelCost;
    }

    private int GetClinicVisitCost(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

        var visitCost = location.ClinicVisitBaseCost;
        if (visitCost <= 0)
        {
            return 0;
        }

        if (Player.Skills.GetLevel(SkillId.Medical) >= 2)
        {
            visitCost = Math.Max(20, visitCost - 5);
        }

        if (location.Id == LocationId.Clinic && Relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 20)
        {
            visitCost = Math.Max(18, visitCost - 6);
        }

        if (location.Id == LocationId.Pharmacy && Relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 12)
        {
            visitCost = Math.Max(20, visitCost - 4);
        }

        return visitCost;
    }

    private int GetTravelEnergyCost(Location destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var energyCost = 5;
        if (destination.District == DistrictId.BulaqAlDakrour && Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 12)
        {
            energyCost = 3;
        }

        if (destination.District == DistrictId.Shubra && Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 12)
        {
            energyCost = Math.Max(2, energyCost - 1);
        }

        return energyCost;
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

        if (World.CurrentLocationId == LocationId.Depot && Relationships.GetNpcRelationship(NpcId.DispatcherSafaa).Trust >= 15)
        {
            ReduceCrimeHeat(5, "Safaa reroutes the gossip faster than the depot can pin it to you. The heat slips sideways.", "crime_safaa_reroute", "crime_safaa_reroute_seen");

            if (!result.Success)
            {
                ApplyCrimeFailureMitigation(
                    moneyGain: 8,
                    stressRelief: 4,
                    message: "Safaa turns a blown move into something survivable and keeps one driver's mouth shut.",
                    knotName: "crime_safaa_salvage",
                    flagName: "crime_safaa_salvage_seen");
            }
        }

        if (World.CurrentLocationId == LocationId.Laundry && Relationships.GetNpcRelationship(NpcId.LaundryOwnerIman).Trust >= 15)
        {
            ReduceCrimeHeat(4, "Iman clocks the wrong kind of attention in the lane and sends you out the back before it settles on your face.", "crime_iman_cover", "crime_iman_cover_seen");

            if (!result.Success)
            {
                ApplyCrimeFailureMitigation(
                    moneyGain: 0,
                    stressRelief: 5,
                    message: "Iman does not ask questions. She only gets you clear before panic starts showing.",
                    knotName: "crime_iman_exit",
                    flagName: "crime_iman_exit_seen");
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
            CrimeType.DepotFareSkim => GetCrimeScene(result, "crime_safaa_skim_success", "crime_safaa_skim_detected", "crime_safaa_skim_failure"),
            CrimeType.ShubraBundleLift => GetCrimeScene(result, "crime_iman_bundle_success", "crime_iman_bundle_detected", "crime_iman_bundle_failure"),
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
        if (Clock.Day - LastCrimeDay > 1 || LastCrimeDay == 0)
        {
            return;
        }

        if (PolicePressure >= 60 && ActivityLedgerSystem.IsPublicFacingJob(job.Type))
        {
            Player.Stats.ModifyStress(4);
            ModifyEmployerTrust(job.Type, -2);
            RaiseEvent("The street heat follows you into work. People notice how tense you look.");

            if (!HasStoryFlag("event_public_work_heat_seen"))
            {
                SetStoryFlag("event_public_work_heat_seen");
                QueueNarrativeScene("event_public_work_heat");
            }
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

    private void QueueNarrativeFollowUpScenes()
    {
        if (CrimeCommittedToday &&
            TotalCrimeEarnings >= 150 &&
            CrimesCommitted >= 2 &&
            Player.Household.MotherHealth < 65 &&
            !HasStoryFlag("event_mother_wrong_money_seen"))
        {
            SetStoryFlag("event_mother_wrong_money_seen");
            QueueNarrativeScene("event_mother_wrong_money");
        }

        if (CrimeCommittedToday &&
            PolicePressure >= 60 &&
            Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust >= 15 &&
            !HasStoryFlag("event_neighbor_watch_seen"))
        {
            SetStoryFlag("event_neighbor_watch_seen");
            QueueNarrativeScene("event_neighbor_watch");
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
            if (!HasStoryFlag("background_prisoner_heat_seen"))
            {
                SetStoryFlag("background_prisoner_heat_seen");
                QueueNarrativeScene("background_prisoner_heat");
            }
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

        var reasons = new List<string>();

        if (Player.Stats.Money < definition.Cost)
        {
            reasons.Add($"Not enough money. Cost: {definition.Cost} LE.");
        }

        if (definition.OpportunityLocationId != World.CurrentLocationId)
        {
            reasons.Add($"This opportunity is only discussed at {GetLocationName(definition.OpportunityLocationId)}.");
        }

        if (definition.OpportunityNpc is NpcId sponsorNpc && !GetReachableNpcs().Contains(sponsorNpc))
        {
            reasons.Add($"{NpcRegistry.GetName(sponsorNpc)} is not available to discuss this here right now.");
        }

        if (_investmentState.ActiveInvestments.Any(i => i.Type == definition.Type))
        {
            reasons.Add("You already have this investment.");
        }

        if (definition.RequiredRelationshipNpc is NpcId npcId &&
            definition.RequiredRelationshipTrust > 0)
        {
            var trust = Relationships.GetNpcRelationship(npcId).Trust;
            if (trust < definition.RequiredRelationshipTrust)
            {
                reasons.Add($"Need {definition.RequiredRelationshipTrust} trust with {NpcRegistry.GetName(npcId)}. Current: {trust}.");
            }
        }

        if (definition.RequiresCrimePath && TotalCrimeEarnings < 50)
        {
            reasons.Add("Requires active involvement in crime operations.");
        }

        if (definition.RequiresStreetSmartsOrExPrisoner)
        {
            var hasStreetSmarts = Player.Skills.GetLevel(SkillId.StreetSmarts) >= 2;
            var isExPrisoner = Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner;

            if (!hasStreetSmarts && !isExPrisoner)
            {
                reasons.Add("Requires street smarts (level 2+) or ex-prisoner background.");
            }
        }

        return new InvestmentEligibility(reasons.Count == 0, reasons);
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
                summary.AddResult(new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: false,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 0,
                    InvestedAmountLost: 0,
                    $"{GetInvestmentName(investment.Type)} is recovering after last week's disruption and pays nothing this week."));
                investment.Unsuspend();
                continue;
            }

            var result = ResolveSingleInvestment(investment, rng);
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

#pragma warning disable CA5394 // Random is sufficient for gameplay mechanics
    private InvestmentResolution ResolveSingleInvestment(Investment investment, Random rng)
    {
        var definition = InvestmentRegistry.GetByType(investment.Type);
        if (definition is null)
        {
            return new InvestmentResolution(investment.Type, 0, false, 0, 0, 0, "Investment definition not found.");
        }

        var profile = investment.RiskProfile;

        if (rng.NextDouble() < profile.WeeklyFailureChance)
        {
            return new InvestmentResolution(
                investment.Type,
                0,
                WasLost: true,
                ExtortionPaid: 0,
                PolicePressureIncrease: 0,
                InvestedAmountLost: investment.InvestedAmount,
                $"Your {definition.Name} venture failed. Investment lost.");
        }

        if (rng.NextDouble() < profile.ExtortionChance)
        {
            var extortionAmount = rng.Next(profile.ExtortionAmountMin, profile.ExtortionAmountMax + 1);

            if (Player.Stats.Money >= extortionAmount)
            {
                return new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: false,
                    ExtortionPaid: extortionAmount,
                    PolicePressureIncrease: 0,
                    InvestedAmountLost: 0,
                    $"Gangs demanded {extortionAmount} LE from your {definition.Name} operation.");
            }
            else
            {
                investment.Suspend();
                return new InvestmentResolution(
                    investment.Type,
                    0,
                    WasLost: false,
                    ExtortionPaid: 0,
                    PolicePressureIncrease: 2,
                    InvestedAmountLost: 0,
                    $"Could not pay extortion for {definition.Name}. Operation suspended.");
            }
        }

        if (rng.NextDouble() < profile.PoliceHeatChance)
        {
            return new InvestmentResolution(
                investment.Type,
                0,
                WasLost: false,
                ExtortionPaid: 0,
                PolicePressureIncrease: 5,
                InvestedAmountLost: 0,
                $"Police interest in your {definition.Name} increases pressure.");
        }

        if (rng.NextDouble() < profile.BetrayalChance)
        {
            return new InvestmentResolution(
                investment.Type,
                0,
                WasLost: true,
                ExtortionPaid: 0,
                PolicePressureIncrease: 0,
                InvestedAmountLost: investment.InvestedAmount,
                $"Your partner in {definition.Name} disappeared with the funds.");
        }

        var income = rng.Next(investment.WeeklyIncomeMin, investment.WeeklyIncomeMax + 1);
        return new InvestmentResolution(
            investment.Type,
            income,
            WasLost: false,
            ExtortionPaid: 0,
            PolicePressureIncrease: 0,
            InvestedAmountLost: 0,
            $"{definition.Name} earned {income} LE this week.");
    }
#pragma warning restore CA5394

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

    private static string GetLocationName(LocationId locationId)
    {
        return WorldState.AllLocations.FirstOrDefault(location => location.Id == locationId)?.Name ?? locationId.Value;
    }

    private static string GetInvestmentName(InvestmentType type)
    {
        return InvestmentRegistry.GetByType(type)?.Name ?? type.ToString();
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
