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
using Slums.Core.Calendar;
using Slums.Core.Community;
using Slums.Core.Home;
using Slums.Core.Rumors;
using Slums.Core.Weather;
using Slums.Core.Heat;
using Slums.Core.Territory;
using Slums.Core.Economy;
using Slums.Core.World;
using Slums.Core.Phone;
using Slums.Core.Information;
using Slums.Core.Robotics;
using Slums.Core.Inventory;
using Slums.Core.Technology;
using Slums.Core.World.News;

using Slums.Core.Randomness;
using Slums.Core.Diagnostics;
using Slums.Core.State.DailyResolution;
using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

namespace Slums.Core.State;

public sealed partial class GameSession : INarrativeOutcomeTarget
{
    public const int ConversationDurationMinutes = 45;
    public const int EmergencySupportDurationMinutes = 60;
    private readonly CrimeService _crimeService = new();
    private readonly RandomEventService _randomEventService = new();
    private readonly PlayerIdentityState _playerIdentity;
    private readonly GameRunState _runState;
    private readonly GameCrimeState _crimeState;
    private readonly GameWorkState _workState;
    private readonly GameNarrativeState _narrativeState;
    private readonly GameInvestmentState _investmentState;
    private readonly RentState _rentState;
    private Random _sharedRandom;
    private readonly LocationPricingService _locationPricingService;
    private readonly Queue<string> _pendingNarrativeScenes;
    private readonly HashSet<string> _storyFlags;
    private readonly Dictionary<string, int> _randomEventHistory;
    private readonly bool _useDynamicDistrictConditions;
    private readonly List<GameMutationRecord> _mutations = [];
    private readonly Dictionary<SkillId, bool> _trainedSkillsToday = [];

    internal CrimeService CrimeService => _crimeService;
    internal GameCrimeState CrimeState => _crimeState;

    public GameSession(Random? sharedRandom = null)
    {
        Clock = new GameClock();
        _playerIdentity = new PlayerIdentityState();
        Player = new PlayerCharacter(_playerIdentity, new SurvivalStats(), new NutritionState(), new HouseholdCareState(), new HouseholdAssetsState(), new SkillState(), new RoboticsState());
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
#pragma warning disable CA5394 // Gameplay randomness does not require cryptographic strength
        _sharedRandom = sharedRandom ?? new GameRandom((ulong)Random.Shared.NextInt64());
#pragma warning restore CA5394
        _locationPricingService = new LocationPricingService();
        _pendingNarrativeScenes = _narrativeState.PendingNarrativeScenes;
        _storyFlags = _narrativeState.StoryFlags;
        _randomEventHistory = _narrativeState.RandomEventHistory;
        Territory.Initialize(_playerIdentity.BackgroundType);
        NpcEconomies.Initialize();
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

    /// <summary>Structured journal of events and automatic transactions, persisted with saves.</summary>
    public EventJournal EventJournal { get; } = new();

    /// <summary>The session-owned random source; all gameplay randomness flows through it.</summary>
    public Random SharedRandom => _sharedRandom;

    /// <summary>
    /// Serializable state of the shared random source, or <c>null</c> when the source is not a
    /// <see cref="GameRandom"/> (e.g. a session constructed with an external plain <see cref="Random"/>).
    /// </summary>
    public GameRandomState? RandomState => (_sharedRandom as GameRandom)?.CaptureState();

    /// <summary>Restores the shared random source to an exact captured state.</summary>
    internal void RestoreRandomState(GameRandomState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _sharedRandom = new GameRandom(state);
    }

    public GameClock Clock { get; }
    public PlayerCharacter Player { get; }
    public WorldState World { get; }
    public RelationshipState Relationships { get; }
    public JobProgressState JobProgress { get; }
    public JobService Jobs { get; }
    public bool IsGameOver { get => _runState.IsGameOver; internal set => _runState.IsGameOver = value; }
    public string? GameOverReason { get => _runState.GameOverReason; internal set => _runState.GameOverReason = value; }
    public EndingId? EndingId { get => _runState.EndingId; internal set => _runState.EndingId = value; }
    public int PolicePressure => DistrictHeat.GetGlobalPressure();
    public int TotalCrimeEarnings { get => _crimeState.TotalCrimeEarnings; private set => _crimeState.TotalCrimeEarnings = value; }
    public int CrimesCommitted { get => _crimeState.CrimesCommitted; private set => _crimeState.CrimesCommitted = value; }
    public int TotalHonestWorkEarnings { get => _workState.TotalHonestWorkEarnings; private set => _workState.TotalHonestWorkEarnings = value; }
    public int HonestShiftsCompleted { get => _workState.HonestShiftsCompleted; private set => _workState.HonestShiftsCompleted = value; }
    public int DaysSurvived { get => _runState.DaysSurvived; internal set => _runState.DaysSurvived = value; }
    public int LastCrimeDay { get => _crimeState.LastCrimeDay; private set => _crimeState.LastCrimeDay = value; }
    public int LastHonestWorkDay { get => _workState.LastHonestWorkDay; private set => _workState.LastHonestWorkDay = value; }
    public int LastPublicFacingWorkDay { get => _workState.LastPublicFacingWorkDay; private set => _workState.LastPublicFacingWorkDay = value; }
    public IReadOnlyCollection<string> StoryFlags => _storyFlags;
    public IReadOnlyDictionary<string, int> RandomEventHistory => _randomEventHistory;
    public bool HasCrimeCommittedToday => CrimeCommittedToday;
    public bool HasClaimedEmergencySupport => _runState.EmergencySupportClaimed;
    public bool CanRequestEmergencySupport => Clock.Day <= 7 && Player.HasSelectedBackground && !HasClaimedEmergencySupport;
    public string? PendingEndingKnot { get => _runState.PendingEndingKnot; internal set => _runState.PendingEndingKnot = value; }

    public EndingId? PendingEndingId { get => _runState.PendingEndingId; internal set => _runState.PendingEndingId = value; }

    public string? FinalSacrifice { get => _runState.FinalSacrifice; internal set => _runState.FinalSacrifice = value; }
    public IReadOnlyList<Investment> ActiveInvestments => _investmentState.ActiveInvestments;
    public int TotalInvestmentEarnings { get => _investmentState.TotalInvestmentEarnings; private set => _investmentState.TotalInvestmentEarnings = value; }
    public int TotalHerbEarnings => Player.HouseholdAssets.TotalHerbEarnings;
    public int UnpaidRentDays => _rentState.UnpaidRentDays;
    public int AccumulatedRentDebt => _rentState.AccumulatedRentDebt;
    public int RentGraceDaysRemaining => _rentState.GraceDaysRemaining;
    public bool FirstWarningGiven => _rentState.FirstWarningGiven;
    public bool FinalWarningGiven => _rentState.FinalWarningGiven;
    private bool CrimeCommittedToday { get => _crimeState.CrimeCommittedToday; set => _crimeState.CrimeCommittedToday = value; }
    public HomeUpgradeState HomeUpgrades { get; } = new();
    public CommunityEventAttendance EventAttendance { get; } = new();
    public WeatherState CurrentWeather { get; internal set; } = WeatherState.Clear;
    public RumorState Rumors { get; } = new();
    public DistrictHeatState DistrictHeat { get; } = new();
    public TerritoryState Territory { get; } = new();
    public NpcEconomyState NpcEconomies { get; } = new();
    public PlayerDebtState PlayerDebts { get; } = new();
    public PhoneState Phone { get; } = new();
    public PhoneMessageState PhoneMessages { get; } = new();
    public TipState Tips { get; } = new();
    public NewsState News { get; } = new();
    public InfrastructureState Infrastructure { get; } = new();
    public CommunityAdaptationState CommunityAdaptation { get; } = new();
    public CityCrisisState CityCrisis { get; } = new();
    public CentralCharacterArcState CentralCharacterArcs { get; } = new();
    public TechnologyObligationState Technology { get; } = new();
    public InventoryState Inventory { get; } = new();
    public RamadanState RamadanState { get; internal set; } = RamadanState.Inactive;

    public IReadOnlyList<ActiveNewsFlash> ActiveNews => News.ActiveFlashes;

    public IReadOnlyList<NewsFlashDefinition> GetActiveNewsDefinitions()
    {
        return News.ActiveFlashes
            .Select(static flash => NewsRegistry.GetById(flash.DefinitionId))
            .OfType<NewsFlashDefinition>()
            .ToArray();
    }

    public IReadOnlyList<NpcAvailability> GetNpcAvailability()
    {
        return NpcAvailabilityResolver.ResolveAll(
            Clock,
            World.CurrentLocationId,
            NpcScheduleRegistry.All,
            NewsImpactCalculator.GetActiveNewsIds(News));
    }

    public event EventHandler<GameEventArgs>? GameEvent;
    public IReadOnlyList<GameMutationRecord> Mutations => _mutations;
    public event EventHandler<GameMutationEventArgs>? MutationRecorded;

    public IReadOnlyList<InvestmentDefinition> GetCurrentInvestmentOpportunities()
        => InvestmentPurchaseService.GetCurrentOpportunities(this);

    public IReadOnlyList<EndingId> GetAvailableEndingChoices()
        => EndingCommitmentService.GetAvailableEndingChoices(this);

    public bool TryChooseEnding(EndingId endingId)
        => EndingCommitmentService.TryChooseEnding(this, endingId);

    public void CommitEnding(EndingId endingId, string sacrifice)
        => EndingCommitmentService.CommitEnding(this, endingId, sacrifice);

    /// <summary>Daily-pipeline access to the random event service.</summary>
    internal RandomEventService RandomEventService => _randomEventService;
    internal LocationPricingService LocationPricing => _locationPricingService;
    internal RentState Rent => _rentState;
    internal GameInvestmentState InvestmentState => _investmentState;
    internal GameWorkState WorkState => _workState;

    internal void ClaimEmergencySupport()
    {
        _runState.EmergencySupportClaimed = true;
    }

    /// <summary>Processes one day of rent against the session's recurring rent cost.</summary>
    internal RentResult ProcessRentDay()
    {
        return _rentState.ProcessDay(RecurringExpenses.DailyRentCost, Player.Stats.Money);
    }

    /// <summary>Clears the per-day training flags during the daily resolution.</summary>
    internal void ClearDailyTraining()
    {
        TrainingService.ClearDaily(this);
    }

    /// <summary>Indicates whether this run rolls dynamic daily district conditions.</summary>
    internal bool UseDynamicDistrictConditions => _useDynamicDistrictConditions;

    /// <summary>Starts a new day in the crime activity ledger during the daily resolution.</summary>
    internal void BeginDailyActivityLedger()
    {
        ActivityLedgerSystem.BeginNewDay(_crimeState);
    }

    internal void QueueCityCrisisBeat()
    {
        var beat = CityCrisisPlanner.GetNextBeat(Clock.Day, CityCrisis);
        if (beat is null)
        {
            return;
        }

        CityCrisis.MarkBeatQueued();
        QueueNarrativeScene(beat.KnotName);
        RaiseEvent($"Crisis update: {beat.Phase}.");
    }

    internal void RestoreCityCrisisState(
        int beatIndex,
        int evidenceCollected,
        int resourcesCommitted,
        int cooperativeCondition,
        CityCrisisDecision decision,
        CityCrisisResolution resolution,
        int decisionDay = 0,
        int callbackDueDay = 0,
        CityCrisisDecision pendingCallbackDecision = CityCrisisDecision.None,
        bool callbackQueued = false)
    {
        CityCrisis.Restore(
            beatIndex,
            evidenceCollected,
            resourcesCommitted,
            cooperativeCondition,
            decision,
            resolution,
            decisionDay,
            callbackDueDay,
            pendingCallbackDecision,
            callbackQueued);
    }

    internal void RollTerritoryEvents(Random random)
        => TerritoryEventRoller.Roll(this, random);

    public bool RestAtHome()
    {
        return HomeUpgradeService.RestAtHome(this);
    }

    public bool TryPurchaseHomeUpgrade(HomeUpgrade upgrade)
    {
        return HomeUpgradeService.Purchase(this, upgrade);
    }

    public bool TryTravelTo(LocationId locationId)
        => TravelService.TryTravelTo(this, locationId);

    public bool TryWalkTo(LocationId locationId)
        => TravelService.TryWalkTo(this, locationId);

    public bool CanAffordTravel(LocationId locationId)
        => TravelService.CanAfford(this, locationId);

    public IReadOnlyList<EntertainmentActivity> GetAvailableEntertainmentActivities()
    {
        return EntertainmentService.GetAvailableActivities(this);
    }

    public bool TryPerformEntertainment(EntertainmentActivity activity)
    {
        return EntertainmentService.Perform(this, activity);
    }

    public IReadOnlyList<CommunityEventDefinition> GetAvailableCommunityEvents()
        => CommunityEventService.GetAvailable(this);

    public bool AttendCommunityEvent(CommunityEventId eventId, Random? random = null)
        => CommunityEventService.Attend(this, eventId, random);

    public IReadOnlyList<CommunityActionPreview> GetCommunityActionPreviews()
        => CommunityOrganizingService.GetPreviews(this);

    public CommunityActionPreview PreviewCommunityAction(CommunityActionType actionType)
        => CommunityOrganizingService.Preview(this, actionType);

    public bool PerformCommunityAction(CommunityActionType actionType)
        => CommunityOrganizingService.Perform(this, actionType);

    public IReadOnlyList<TechnicalRepairPreview> GetTechnicalRepairPreviews()
        => TechnicalRepairService.GetPreviews(this);

    public TechnicalRepairPreview PreviewTechnicalRepair(TechnicalRepairActionType actionType)
        => TechnicalRepairService.Preview(this, actionType);

    public bool PerformTechnicalRepair(TechnicalRepairActionType actionType)
        => TechnicalRepairService.Perform(this, actionType);

    public IReadOnlyList<DigitalServicePreview> GetDigitalServicePreviews()
        => DigitalServiceService.GetPreviews(this);

    public DigitalServicePreview PreviewDigitalService(DigitalServiceActionType actionType)
        => DigitalServiceService.Preview(this, actionType);

    public bool PerformDigitalService(DigitalServiceActionType actionType)
        => DigitalServiceService.Perform(this, actionType);

    /// <summary>
    /// Accepts one early-run emergency support package tied to the selected background.
    /// </summary>
    public bool RequestEmergencySupport()
        => CommunityEventService.RequestEmergencySupport(this);

    public IReadOnlyList<TrainingActivity> GetAvailableTrainingActivities()
    {
        return TrainingService.GetAvailable(this);
    }

    public bool TryPerformTraining(TrainingActivity activity)
    {
        return TrainingService.Perform(this, activity);
    }

    public void ApplyGenderRelationshipModifiers()
    {
        foreach (var npcId in Enum.GetValues<NpcId>())
        {
            var modifier = GenderModifiers.NpcStartingTrustModifier(Player.Gender, npcId);
            if (modifier != 0)
            {
                Relationships.ModifyNpcTrust(npcId, modifier);
            }
        }
    }

    internal void RestoreTrainedSkillsToday(Dictionary<SkillId, bool> trainedSkillsToday)
    {
        TrainingService.Restore(this, trainedSkillsToday);
    }

    public IReadOnlyDictionary<SkillId, bool> TrainedSkillsToday => _trainedSkillsToday;
    internal Dictionary<SkillId, bool> TrainedSkillsTodayMutable => _trainedSkillsToday;

    internal void RestoreHomeUpgrades(IEnumerable<HomeUpgrade> upgrades)
    {
        HomeUpgradeService.Restore(this, upgrades);
    }

    public IReadOnlyList<HomeUpgrade> GetAvailableHomeUpgrades()
    {
        return HomeUpgradeService.GetAvailable(this);
    }

    public JobResult WorkJob(JobShift job, Random? random = null)
        => WorkSessionService.Work(this, job, random);

    public IReadOnlyList<JobShift> GetAvailableJobs()
        => WorkSessionService.GetAvailable(this);

    public IReadOnlyList<CrimeAttempt> GetAvailableCrimes()
        => CrimeSessionService.GetAvailableCrimes(this);

    public string? GetCrimeBlockReason()
        => CrimeSessionService.GetCrimeBlockReason(this);

    public CrimeResult CommitCrime(CrimeAttempt attempt, Random? random = null)
        => CrimeSessionService.CommitCrime(this, attempt, random);

    public bool BuyFood()
    {
        return FoodShopService.BuyFood(this);
    }

    public bool BuyMedicine()
    {
        return FoodShopService.BuyMedicine(this);
    }

    public bool EatAtHome()
        => MealService.EatAtHome(this);

    public bool EatStreetFood()
        => MealService.EatStreetFood(this);

    public void CheckOnMother()
        => ClinicVisitService.CheckOnMother(this);

    public bool GiveMotherMedicine()
        => ClinicVisitService.GiveMotherMedicine(this);

    public MotherClinicVisitResult TakeMotherToClinic()
        => ClinicVisitService.TakeMotherToClinic(this);

#pragma warning disable CA1024
    public int GetFoodCost()
    {
        return FoodShopService.GetFoodCost(this);
    }

    public ProvisioningMealPlan GetProvisioningMealPlan()
    {
        return ProvisioningCalculator.GetMealPlan(
            Player.Skills.GetLevel(SkillId.Provisioning),
            Player.HouseholdAssets.GetHomeCookingBonus(CurrentWeek));
    }

    public int GetStreetFoodCost()
    {
        return FoodShopService.GetStreetFoodCost(this);
    }

    public CurrentLocationClinicStatus GetCurrentLocationClinicStatus()
        => ClinicVisitService.GetCurrentLocationClinicStatus(this);
#pragma warning restore CA1024

#pragma warning disable CA1822
    public IReadOnlyList<Location> GetClinicLocations()
#pragma warning restore CA1822
        => ClinicVisitService.GetClinicLocations(this);

    public ClinicTravelOption GetClinicTravelOption(LocationId clinicLocationId)
        => ClinicVisitService.GetClinicTravelOption(this, clinicLocationId);

    public TravelAndClinicVisitResult TravelAndTakeMotherToClinic(LocationId clinicLocationId)
        => ClinicVisitService.TravelAndTakeMotherToClinic(this, clinicLocationId);

    public int GetMedicineCost()
    {
        return FoodShopService.GetMedicineCost(this);
    }

    public JobPreview PreviewJob(JobType jobType)
        => WorkSessionService.Preview(this, jobType);

    public IReadOnlyList<DistrictConditionDefinition> GetDailyDistrictConditions()
        => DistrictConditionRoller.GetDailyConditions(this);

    public DistrictConditionDefinition? GetActiveDistrictConditionDefinition(DistrictId districtId)
        => DistrictConditionRoller.GetActiveCondition(this, districtId);

    public int GetTravelCost(LocationId locationId)
        => TravelService.GetTravelCost(this, locationId);

    public int GetTravelTimeMinutes(LocationId locationId)
        => TravelService.GetTravelTimeMinutes(this, locationId);

    public int GetWalkTimeMinutes(LocationId locationId)
        => TravelService.GetWalkTimeMinutes(this, locationId);

    public string? GetTravelConditionSummary(LocationId locationId)
        => TravelService.GetTravelConditionSummary(this, locationId);

    public int CurrentDay => Clock.Day;

    public int CurrentWeek => CalendarService.GetCurrentWeek(this);

    public bool CanUseHouseholdAssets()
        => HouseholdAssetsService.CanUse(this);

    public bool AdoptStreetCat()
        => HouseholdAssetsService.AdoptStreetCat(this);

    public bool BuyFishTank()
        => HouseholdAssetsService.BuyFishTank(this);

    public bool BuyPlant(PlantType plantType)
        => HouseholdAssetsService.BuyPlant(this, plantType);

    public bool BuyRobot(RobotType robotType)
        => HouseholdAssetsService.BuyRobot(this, robotType);

    public bool BuyRobotParts(int quantity = 1)
        => HouseholdAssetsService.BuyRobotParts(this, quantity);

    public bool RepairRobot(Guid robotId)
        => HouseholdAssetsService.RepairRobot(this, robotId);

    public bool PayPetCare()
        => HouseholdAssetsService.PayPetCare(this);

    public bool PayPlantCare()
        => HouseholdAssetsService.PayPlantCare(this);

    public bool UpgradePlant(Guid plantId, PlantUpgradeType upgradeType)
        => HouseholdAssetsService.UpgradePlant(this, plantId, upgradeType);

    public bool UpgradeFishTank(FishTankUpgradeType upgradeType)
        => HouseholdAssetsService.UpgradeFishTank(this, upgradeType);

    public IReadOnlyList<NpcId> GetReachableNpcs()
    {
        return NpcRegistry.GetReachableNpcs(World.CurrentLocationId, PolicePressure);
    }

    public void AdjustMoney(int delta)
    {
        Player.Stats.ModifyMoney(delta);
    }

    public void ApplyRentPayment(int amount)
        => DebtAndLoanService.ApplyRentPayment(this, amount);

    public void GrantRentGraceDays(int days)
        => DebtAndLoanService.GrantRentGraceDays(this, days);

    public void ApplyDebtPayment(DebtSource source, int amount)
        => DebtAndLoanService.ApplyDebtPayment(this, source, amount);

    public void ExtendDebtDueDate(DebtSource source, int days)
        => DebtAndLoanService.ExtendDebtDueDate(this, source, days);

    /// <summary>
    /// Applies one complete authored narrative outcome inside the session mutation boundary.
    /// </summary>
    /// <param name="sourceKnot">The Ink knot that produced the outcome.</param>
    /// <param name="effectReason">The authored message or reason displayed for the outcome.</param>
    /// <param name="applyOutcome">The application-layer effect adapter.</param>
    public void ApplyNarrativeOutcome(string sourceKnot, string? effectReason, Action<INarrativeOutcomeTarget> applyOutcome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKnot);
        ArgumentNullException.ThrowIfNull(applyOutcome);

        var before = CaptureStats();
        applyOutcome(this);
        CheckGameOverConditions();
        RecordMutation(
            MutationCategories.Narrative,
            "ApplyNarrativeOutcome",
            before,
            CaptureStats(),
            string.IsNullOrWhiteSpace(effectReason)
                ? $"Applied Ink outcome from '{sourceKnot}'"
                : $"Applied Ink outcome from '{sourceKnot}': {effectReason}");
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

    internal void RestoreStoryFlags(IEnumerable<string> flags)
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

    internal bool TryQueueNarrativeTrigger(NarrativeSceneTrigger? trigger)
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
        => EndingCommitmentService.TryTakePendingEndingKnot(this, out knotName);

    public IReadOnlyList<string> PendingNarrativeScenes => [.. _pendingNarrativeScenes];

    internal void SetPolicePressure(int value)
        => CrimeSessionService.SetPolicePressure(this, value);

    public CrimeRoutePreview PreviewCrime(CrimeAttempt attempt)
        => CrimeSessionService.PreviewCrime(this, attempt);

    public int GetEffectiveRandomEventWeight(RandomEvent randomEvent)
        => RandomEventService.GetEffectiveEventWeight(this, randomEvent);

    internal void RollDistrictConditionsForCurrentDay(Random random)
        => DistrictConditionRoller.RollForCurrentDay(this, random);

    internal void SetBaselineDistrictConditions()
        => DistrictConditionRoller.SetBaseline(this);

    internal void CheckGameOverConditions()
        => EndingCommitmentService.CheckGameOverConditions(this);

    internal void QueueNarrativeFollowUpScenes()
    {
        var reachabilityContext = NarrativeFollowUpContextBuilder.BuildReachabilityContext(this);

        TryQueueNarrativeTrigger(NarrativeReachabilityPlanner.GetWeatherTrigger(reachabilityContext, _storyFlags));
        TryQueueNarrativeTrigger(NarrativeReachabilityPlanner.GetSeasonalTrigger(reachabilityContext, _storyFlags));

        var communityDebtContext = NarrativeFollowUpContextBuilder.BuildCommunityDebtContext(this);

        foreach (var trigger in NarrativeCommunityDebtPlanner.GetTriggers(communityDebtContext, _storyFlags))
        {
            TryQueueNarrativeTrigger(trigger);
        }

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

        foreach (var trigger in NarrativeFollowUpPlanner.GetWorkFollowUpTriggers(
                     HonestShiftsCompleted,
                     CrimesCommitted,
                     Relationships,
                     _storyFlags))
        {
            TryQueueNarrativeTrigger(trigger);
        }

        TryQueueNarrativeTrigger(NarrativeFollowUpPlanner.GetCommunityAftermathTrigger(EventAttendance, _storyFlags));

        var crisisCallback = CityCrisisPlanner.GetDelayedCallback(Clock.Day, CityCrisis, _storyFlags);
        if (crisisCallback is not null && TryQueueNarrativeTrigger(crisisCallback))
        {
            CityCrisis.MarkCallbackQueued();
        }

        var centralArc = CentralCharacterArcPlanner.GetNextTrigger(Clock.Day, _storyFlags);
        if (centralArc is not null && TryQueueNarrativeTrigger(centralArc))
        {
            var character = CentralCharacterKnotMap.ResolveCharacter(centralArc.KnotName);
            if (character is not null)
            {
                CentralCharacterArcs.MarkBeat(character.Value);
            }
        }
    }

    internal CrimeModifierEvaluation EvaluateCrimeModifiers(CrimeAttempt attempt)
        => CrimeSessionService.EvaluateCrimeModifiers(this, attempt);

    internal void ApplyRandomEvent(RandomEvent randomEvent)
        => RandomEventService.ApplyEvent(this, randomEvent);

    internal void ApplySkillGain(SkillId skillId)
    {
        if (SkillService.ApplySkillGain(skillId, this, out var newLevel))
        {
            RaiseEvent($"{skillId} improves to {newLevel}.");
        }
    }

    internal void SyncLegacyHunger()
    {
        Player.Stats.SetHunger(Player.Nutrition.Satiety);
    }

    internal GameDayOfWeek GetCurrentDayOfWeek()
        => CalendarService.GetCurrentDayOfWeek(this);

    public DayScheduleModifiers GetCurrentSchedule()
        => CalendarService.GetCurrentSchedule(this);

    public Season GetCurrentSeason()
        => CalendarService.GetCurrentSeason(this);

    public SeasonModifiers GetCurrentSeasonModifiers()
        => CalendarService.GetCurrentSeasonModifiers(this);

    public ActiveHolidayState GetActiveHolidayState()
        => CalendarService.GetActiveHolidayState(this);

    public void SetRamadanFasting(bool isFasting)
        => CalendarService.SetRamadanFasting(this, isFasting);

    public IReadOnlyList<InvestmentDefinition> GetAvailableInvestments()
        => InvestmentPurchaseService.GetAvailable(this);

    public InvestmentEligibility CheckInvestmentEligibility(InvestmentDefinition definition)
        => InvestmentPurchaseService.CheckEligibility(this, definition);

    public MakeInvestmentResult MakeInvestment(InvestmentType type)
        => InvestmentPurchaseService.MakeInvestment(this, type);

    public InvestmentResolutionSummary ResolveWeeklyInvestments(Random? random = null)
        => InvestmentPurchaseService.ResolveWeekly(this, random);

    internal void RestoreInvestmentState(
        IEnumerable<InvestmentSnapshot> investments,
        int totalInvestmentEarnings)
        => InvestmentPurchaseService.Restore(this, investments, totalInvestmentEarnings);

    internal void ResolveWeeklyHouseholdAssets()
        => HouseholdAssetsService.ResolveWeekly(this);

    internal void TryRollStreetCatEncounter(Random random)
        => HouseholdAssetsService.TryRollStreetCatEncounter(this, random);

    internal void ResolveWeeklyEconomy(Random random)
        => WeeklyEconomyResolution.Resolve(this, random);

    internal void ProcessDailyDebt()
    {
        var result = DebtService.ProcessDailyLoanShark(
            PlayerDebts,
            Player.Stats,
            Clock.Day,
            Player.Skills.GetLevel(SkillId.Composure));
        if (!string.IsNullOrEmpty(result.Message))
        {
            RaiseEvent(result.Message);
        }

        if (result.TriggersDestitution)
        {
            EndingCommitmentService.TriggerDestitution(this);
        }
    }

    public (bool Success, int Amount, string Message) TryBorrowFromNpc(NpcId npc, int amount)
        => DebtAndLoanService.BorrowFromNpc(this, npc, amount);

    public (bool Success, int Amount, string Message) TryBorrowFromLandlord(int amount)
        => DebtAndLoanService.BorrowFromLandlord(this, amount);

    public (bool Success, int Amount, string Message) TryBorrowFromLoanShark(int amount)
        => DebtAndLoanService.BorrowFromLoanShark(this, amount);

    public (bool Success, string Message) TryLendToNpc(NpcId npc, int amount)
        => DebtAndLoanService.LendToNpc(this, npc, amount);

    public (bool Success, string Message) RefuseNpcLoan(NpcId npc)
        => DebtAndLoanService.RefuseNpcLoan(this, npc);

    public (bool Success, int Remaining, string Message) RepayDebt(DebtSource source, int amount)
        => DebtAndLoanService.RepayDebt(this, source, amount);

    internal void RestoreEconomyState(
        IEnumerable<(NpcId Npc, NpcWealthLevel WealthLevel, int Generosity,
            Dictionary<DebtorId, int> OwedTo, Dictionary<DebtorId, int> OwedBy,
            int LastHardshipDay, int LastWindfallDay, int GenerousUntilDay)> npcEconomies,
        IEnumerable<PlayerDebt> playerDebts)
        => DebtAndLoanService.RestoreEconomyState(this, npcEconomies, playerDebts);

    internal void ProcessDailyPhone(Random random)
        => PhoneService.ProcessDaily(this, random);

    public (bool Success, string Message) RefillPhoneCredit()
        => PhoneService.RefillCredit(this);

    public (bool Success, string Message) RespondToMessage(string messageId)
        => PhoneService.RespondToMessage(this, messageId);

    public (bool Success, string Message, int TrustLoss) IgnoreMessage(string messageId)
        => PhoneService.IgnoreMessage(this, messageId);

    public (bool Success, string Message) ReplacePhone()
        => PhoneService.ReplacePhone(this);

    internal void RestorePhoneState(bool hasPhone, int creditRemaining, int daysSinceCreditRefill,
        bool phoneLost, int? phoneLostDay, bool phoneRecovered, int handsetCondition = 65)
        => PhoneService.RestoreState(this, hasPhone, creditRemaining, daysSinceCreditRefill, phoneLost, phoneLostDay, phoneRecovered, handsetCondition);

    internal void RestorePhoneMessages(IEnumerable<PhoneMessage> messages)
        => PhoneService.RestoreMessages(this, messages);

    internal void ProcessDailyTips(Random random)
        => TipService.ProcessDaily(this, random);

    public (bool Success, string Message) AcknowledgeTip(string tipId)
        => TipService.Acknowledge(this, tipId);

    public (bool Success, string Message, int TrustLoss) IgnoreTipAction(string tipId)
        => TipService.Ignore(this, tipId);

    internal void RestoreTips(IEnumerable<Tip> tips, Dictionary<NpcId, int> ignoredCounts)
        => TipService.Restore(this, tips, ignoredCounts);
}
