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
using Slums.Core.Training;
using Slums.Core.Home;
using Slums.Core.World;
using EntitiesDb;
using Slums.Core.Diagnostics;
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
    private readonly List<GameMutationRecord> _mutations = [];
    private readonly Dictionary<SkillId, bool> _trainedSkillsToday = [];

    public GameSession(Random? sharedRandom = null)
    {
        Clock = new GameClock();
        _playerIdentity = new PlayerIdentityState();
        Player = new PlayerCharacter(_playerIdentity, new SurvivalStats(), new NutritionState(), new HouseholdCareState(), new HouseholdAssetsState(), new SkillState());
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
        _database.Create(_playerIdentity, Player.Stats, Player.Nutrition, Player.Household, Player.HouseholdAssets, Player.Skills);
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
    public int TotalHerbEarnings => Player.HouseholdAssets.TotalHerbEarnings;
    public int UnpaidRentDays => _rentState.UnpaidRentDays;
    public int AccumulatedRentDebt => _rentState.AccumulatedRentDebt;
    public bool FirstWarningGiven => _rentState.FirstWarningGiven;
    public bool FinalWarningGiven => _rentState.FinalWarningGiven;
    private bool CrimeCommittedToday { get => _crimeState.CrimeCommittedToday; set => _crimeState.CrimeCommittedToday = value; }
    public HomeUpgradeState HomeUpgrades { get; } = new();

    public event EventHandler<GameEventArgs>? GameEvent;
    public IReadOnlyList<GameMutationRecord> Mutations => _mutations;
    public event EventHandler<GameMutationEventArgs>? MutationRecorded;

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
            const int endOfDayMinutes = EndOfDayHour * 60;

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
        var before = CaptureStats();
        var currentWeek = CurrentWeek;
        Player.Stats.ApplyDailyDecay();

        var nutritionResolution = Player.Nutrition.ResolveDay();
        Player.Stats.ModifyEnergy(nutritionResolution.EnergyDelta);
        Player.Stats.ModifyHealth(nutritionResolution.HealthDelta);
        Player.Stats.ModifyStress(nutritionResolution.StressDelta);
        SyncLegacyHunger();

        var overnightRecovery = SleepQualityCalculator.CalculateOvernightRecovery(
            Player.Stats, Player.Nutrition, Player.Household,
            UnpaidRentDays, HomeUpgrades);
        Player.Stats.ModifyEnergy(overnightRecovery);

        if (HomeUpgrades.GetStressBonus() > 0)
        {
            Player.Stats.ModifyStress(-HomeUpgrades.GetStressBonus());
        }

        var motherCareResolution = Player.Household.ResolveDay();
        Player.Stats.ModifyStress(motherCareResolution.StressDelta);
        var householdAssetsBonus = Player.HouseholdAssets.GetMotherDailyHealthBonus(currentWeek);
        if (householdAssetsBonus > 0)
        {
            Player.Household.UpdateMotherHealth(householdAssetsBonus);
        }

        var rentResult = _rentState.ProcessDay(RecurringExpenses.DailyRentCost, Player.Stats.Money);
        if (rentResult.Paid)
        {
            Player.Stats.ModifyMoney(-RecurringExpenses.DailyRentCost);
            RaiseAutoTransaction($"Paid rent: {RecurringExpenses.DailyRentCost} LE");
        }
        else
        {
            RaiseAutoTransaction($"Could not pay rent! Debt: {rentResult.AccumulatedDebt} LE. Unpaid days: {rentResult.CurrentUnpaidDays}.");

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

        var herbIncome = Player.HouseholdAssets.ResolveSellablePlantIncome(Clock.Day, currentWeek);
        if (herbIncome > 0)
        {
            Player.Stats.ModifyMoney(herbIncome);
            RaiseAutoTransaction($"The street vendor moves your herbs quietly. +{herbIncome} LE reaches home.");
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

        if (GetCurrentDayOfWeek() == GameDayOfWeek.Monday)
        {
            ResolveWeeklyHouseholdAssets();
            if (_investmentState.ActiveInvestments.Count > 0)
            {
                ResolveWeeklyInvestments(random ?? _sharedRandom);
            }
        }

        Player.Nutrition.BeginNewDay();
        Player.Household.BeginNewDay();
        _trainedSkillsToday.Clear();

        foreach (var randomEvent in _randomEventService.RollDailyEvents(this, random ?? _sharedRandom))
        {
            ApplyRandomEvent(randomEvent);
        }

        TryRollStreetCatEncounter(random ?? _sharedRandom);
        QueueNarrativeFollowUpScenes();

        ActivityLedgerSystem.BeginNewDay(_crimeState);
        CheckGameOverConditions();
        RecordMutation(MutationCategories.DayTransition, "EndDay", before, CaptureStats(), $"Day {CurrentDay} completed");
    }

    public bool RestAtHome()
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "RestAtHome", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to go home to rest.");
            return false;
        }

        var recovery = SleepQualityCalculator.CalculateRecovery(
            Player.Stats, Player.Nutrition, Player.Household,
            UnpaidRentDays, HomeUpgrades);

        Player.Stats.ModifyEnergy(recovery);
        Player.Stats.ModifyHunger(-10);
        Player.Stats.ModifyStress(-15);
        AdvanceTime(8 * 60);

        var breakdown = SleepQualityCalculator.BuildRecoveryBreakdown(
            recovery, Player.Stats, Player.Nutrition, Player.Household,
            UnpaidRentDays, HomeUpgrades);
        RaiseEvent($"You rest at home. Energy +{recovery}. ({breakdown})");
        RecordMutation(MutationCategories.Rest, "RestAtHome", before, CaptureStats(), "Rested at home");
        return true;
    }

    public bool TryPurchaseHomeUpgrade(HomeUpgrade upgrade)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPurchaseHomeUpgrade", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to be at home to improve it.");
            return false;
        }

        if (HomeUpgrades.HasUpgrade(upgrade))
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPurchaseHomeUpgrade", before, CaptureStats(), $"{upgrade} already purchased");
            RaiseEvent($"You already have {HomeUpgradeDefinitions.GetDescription(upgrade)}.");
            return false;
        }

        var cost = HomeUpgradeDefinitions.GetCost(upgrade);
        if (Player.Stats.Money < cost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPurchaseHomeUpgrade", before, CaptureStats(), $"Not enough money ({cost} LE)");
            RaiseEvent($"You can't afford that. You need {cost} LE but only have {Player.Stats.Money} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-cost);
        HomeUpgrades.Purchase(upgrade);
        RaiseEvent($"You bought {HomeUpgradeDefinitions.GetDescription(upgrade)} for {cost} LE.");
        RecordMutation(MutationCategories.Shop, "TryPurchaseHomeUpgrade", before, CaptureStats(), $"Purchased {upgrade} for {cost} LE");
        return true;
    }

    public bool TryTravelTo(LocationId locationId)
    {
        var before = CaptureStats();
        var location = WorldState.AllLocations.FirstOrDefault(l => l.Id == locationId);
        if (location is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, CaptureStats(), $"Location {locationId} not found");
            return false;
        }

        if (World.CurrentLocationId == locationId)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, CaptureStats(), $"Already at {location.Name}");
            RaiseEvent($"You are already at {location.Name}.");
            return false;
        }

        var travelCost = GetTravelCost(location);
        var travelEnergyCost = GetTravelEnergyCost(location);

        if (Player.Stats.Money < travelCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, CaptureStats(), $"Not enough money (need {travelCost} LE, have {Player.Stats.Money} LE)");
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
        RecordMutation(MutationCategories.Travel, "TryTravelTo", before, CaptureStats(), $"Traveled to {location.Name} (cost {travelCost} LE)");
        return true;
    }

    public bool TryWalkTo(LocationId locationId)
    {
        var before = CaptureStats();
        var location = WorldState.AllLocations.FirstOrDefault(l => l.Id == locationId);
        if (location is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, CaptureStats(), $"Location {locationId} not found");
            return false;
        }

        if (World.CurrentLocationId == locationId)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, CaptureStats(), $"Already at {location.Name}");
            RaiseEvent($"You are already at {location.Name}.");
            return false;
        }

        var walkEnergyCost = GetWalkEnergyCost(location);
        var walkTimeMinutes = GetWalkTimeMinutes(location);

        if (Player.Stats.Energy < walkEnergyCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, CaptureStats(), $"Too exhausted (need {walkEnergyCost} energy, have {Player.Stats.Energy})");
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
        RecordMutation(MutationCategories.Travel, "TryWalkTo", before, CaptureStats(), $"Walked to {location.Name}");
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
        var before = CaptureStats();

        if (Player.Stats.Money < activity.BaseCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, CaptureStats(), $"Cannot afford {activity.Name} (cost {activity.BaseCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"You cannot afford {activity.Name} right now.");
            return false;
        }

        if (Player.Stats.Energy < activity.EnergyCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, CaptureStats(), $"Too tired for {activity.Name} (need {activity.EnergyCost} energy, have {Player.Stats.Energy})");
            RaiseEvent($"You are too tired for {activity.Name}.");
            return false;
        }

        var location = World.GetCurrentLocation();
        if (location is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, CaptureStats(), "No current location");
            RaiseEvent("You are nowhere.");
            return false;
        }

        var availableActivities = GetAvailableEntertainmentActivities();
        if (!availableActivities.Contains(activity))
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformEntertainment", before, CaptureStats(), $"{activity.Name} not available here");
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
        RecordMutation(MutationCategories.Entertainment, "TryPerformEntertainment", before, CaptureStats(), $"{activity.Name} (cost {activity.BaseCost} LE, stress -{activity.StressReduction})");
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

    public IReadOnlyList<TrainingActivity> GetAvailableTrainingActivities()
    {
        var results = new List<TrainingActivity>();
        foreach (var activity in TrainingRegistry.AllActivities)
        {
            if (activity.RequiresHome && World.CurrentLocationId != LocationId.Home)
            {
                continue;
            }

            if (activity.RequiredNpc is NpcId npcId)
            {
                var relationship = Relationships.GetNpcRelationship(npcId);
                if (relationship.Trust < activity.RequiredTrust)
                {
                    continue;
                }
            }

            if (Player.Skills.GetLevel(activity.Skill) >= 10)
            {
                continue;
            }

            if (_trainedSkillsToday.ContainsKey(activity.Skill))
            {
                continue;
            }

            results.Add(activity);
        }

        return results;
    }

    public bool TryPerformTraining(TrainingActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        var before = CaptureStats();

        var available = GetAvailableTrainingActivities();
        if (!available.Contains(activity))
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, CaptureStats(), $"{activity.Name} not available");
            RaiseEvent($"{activity.Name} is not available right now.");
            return false;
        }

        if (Player.Stats.Energy < activity.EnergyCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, CaptureStats(), $"Too tired (need {activity.EnergyCost} energy, have {Player.Stats.Energy})");
            RaiseEvent($"You are too tired for {activity.Name}.");
            return false;
        }

        if (Player.Stats.Money < activity.MoneyCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, CaptureStats(), $"Cannot afford {activity.Name} (cost {activity.MoneyCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"You cannot afford {activity.Name} right now.");
            return false;
        }

        if (Clock.Hour < 18 || Clock.Hour >= 22)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, CaptureStats(), "Not evening hours (18:00-22:00)");
            RaiseEvent("You can only train in the evening (18:00-22:00).");
            return false;
        }

        if (Player.Skills.GetLevel(activity.Skill) >= 10)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryPerformTraining", before, CaptureStats(), $"{activity.Skill} already at max level");
            RaiseEvent($"Your {activity.Skill} is already at maximum.");
            return false;
        }

        var actualEnergyCost = activity.EnergyCost;
        var stressModifier = 0;

        if (Player.BackgroundType == BackgroundType.MedicalSchoolDropout && activity.Type == TrainingActivityType.StudyMedical)
        {
            stressModifier = -3;
        }

        if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner && activity.Type == TrainingActivityType.StreetDice)
        {
            actualEnergyCost = Math.Max(1, actualEnergyCost - 3);
        }

        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && activity.Type == TrainingActivityType.RooftopExercise)
        {
            actualEnergyCost = Math.Max(1, actualEnergyCost - 3);
        }

        if (activity.MoneyCost > 0)
        {
            Player.Stats.ModifyMoney(-activity.MoneyCost);
        }

        AdvanceTime(activity.TimeCostMinutes);
        Player.Stats.ModifyEnergy(-actualEnergyCost);

        var oldLevel = Player.Skills.GetLevel(activity.Skill);
        ApplySkillGain(activity.Skill);
        var newLevel = Player.Skills.GetLevel(activity.Skill);

        _trainedSkillsToday[activity.Skill] = true;

        if (stressModifier != 0)
        {
            Player.Stats.ModifyStress(stressModifier);
        }

        RaiseEvent(GetTrainingFlavorMessage(activity));
        RecordMutation(MutationCategories.Training, "TryPerformTraining", before, CaptureStats(), $"{activity.Name} ({activity.Skill} {oldLevel}->{newLevel})");
        return true;
    }

    public void RestoreTrainedSkillsToday(Dictionary<SkillId, bool> trainedSkillsToday)
    {
        ArgumentNullException.ThrowIfNull(trainedSkillsToday);
        _trainedSkillsToday.Clear();
        foreach (var pair in trainedSkillsToday)
        {
            _trainedSkillsToday[pair.Key] = pair.Value;
        }
    }

    public IReadOnlyDictionary<SkillId, bool> TrainedSkillsToday => _trainedSkillsToday;

    public void RestoreHomeUpgrades(IEnumerable<HomeUpgrade> upgrades)
    {
        ArgumentNullException.ThrowIfNull(upgrades);
        HomeUpgrades.Restore(upgrades);
    }

    public IReadOnlyList<HomeUpgrade> GetAvailableHomeUpgrades()
    {
        return HomeUpgradeDefinitions.AllUpgrades
            .Where(u => !HomeUpgrades.HasUpgrade(u))
            .ToList();
    }

    private static string GetTrainingFlavorMessage(TrainingActivity activity)
    {
        return activity.Type switch
        {
            TrainingActivityType.StudyMedical => "The old textbooks feel less foreign now. Knowledge settles in.",
            TrainingActivityType.PracticePersuasion => "Words sharpen. Umm Karim nods approvingly.",
            TrainingActivityType.StreetDice => "The dice talk to you differently after Youssef's lessons.",
            TrainingActivityType.RooftopExercise => "Your muscles burn, but the evening breeze makes it bearable.",
            _ => $"You practiced {activity.Name}."
        };
    }

    public JobResult WorkJob(JobShift job, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(job);

        var before = CaptureStats();
        var location = World.GetCurrentLocation();
        if (location is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, CaptureStats(), "No current location");
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
            RecordMutation(MutationCategories.Work, "WorkJob", before, CaptureStats(), result.Message);
        }
        else
        {
            RaiseEvent(result.Message);
            RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, CaptureStats(), result.Message);
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

        var schedule = GetCurrentSchedule();
        return Jobs.GetAvailableJobs(location, Player, Relationships, JobProgress)
            .Where(job => !schedule.BlockedJobTypes.Contains(job.Type.ToString()))
            .Select(job => ApplyDayScheduleToJob(ApplyDistrictConditionToJob(job), schedule))
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

        var before = CaptureStats();
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
        RecordMutation(MutationCategories.Crime, "CommitCrime", before, CaptureStats(), $"{attempt.Type}: success={result.Success}, detected={result.Detected}");
        return result;
    }

    public bool BuyFood()
    {
        var before = CaptureStats();
        var foodCost = GetFoodCost();
        if (Player.Stats.Money < foodCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyFood", before, CaptureStats(), $"Not enough money (need {foodCost} LE, have {Player.Stats.Money} LE)");
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
        RecordMutation(MutationCategories.Food, "BuyFood", before, CaptureStats(), $"Bought food for {foodCost} LE");
        return true;
    }

    public bool BuyMedicine()
    {
        var before = CaptureStats();
        var medicineCost = GetMedicineCost();
        if (Player.Stats.Money < medicineCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyMedicine", before, CaptureStats(), $"Not enough money (need {medicineCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. Medicine costs {medicineCost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-medicineCost);
        Player.Household.AddMedicine(2);
        ApplySkillGain(SkillId.Medical);
        RaiseEvent($"Bought medicine for {medicineCost} LE. Medicine stock: {Player.Household.MedicineStock}");
        RecordMutation(MutationCategories.Shop, "BuyMedicine", before, CaptureStats(), $"Bought medicine for {medicineCost} LE");
        return true;
    }

    public bool EatAtHome()
    {
        var before = CaptureStats();
        if (!Player.Household.FeedMother())
        {
            RecordMutation(MutationCategories.GuardRejected, "EatAtHome", before, CaptureStats(), "Not enough food at home");
            RaiseEvent("There is not enough food at home.");
            return false;
        }

        Player.Nutrition.Eat(MealQuality.Basic);
        SyncLegacyHunger();
        var cookingBonus = Player.HouseholdAssets.GetHomeCookingBonus(CurrentWeek);
        if (cookingBonus > 0)
        {
            Player.Stats.ModifyStress(-cookingBonus);
        }

        RaiseEvent("You eat a simple meal at home and make sure your mother eats too.");
        if (cookingBonus > 0)
        {
            RaiseEvent($"Fresh herbs soften the meal a little. Stress -{cookingBonus}.");
        }

        RecordMutation(MutationCategories.Food, "EatAtHome", before, CaptureStats(), "Ate at home");
        return true;
    }

    public bool EatStreetFood()
    {
        var before = CaptureStats();
        var streetFoodCost = GetStreetFoodCost();
        if (Player.Stats.Money < streetFoodCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "EatStreetFood", before, CaptureStats(), $"Not enough money (need {streetFoodCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"You do not have enough money for street food. It costs {streetFoodCost} LE here.");
            return false;
        }

        Player.Stats.ModifyMoney(-streetFoodCost);
        Player.Nutrition.Eat(MealQuality.Basic);
        SyncLegacyHunger();
        RaiseEvent($"You grab a cheap meal from the street for {streetFoodCost} LE.");
        RecordMutation(MutationCategories.Food, "EatStreetFood", before, CaptureStats(), $"Ate street food for {streetFoodCost} LE");
        return true;
    }

    public void CheckOnMother()
    {
        var before = CaptureStats();
        Player.Household.CheckOnMother();
        RaiseEvent(GetMotherStatusMessage());
        RecordMutation(MutationCategories.Clinic, "CheckOnMother", before, CaptureStats(), GetMotherStatusMessage());
    }

    public bool GiveMotherMedicine()
    {
        var before = CaptureStats();
        if (!Player.Household.GiveMedicine())
        {
            RecordMutation(MutationCategories.GuardRejected, "GiveMotherMedicine", before, CaptureStats(), "No medicine available");
            RaiseEvent("You have no medicine to give.");
            return false;
        }

        RaiseEvent("You give your mother her medicine.");
        RecordMutation(MutationCategories.Clinic, "GiveMotherMedicine", before, CaptureStats(), "Gave mother medicine");
        return true;
    }

    public MotherClinicVisitResult TakeMotherToClinic()
    {
        var before = CaptureStats();
        var clinicStatus = GetCurrentLocationClinicStatus();
        if (!clinicStatus.HasClinicServices)
        {
            RecordMutation(MutationCategories.GuardRejected, "TakeMotherToClinic", before, CaptureStats(), "No clinic at this location");
            RaiseEvent("There is no clinic service at this location.");
            return new MotherClinicVisitResult(false, 0, 0);
        }

        if (!clinicStatus.IsOpenToday)
        {
            RecordMutation(MutationCategories.GuardRejected, "TakeMotherToClinic", before, CaptureStats(), $"{clinicStatus.LocationName} closed today");
            RaiseEvent($"{clinicStatus.LocationName} is closed today. Open days: {clinicStatus.OpenDaysSummary}.");
            return new MotherClinicVisitResult(false, clinicStatus.VisitCost, 0);
        }

        if (Player.Stats.Money < clinicStatus.VisitCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TakeMotherToClinic", before, CaptureStats(), $"Not enough money (need {clinicStatus.VisitCost} LE, have {Player.Stats.Money} LE)");
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

        RecordMutation(MutationCategories.Clinic, "TakeMotherToClinic", before, CaptureStats(), $"Clinic visit at {clinicStatus.LocationName} (cost {clinicStatus.VisitCost} LE, health +{healthChange})");
        return new MotherClinicVisitResult(true, clinicStatus.VisitCost, healthChange);
    }

#pragma warning disable CA1024
    public int GetFoodCost()
    {
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        var schedule = GetCurrentSchedule();
        var baseModifier = (districtCondition?.Effect.FoodCostModifier ?? 0) + schedule.FoodCostModifier;
        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && schedule.FoodCostModifier < 0)
        {
            baseModifier -= 1;
        }

        var modifiedCost = _locationPricingService.GetFoodCost(World.CurrentDistrict) + baseModifier;
        return Math.Max(1, modifiedCost);
    }

    public int GetStreetFoodCost()
    {
        var districtCondition = GetActiveDistrictConditionDefinition(World.CurrentDistrict);
        var schedule = GetCurrentSchedule();
        var baseModifier = (districtCondition?.Effect.StreetFoodCostModifier ?? 0) + schedule.FoodCostModifier;
        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && schedule.FoodCostModifier < 0)
        {
            baseModifier -= 1;
        }

        var modifiedCost = _locationPricingService.GetStreetFoodCost(World.CurrentDistrict) + baseModifier;
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
            IsOpenToday: location.ClinicOpenDays.Contains(currentDay.ToSystemDayOfWeek()),
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
            IsOpenToday: location.ClinicOpenDays.Contains(currentDay.ToSystemDayOfWeek()),
            OpenDaysSummary: FormatOpenDays(location.ClinicOpenDays),
            TravelTimeMinutes: GetTravelTimeMinutes(location),
            CanAfford: Player.Stats.Money >= totalCost,
            IsValidOption: true);
    }

    public TravelAndClinicVisitResult TravelAndTakeMotherToClinic(LocationId clinicLocationId)
    {
        var before = CaptureStats();
        var option = GetClinicTravelOption(clinicLocationId);
        if (!option.IsValidOption)
        {
            RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, CaptureStats(), "No clinic at that location");
            RaiseEvent("There is no clinic service at that location.");
            return new TravelAndClinicVisitResult(false, 0, 0, 0, 0);
        }

        if (!option.IsOpenToday)
        {
            RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, CaptureStats(), $"{option.LocationName} closed today");
            RaiseEvent($"{option.LocationName} is closed today. Open days: {option.OpenDaysSummary}.");
            return new TravelAndClinicVisitResult(false, option.TravelCost, option.ClinicCost, option.TotalCost, 0);
        }

        if (Player.Stats.Money < option.TotalCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, CaptureStats(), $"Not enough money (need {option.TotalCost} LE, have {Player.Stats.Money} LE)");
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

        RecordMutation(MutationCategories.Clinic, "TravelAndTakeMotherToClinic", before, CaptureStats(), $"Travel+clinic to {option.LocationName} (total cost {option.TravelCost + clinicResult.TotalCost} LE)");
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
        var schedule = GetCurrentSchedule();
        var scheduleDiscount = schedule.ClinicDiscount ? schedule.ClinicDiscountAmount : 0;
        if (scheduleDiscount > 0 && Player.BackgroundType == BackgroundType.MedicalSchoolDropout)
        {
            scheduleDiscount *= 2;
        }

        var modifiedCost = _locationPricingService.GetClinicVisitCost(location, Relationships, Player.Skills) + (districtCondition?.Effect.ClinicVisitCostModifier ?? 0) - scheduleDiscount;
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

    public int CurrentWeek => ((Clock.Day - 1) / 7) + 1;

    public bool CanUseHouseholdAssets()
    {
        return World.CurrentLocationId == LocationId.FishMarket
            || World.CurrentLocationId == LocationId.PlantShop
            || (World.CurrentLocationId == LocationId.Home
                && (Player.HouseholdAssets.HasAnyAssets || Player.HouseholdAssets.HasStreetCatEncounter));
    }

    public bool AdoptStreetCat()
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "AdoptStreetCat", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to be home to bring a street cat inside.");
            return false;
        }

        if (!Player.HouseholdAssets.AdoptCat(Clock.Day, CurrentWeek))
        {
            RecordMutation(MutationCategories.GuardRejected, "AdoptStreetCat", before, CaptureStats(), "No cat encounter available");
            RaiseEvent("No stray cat is trusting you enough to come home right now.");
            return false;
        }

        RaiseEvent("The cat slips inside, claims a corner, and your mother smiles for the first time all day.");
        RecordMutation(MutationCategories.HouseholdAsset, "AdoptStreetCat", before, CaptureStats(), "Adopted street cat");
        return true;
    }

    public bool BuyFishTank()
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.FishMarket)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyFishTank", before, CaptureStats(), "Not at fish market");
            RaiseEvent("You need to be at the fish market to buy a tank.");
            return false;
        }

        if (!Player.HouseholdAssets.CanBuyFishTank)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyFishTank", before, CaptureStats(), "Already have a fish tank");
            RaiseEvent("There is already a fish tank at home.");
            return false;
        }

        var definition = PetRegistry.GetByType(PetType.Fish);
        if (Player.Stats.Money < definition.OneTimeCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyFishTank", before, CaptureStats(), $"Not enough money (need {definition.OneTimeCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. A fish tank costs {definition.OneTimeCost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-definition.OneTimeCost);
        Player.HouseholdAssets.BuyFishTank(Clock.Day, CurrentWeek);
        RaiseEvent($"You carry a modest fish tank home from the market for {definition.OneTimeCost} LE.");
        RecordMutation(MutationCategories.HouseholdAsset, "BuyFishTank", before, CaptureStats(), $"Bought fish tank for {definition.OneTimeCost} LE");
        return true;
    }

    public bool BuyPlant(PlantType plantType)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.PlantShop)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyPlant", before, CaptureStats(), "Not at plant shop");
            RaiseEvent("You need to be at the plant shop to buy plants.");
            return false;
        }

        if (!Player.HouseholdAssets.CanBuyPlant)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyPlant", before, CaptureStats(), "No room for more plants");
            RaiseEvent("There is no room left for more plants at home.");
            return false;
        }

        var definition = PlantRegistry.GetByType(plantType);
        if (Player.Stats.Money < definition.OneTimeCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyPlant", before, CaptureStats(), $"Not enough money (need {definition.OneTimeCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. {definition.Name} costs {definition.OneTimeCost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-definition.OneTimeCost);
        Player.HouseholdAssets.BuyPlant(plantType, Clock.Day, CurrentWeek);
        RaiseEvent($"You buy {definition.Name} for {definition.OneTimeCost} LE and carry it back home.");
        RecordMutation(MutationCategories.HouseholdAsset, "BuyPlant", before, CaptureStats(), $"Bought {definition.Name} for {definition.OneTimeCost} LE");
        return true;
    }

    public bool PayPetCare()
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "PayPetCare", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to be home to sort out pet care.");
            return false;
        }

        var cost = Player.HouseholdAssets.GetPetCareCostDue(CurrentWeek);
        if (cost <= 0)
        {
            RecordMutation(MutationCategories.GuardRejected, "PayPetCare", before, CaptureStats(), "Pet care already covered");
            RaiseEvent("Pet care is already covered for this week.");
            return false;
        }

        if (Player.Stats.Money < cost)
        {
            RecordMutation(MutationCategories.GuardRejected, "PayPetCare", before, CaptureStats(), $"Not enough money (need {cost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. Pet food for the week costs {cost} LE.");
            return false;
        }

        Player.Stats.ModifyMoney(-cost);
        Player.HouseholdAssets.PayPetCare(CurrentWeek);
        RaiseEvent($"You cover this week's pet food and care supplies for {cost} LE.");
        RecordMutation(MutationCategories.HouseholdAsset, "PayPetCare", before, CaptureStats(), $"Paid pet care {cost} LE");
        return true;
    }

    public bool PayPlantCare()
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "PayPlantCare", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to be home to water and supply the plants.");
            return false;
        }

        var cost = Player.HouseholdAssets.GetPlantCareCostDue(CurrentWeek);
        if (cost <= 0)
        {
            RecordMutation(MutationCategories.GuardRejected, "PayPlantCare", before, CaptureStats(), "Plant care already covered");
            RaiseEvent("Plant care is already covered for this week.");
            return false;
        }

        if (Player.Stats.Money < cost)
        {
            RecordMutation(MutationCategories.GuardRejected, "PayPlantCare", before, CaptureStats(), $"Not enough money (need {cost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. Plant care supplies cost {cost} LE this week.");
            return false;
        }

        Player.Stats.ModifyMoney(-cost);
        Player.HouseholdAssets.PayPlantCare(CurrentWeek);
        RaiseEvent($"You pay {cost} LE to keep the plants watered and supplied this week.");
        RecordMutation(MutationCategories.HouseholdAsset, "PayPlantCare", before, CaptureStats(), $"Paid plant care {cost} LE");
        return true;
    }

    public bool UpgradePlant(Guid plantId, PlantUpgradeType upgradeType)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to be home to work on the plants.");
            return false;
        }

        var plant = Player.HouseholdAssets.GetPlant(plantId);
        if (plant is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, CaptureStats(), "Plant not found");
            RaiseEvent("That plant is not in your flat anymore.");
            return false;
        }

        var cost = PlantUpgradeCatalog.GetCost(upgradeType);
        if (Player.Stats.Money < cost)
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, CaptureStats(), $"Not enough money (need {cost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. {PlantUpgradeCatalog.GetName(upgradeType)} costs {cost} LE.");
            return false;
        }

        if (!Player.HouseholdAssets.TryUpgradePlant(plantId, upgradeType, CurrentWeek))
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, CaptureStats(), $"{PlantUpgradeCatalog.GetName(upgradeType)} already active");
            RaiseEvent($"{PlantUpgradeCatalog.GetName(upgradeType)} is already active for that plant.");
            return false;
        }

        Player.Stats.ModifyMoney(-cost);
        var definition = PlantRegistry.GetByType(plant.Type);
        RaiseEvent($"{definition.Name}: {PlantUpgradeCatalog.GetName(upgradeType)} added for {cost} LE.");
        RecordMutation(MutationCategories.HouseholdAsset, "UpgradePlant", before, CaptureStats(), $"Upgraded {definition.Name} with {PlantUpgradeCatalog.GetName(upgradeType)} for {cost} LE");
        return true;
    }

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

    public void RestoreHouseholdAssetsState(
        IEnumerable<OwnedPet> pets,
        IEnumerable<OwnedPlant> plants,
        bool hasStreetCatEncounter,
        int lastStreetCatEncounterDay,
        int totalHerbEarnings)
    {
        Player.HouseholdAssets.Restore(pets, plants, hasStreetCatEncounter, lastStreetCatEncounterDay, totalHerbEarnings);
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
            new ActiveDistrictCondition { District = DistrictId.Shubra, ConditionId = "shubra_steady_day" },
            new ActiveDistrictCondition { District = DistrictId.DowntownCairo, ConditionId = "downtown_cairo_steady_day" }
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
        var schedule = GetCurrentSchedule();
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
            job = ApplyDistrictConditionToJob(job);
        }

        if (hasScheduleModifiers)
        {
            job = ApplyDayScheduleToJob(job, schedule);
        }

        return new JobPreview(
            job,
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

        return CloneJobShift(
            job,
            Math.Max(0, job.BasePay + payModifier),
            job.StressCost);
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

        var before = CaptureStats();
        EndingId = ending;
        IsGameOver = true;
        GameOverReason = EndingService.GetMessage(ending.Value);
        PendingEndingKnot = EndingService.GetInkKnot(this, ending.Value);
        RecordMutation(MutationCategories.EndingTriggered, "CheckGameOverConditions", before, CaptureStats(), $"Ending triggered: {ending}");
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

        var schedule = GetCurrentSchedule();
        if (schedule.CrimeDetectionModifier != 0)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Clamp(modifiedAttempt.DetectionRisk + schedule.CrimeDetectionModifier, 1, 95)
            };
            activeModifiers.Add($"{schedule.DayName}: crime detection {schedule.CrimeDetectionModifier} (schedule effect).");
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

        var before = CaptureStats();
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

        RecordMutation(MutationCategories.RandomEvent, "ApplyRandomEvent", before, CaptureStats(), $"Event: {randomEvent.Id} - {randomEvent.Description}");
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

    private GameDayOfWeek GetCurrentDayOfWeek()
    {
        return Clock.DayOfWeek;
    }

    public DayScheduleModifiers GetCurrentSchedule()
    {
        return DayScheduleRegistry.GetModifiers(GetCurrentDayOfWeek());
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

    private Dictionary<string, object?> CaptureStats() => new()
    {
        ["Money"] = Player.Stats.Money,
        ["Hunger"] = Player.Stats.Hunger,
        ["Energy"] = Player.Stats.Energy,
        ["Health"] = Player.Stats.Health,
        ["Stress"] = Player.Stats.Stress,
        ["MotherHealth"] = Player.Household.MotherHealth,
        ["PolicePressure"] = PolicePressure,
        ["Day"] = CurrentDay,
        ["Location"] = World.CurrentLocationId.ToString(),
        ["FoodStockpile"] = Player.Household.FoodStockpile,
        ["RentDaysUnpaid"] = UnpaidRentDays,
    };

    private void RecordMutation(string category, string action, Dictionary<string, object?> before, Dictionary<string, object?> after, string reason)
    {
        var record = new GameMutationRecord(RunId, DateTimeOffset.UtcNow, category, action, before, after, reason);
        _mutations.Add(record);
        MutationRecorded?.Invoke(this, new GameMutationEventArgs(record));
    }

    private void RaiseEvent(string message)
    {
        GameEvent?.Invoke(this, new GameEventArgs(message));
    }

    private void RaiseAutoTransaction(string message)
    {
        RaiseEvent($"[Day {CurrentDay}] {message}");
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
        var before = CaptureStats();
        var definition = InvestmentRegistry.GetByType(type);
        if (definition is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "MakeInvestment", before, CaptureStats(), $"Unknown investment type: {type}");
            return new MakeInvestmentResult(false, 0, "Unknown investment type.");
        }

        var eligibility = CheckInvestmentEligibility(definition);
        if (!eligibility.IsEligible)
        {
            RecordMutation(MutationCategories.GuardRejected, "MakeInvestment", before, CaptureStats(), string.Join(" ", eligibility.FailureReasons));
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

        RecordMutation(MutationCategories.Investment, "MakeInvestment", before, CaptureStats(), $"Invested {definition.Cost} LE in {definition.Name}");
        return new MakeInvestmentResult(true, definition.Cost, $"Successfully invested in {definition.Name}.");
    }

    public InvestmentResolutionSummary ResolveWeeklyInvestments(Random? random = null)
    {
        var before = CaptureStats();
        var rng = random ?? _sharedRandom;
        var summary = new InvestmentResolutionSummary();
        var schedule = GetCurrentSchedule();

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

            if (result.Income > 0 && schedule.InvestmentRevenueModifier != 0)
            {
                result = result with { Income = Math.Max(0, result.Income + schedule.InvestmentRevenueModifier) };
            }
            summary.AddResult(result);

            if (result.WasLost)
            {
                toRemove.Add(investment);
            }

            if (result.Income > 0)
            {
                Player.Stats.ModifyMoney(result.Income);
                TotalInvestmentEarnings += result.Income;
                if (!result.WasLost && result.ExtortionPaid == 0 && result.PolicePressureIncrease == 0)
                {
                    var investmentDef = InvestmentRegistry.GetByType(investment.Type);
                    var investmentName = investmentDef?.Name ?? investment.Type.ToString();
                    RaiseAutoTransaction($"{investmentName}: +{result.Income} LE weekly income.");
                }
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
                RaiseAutoTransaction(result.Message);
            }
        }

        foreach (var investment in toRemove)
        {
            _investmentState.ActiveInvestments.Remove(investment);
        }

        if (summary.TotalIncome > 0 || summary.TotalLosses > 0 || summary.TotalExtortion > 0)
        {
            RaiseAutoTransaction($"Weekly investments: +{summary.TotalIncome} LE income, -{summary.TotalExtortion} LE extortion, {summary.LostCount} lost.");
        }

        RecordMutation(MutationCategories.Investment, "ResolveWeeklyInvestments", before, CaptureStats(), $"Income +{summary.TotalIncome}, Extortion -{summary.TotalExtortion}, Lost {summary.LostCount}");
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

    private void ResolveWeeklyHouseholdAssets()
    {
        var resolution = Player.HouseholdAssets.ResolveWeeklyNeglect(CurrentWeek);
        if (resolution.StressPenalty <= 0)
        {
            return;
        }

        Player.Stats.ModifyStress(resolution.StressPenalty);
        RaiseAutoTransaction($"Skipping household care all week weighs on your mother. Stress +{resolution.StressPenalty}.");
    }

    private void TryRollStreetCatEncounter(Random random)
    {
#pragma warning disable CA5394
        ArgumentNullException.ThrowIfNull(random);

        if (World.CurrentLocationId != LocationId.Home || Clock.Day < 3)
        {
            return;
        }

        if (random.NextDouble() >= 0.15)
        {
            return;
        }

        if (Player.HouseholdAssets.TryTriggerStreetCatEncounter(Clock.Day))
        {
            RaiseEvent("A street cat starts waiting near your building door as if it has already chosen you.");
        }
#pragma warning restore CA5394
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
