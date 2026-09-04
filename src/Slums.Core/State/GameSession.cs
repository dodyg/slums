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
using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

namespace Slums.Core.State;

public sealed partial class GameSession : INarrativeOutcomeTarget
{
    private const int EndOfDayHour = 22;
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

    /// <summary>Hydrates this session through one validated snapshot restore boundary.</summary>
    /// <param name="restore">The persistence adapter that applies the captured state.</param>
    /// <returns>This hydrated session.</returns>
    public GameSession RestoreFromSnapshot(Action<GameSession> restore)
    {
        ArgumentNullException.ThrowIfNull(restore);
        restore(this);
        ValidateRestoredState();
        return this;
    }

    private void ValidateRestoredState()
    {
        if (Clock.Day < 1 || Clock.Hour is < 0 or > 23 || Clock.Minute is < 0 or > 59)
        {
            throw new InvalidOperationException("Restored session has an incoherent clock.");
        }

        if (Relationships.NpcRelationships.Count != Enum.GetValues<NpcId>().Length)
        {
            throw new InvalidOperationException("Restored session has an incomplete NPC relationship registry.");
        }

        if (JobProgress.Tracks.Count != Enum.GetValues<JobType>().Length)
        {
            throw new InvalidOperationException("Restored session has an incomplete job track registry.");
        }
    }

    public GameClock Clock { get; }
    public PlayerCharacter Player { get; }
    public WorldState World { get; }
    public RelationshipState Relationships { get; }
    public JobProgressState JobProgress { get; }
    public JobService Jobs { get; }
    public bool IsGameOver { get => _runState.IsGameOver; private set => _runState.IsGameOver = value; }
    public string? GameOverReason { get => _runState.GameOverReason; private set => _runState.GameOverReason = value; }
    public EndingId? EndingId { get => _runState.EndingId; private set => _runState.EndingId = value; }
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
    public string? PendingEndingKnot { get => _runState.PendingEndingKnot; private set => _runState.PendingEndingKnot = value; }

    public EndingId? PendingEndingId { get => _runState.PendingEndingId; private set => _runState.PendingEndingId = value; }

    public string? FinalSacrifice { get => _runState.FinalSacrifice; private set => _runState.FinalSacrifice = value; }
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
    {
        return EndingService.GetAvailableEndings(this);
    }

    public bool TryChooseEnding(EndingId endingId)
    {
        var before = CaptureStats();
        if (PendingEndingId is not null || IsGameOver || !EndingService.GetAvailableEndings(this).Contains(endingId))
        {
            RaiseEvent("That long-term path is not ready yet.");
            RecordMutation(MutationCategories.GuardRejected, "ChooseEnding", before, CaptureStats(), $"Ending {endingId} is not available");
            return false;
        }

        PendingEndingId = endingId;
        PendingEndingKnot = EndingKnotCatalog.Commitment;
        RecordMutation(MutationCategories.EndingTriggered, "ChooseEnding", before, CaptureStats(), $"Ending commitment opened: {endingId}");
        return true;
    }

    public void CommitEnding(EndingId endingId, string sacrifice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sacrifice);
        var before = CaptureStats();
        if (PendingEndingId != endingId || IsGameOver)
        {
            throw new InvalidOperationException($"Ending '{endingId}' is not the pending commitment.");
        }

        EndingId = endingId;
        FinalSacrifice = sacrifice;
        PendingEndingId = null;
        IsGameOver = true;
        GameOverReason = EndingService.GetMessage(endingId);
        PendingEndingKnot = EndingService.GetInkKnot(this, endingId);
        RecordMutation(MutationCategories.EndingTriggered, "CommitEnding", before, CaptureStats(), $"Ending committed: {endingId}; sacrifice: {sacrifice}");
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

    internal bool CanCompleteActivityToday(int durationMinutes)
    {
        return DailyActivityWindow.CanComplete(Clock, durationMinutes, EndOfDayHour);
    }

    /// <summary>Resolves the current day through the session-owned daily pipeline.</summary>
    public void EndDay(Random? random = null)
    {
        EndOfDayPipeline.Run(this, random);
    }

    /// <summary>Daily-pipeline access to the random event service.</summary>
    internal RandomEventService RandomEventService => _randomEventService;
    internal LocationPricingService LocationPricing => _locationPricingService;
    internal RentState Rent => _rentState;
    internal GameInvestmentState InvestmentState => _investmentState;

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

    public bool CollectCrisisEvidence(int amount = 1) => CityCrisis.CollectEvidence(amount);

    public bool CommitCrisisResources(int amount)
    {
        var committed = CityCrisis.CommitResources(amount);
        if (committed)
        {
            Technology.RecordMicrogridRepair(amount);
        }

        return committed;
    }

    public bool ChooseCrisisDecision(CityCrisisDecision decision) => CityCrisis.ChooseDecision(decision, Clock.Day);

    public bool MarkCrisisCallbackQueued()
    {
        if (!CityCrisis.HasDueCallback(Clock.Day))
        {
            return false;
        }

        CityCrisis.MarkCallbackQueued();
        return true;
    }

    public bool ResolveCityCrisis(CityCrisisResolution resolution) => CityCrisis.Resolve(resolution);

    public bool RecordCentralCharacterDecision(CentralCharacterId character, CentralArcDecision decision)
    {
        return CentralCharacterArcs.RecordDecision(character, decision);
    }

    public void AdjustPolicePressure(int delta)
    {
        DistrictHeat.AddHeat(World.CurrentDistrict, delta);
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
    {
        foreach (DistrictId district in Enum.GetValues<DistrictId>())
        {
            if (!TerritoryDynamicsCalculator.ShouldTriggerConflictEvent(Territory, district, random))
            {
                continue;
            }

            var control = Territory.GetControl(district);
            if (control.TensionLevel == TensionLevel.Dangerous)
            {
                if (district == World.CurrentDistrict)
                {
                    var crossfire = TerritoryEventRegistry.CrossfireEvent;
                    Player.Stats.ModifyStress(crossfire.StressModifier);
                    Player.Stats.ModifyHealth(crossfire.HealthModifier);
                    RaiseEvent(crossfire.Narration!);
                }
                else
                {
                    RaiseEvent($"Fighting breaks out in {district}. The streets are dangerous.");
                }
            }
            else
            {
                var argument = TerritoryEventRegistry.StreetArgument;
                if (district == World.CurrentDistrict)
                {
                    Player.Stats.ModifyStress(argument.StressModifier);
                    RaiseEvent(argument.Narration!);
                }
                else
                {
                    RaiseEvent($"Tensions flare in {district}. Word spreads through the neighborhood.");
                }
            }

            if (TerritoryDynamicsCalculator.ShouldTriggerPoliceCrackdown(Territory, district, DistrictHeat.GetHeat(district)))
            {
                var beforeFlip = Territory.GetControl(district);
                TerritoryDynamicsCalculator.ApplyPoliceCrackdown(Territory, district);
                DistrictHeat.AddHeat(district, 10);
                var crackdown = TerritoryEventRegistry.PoliceCrackdownEvent;

                if (district == World.CurrentDistrict)
                {
                    Player.Stats.ModifyStress(crackdown.StressModifier);
                    RaiseEvent(crackdown.Narration!);
                }
                else
                {
                    RaiseEvent($"Police crack down hard in {district}. The whole city feels it.");
                }

                var afterCrackdown = Territory.GetControl(district);
                var flip = TerritoryDynamicsCalculator.DetectTerritoryFlip(beforeFlip, afterCrackdown);
                if (flip.HasValue)
                {
                    var flipEvent = TerritoryEventRegistry.TerritoryFlipEvent(flip);
                    RaiseEvent(flipEvent.Narration!);
                }
            }
        }

        if (Player.BackgroundType == BackgroundType.SudaneseRefugee && World.CurrentDistrict == DistrictId.Imbaba)
        {
            var control = Territory.GetControl(DistrictId.Imbaba);
            if (control.TensionLevel >= TensionLevel.Elevated)
            {
#pragma warning disable CA5394
                if (random.Next(100) < 15)
#pragma warning restore CA5394
                {
                    var solidarity = TerritoryEventRegistry.RefugeeSolidarityEvent;
                    Player.Stats.ModifyStress(solidarity.StressModifier);
                    Territory.ModifyTension(DistrictId.Imbaba, solidarity.TensionModifier);
                    RaiseEvent(solidarity.Narration!);
                }
            }
        }
    }

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
    {
        ArgumentNullException.ThrowIfNull(job);

        var before = CaptureStats();
        if (WeatherActivityRules.BlocksJob(CurrentWeather, job.Type))
        {
            var reason = WeatherActivityRules.GetJobBlockReason(CurrentWeather);
            RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, CaptureStats(), reason);
            RaiseEvent(reason);
            return JobResult.Failed(reason);
        }

        var location = World.GetCurrentLocation();
        if (location is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "WorkJob", before, CaptureStats(), "No current location");
            return JobResult.Failed("You are nowhere.");
        }

        var result = Jobs.PerformJob(
            job,
            Player,
            location,
            Relationships,
            JobProgress,
            Clock.Day,
            random ?? _sharedRandom,
            NewsImpactCalculator.GetJobPayModifier(News, job.Type));

        if (result.Success)
        {
            ActivityLedgerSystem.RecordWorkShift(_workState, Clock, job, result);
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
            if (job.Type == JobType.RoboticsScavenging)
            {
                if (Player.Robotics.CanBuyParts(1))
                {
                    Player.Robotics.AddParts(1);
                    RaiseEvent("You salvage one usable board or actuator from the pile. Robot parts +1.");
                }

                var workingRobot = Player.Robotics.Robots.FirstOrDefault(static robot => robot.IsOperational);
                if (workingRobot is not null)
                {
                    workingRobot.Damage(10);
                    RaiseEvent($"The {RobotRegistry.GetByType(workingRobot.Type).Name} takes wear on the scavenging run. Condition: {workingRobot.Condition}%.");
                }

                if (RobotCapabilityRules.GetSalvageBonusParts(Player.Robotics) > 0 && Player.Robotics.CanBuyParts(1))
                {
                    Player.Robotics.AddParts(1);
                    RaiseEvent("The Salvage Crawler finds one extra usable actuator. Robot parts +1.");
                }
            }
            TerritoryDynamicsCalculator.ApplyHonestWorkImpact(Territory, World.CurrentDistrict);

            RaiseEvent(result.Message);
            RecordMutation(MutationCategories.Work, "WorkJob", before, CaptureStats(), result.Message);
            AdvanceTime(job.DurationMinutes);
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
            .Where(job => !WeatherActivityRules.BlocksJob(CurrentWeather, job.Type))
            .Select(job => ApplyDayScheduleToJob(ApplyDistrictConditionToJob(job), schedule))
            .ToArray();
    }

    public IReadOnlyList<CrimeAttempt> GetAvailableCrimes()
    {
        if (GetCrimeBlockReason() is not null)
        {
            return [];
        }

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

    public string? GetCrimeBlockReason()
    {
        if (CurrentWeather.BlocksCrime)
        {
            return WeatherActivityRules.GetCrimeBlockReason(CurrentWeather);
        }

        return TerritoryDynamicsCalculator.IsCrimeBlocked(Territory, World.CurrentDistrict)
            ? "The streets are too dangerous for any criminal activity right now."
            : null;
    }

    public CrimeResult CommitCrime(CrimeAttempt attempt, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var before = CaptureStats();
        var blockReason = GetCrimeBlockReason();
        if (blockReason is not null)
        {
            var blockedResult = new CrimeResult { Message = blockReason };
            RecordMutation(MutationCategories.GuardRejected, "CommitCrime", before, CaptureStats(), blockReason);
            RaiseEvent(blockReason);
            return blockedResult;
        }

        var modifierEvaluation = EvaluateCrimeModifiers(attempt);
        var modifiedAttempt = modifierEvaluation.Attempt;
        ApplyCrimeModifierSideEffects(modifierEvaluation.Signals);
        var districtHeat = DistrictHeat.GetHeat(World.CurrentDistrict);
        var result = _crimeService.AttemptCrime(modifiedAttempt, Player, districtHeat, random ?? _sharedRandom);
        Player.Stats.ModifyEnergy(-result.EnergyCost);
        Player.Stats.ModifyStress(result.StressCost);
        ActivityLedgerSystem.RecordCrimeOutcome(_crimeState, Clock, result);

        if (result.Success)
        {
            Player.Stats.ModifyMoney(result.MoneyEarned);
            ApplySkillGain(SkillId.StreetSmarts);
            ModifyFactionReputation(GetFactionForCurrentCrimeRoute(), 4);
            if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
            {
                ModifyFactionReputation(FactionId.ExPrisonerNetwork, 5);
            }
            TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetFirstSuccessTrigger(_storyFlags));
        }

        TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetRouteSceneTrigger(attempt.Type, result));

        DistrictHeat.AddHeat(World.CurrentDistrict, result.PolicePressureDelta);
        var updatedDistrictHeat = DistrictHeat.GetHeat(World.CurrentDistrict);
        TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetPoliceEncounterTrigger(
            World.CurrentDistrict,
            districtHeat,
            updatedDistrictHeat,
            _storyFlags));
        TerritoryDynamicsCalculator.ApplyCrimeImpact(Territory, World.CurrentDistrict, null);
        RaiseEvent(result.Message);
        ApplyCrimeContactAftermath(result);

        TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetGangRetaliationTrigger(
            result.Detected,
            World.CurrentDistrict,
            Territory.GetControl(World.CurrentDistrict).ControllingFaction,
            Relationships,
            _storyFlags));

        if (TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetCrimeWarningTrigger(PolicePressure, _storyFlags)))
        {
            RaiseEvent("People are whispering that the police are getting close.");
        }

        AdvanceTime(attempt.DurationMinutes);
        CheckGameOverConditions();
        RecordMutation(MutationCategories.Crime, "CommitCrime", before, CaptureStats(), $"{attempt.Type}: success={result.Success}, detected={result.Detected}");
        return result;
    }

    private FactionId GetFactionForCurrentCrimeRoute()
    {
        var controllingFaction = Territory.GetControl(World.CurrentDistrict).ControllingFaction;
        if (controllingFaction.HasValue)
        {
            return controllingFaction.Value;
        }

        return World.CurrentDistrict switch
        {
            DistrictId.Dokki => FactionId.DokkiThugs,
            DistrictId.ArdAlLiwa => FactionId.ExPrisonerNetwork,
            _ => FactionId.ImbabaCrew
        };
    }

    public bool BuyFood()
    {
        return FoodShopService.BuyFood(this);
    }

    public bool BuyMedicine()
    {
        return FoodShopService.BuyMedicine(this);
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
    {
        var preview = ApplyDistrictConditionToJobPreview(Jobs.PreviewJob(jobType, Player, Relationships, JobProgress));
        var modifiers = preview.ActiveModifiers.ToList();
        var payModifier = NewsImpactCalculator.GetJobPayModifier(News, jobType);
        if (payModifier != 0)
        {
            modifiers.Add($"City news changes this shift's pay by {payModifier} LE.");
        }
        var infrastructure = Infrastructure.Get(World.CurrentDistrict, InfrastructureServiceType.Electricity);
        if (infrastructure.IsActive)
        {
            modifiers.Add($"Electricity is {infrastructure.Severity} here; workshop and office work may be interrupted.");
        }
        return preview with { ActiveModifiers = modifiers };
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
        => TravelService.GetTravelCost(this, locationId);

    public int GetTravelTimeMinutes(LocationId locationId)
        => TravelService.GetTravelTimeMinutes(this, locationId);

    public int GetWalkTimeMinutes(LocationId locationId)
        => TravelService.GetWalkTimeMinutes(this, locationId);

    public string? GetTravelConditionSummary(LocationId locationId)
        => TravelService.GetTravelConditionSummary(this, locationId);

    public int CurrentDay => Clock.Day;

    public int CurrentWeek => ((Clock.Day - 1) / 7) + 1;

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

    internal void SetPolicePressure(int value)
    {
        DistrictHeat.SetHeatAll(value);
    }

    public CrimeRoutePreview PreviewCrime(CrimeAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        var modifierEvaluation = EvaluateCrimeModifiers(attempt);
        var districtHeat = DistrictHeat.GetHeat(World.CurrentDistrict);
        var resolution = _crimeService.PreviewCrime(modifierEvaluation.Attempt, Player, districtHeat);
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

        var currentHeat = DistrictHeat.GetHeat(World.CurrentDistrict);
        var updatedHeat = Math.Max(0, currentHeat - amount);
        if (updatedHeat == currentHeat)
        {
            return;
        }

        DistrictHeat.SetHeat(World.CurrentDistrict, updatedHeat);
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

    internal void SetRunId(Guid runId)
    {
        RunId = runId;
    }

    internal void RestoreRunState(
        Guid runId,
        int daysSurvived,
        bool isGameOver,
        string? gameOverReason,
        EndingId? endingId,
        string? pendingEndingKnot,
        bool emergencySupportClaimed = false,
        EndingId? pendingEndingId = null,
        string? finalSacrifice = null)
    {
        SetRunId(runId);
        SetDaysSurvived(daysSurvived);
        IsGameOver = isGameOver;
        GameOverReason = string.IsNullOrWhiteSpace(gameOverReason) ? null : gameOverReason;
        EndingId = endingId;
        PendingEndingKnot = string.IsNullOrWhiteSpace(pendingEndingKnot) ? null : pendingEndingKnot;
        PendingEndingId = pendingEndingId;
        FinalSacrifice = string.IsNullOrWhiteSpace(finalSacrifice) ? null : finalSacrifice;
        _runState.EmergencySupportClaimed = emergencySupportClaimed;
    }

    internal void SetDaysSurvived(int daysSurvived)
    {
        DaysSurvived = Math.Max(0, daysSurvived);
    }

    internal void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted)
    {
        SetCrimeCounters(totalCrimeEarnings, crimesCommitted, LastCrimeDay);
    }

    internal void SetCrimeCounters(int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay)
    {
        TotalCrimeEarnings = Math.Max(0, totalCrimeEarnings);
        CrimesCommitted = Math.Max(0, crimesCommitted);
        LastCrimeDay = Math.Max(0, lastCrimeDay);
    }

    internal void RestoreCrimeState(int policePressure, int totalCrimeEarnings, int crimesCommitted, int lastCrimeDay, bool hasCrimeCommittedToday)
    {
        DistrictHeat.SetHeatAll(policePressure);
        SetCrimeCounters(totalCrimeEarnings, crimesCommitted, lastCrimeDay);
        CrimeCommittedToday = hasCrimeCommittedToday;
    }

    internal void RestoreWorkState(int totalHonestWorkEarnings, int honestShiftsCompleted, int lastHonestWorkDay, int lastPublicFacingWorkDay)
    {
        TotalHonestWorkEarnings = Math.Max(0, totalHonestWorkEarnings);
        HonestShiftsCompleted = Math.Max(0, honestShiftsCompleted);
        LastHonestWorkDay = Math.Max(0, lastHonestWorkDay);
        LastPublicFacingWorkDay = Math.Max(0, lastPublicFacingWorkDay);
    }

    internal void RestoreRentState(int unpaidRentDays, int accumulatedRentDebt, bool firstWarningGiven, bool finalWarningGiven, int graceDaysRemaining = 0)
    {
        _rentState.Restore(unpaidRentDays, accumulatedRentDebt, firstWarningGiven, finalWarningGiven);
        _rentState.RestoreGraceDays(graceDaysRemaining);
    }

    public void RecordEventHistory(string eventId, int count)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        _randomEventHistory[eventId] = Math.Max(0, count);
    }

    internal void RestoreNarrativeState(
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

    internal void RestoreHouseholdAssetsState(
        IEnumerable<OwnedPet> pets,
        IEnumerable<OwnedPlant> plants,
        bool hasStreetCatEncounter,
        int lastStreetCatEncounterDay,
        int totalHerbEarnings,
        IEnumerable<OwnedRobot>? robots = null,
        int robotParts = 0)
        => HouseholdAssetsService.Restore(this, pets, plants, hasStreetCatEncounter, lastStreetCatEncounterDay, totalHerbEarnings, robots, robotParts);

    internal void RestoreRamadanState(bool isActive, bool playerIsFasting, int daysFasting, int daysRemaining)
    {
        RamadanState = new RamadanState
        {
            IsActive = isActive,
            PlayerIsFasting = playerIsFasting,
            DaysFasting = daysFasting,
            DaysRemaining = daysRemaining
        };
    }

    internal void RestoreCommunityEventAttendance(
        int consecutiveSkips,
        int totalAttended,
        int lastAttendanceDay,
        IEnumerable<CommunityEventId> attendedThisWeek,
        int lastWeekResetDay,
        bool hasTeaCircleInvitation)
    {
        ArgumentNullException.ThrowIfNull(attendedThisWeek);

        EventAttendance.ConsecutiveSkips = consecutiveSkips;
        EventAttendance.TotalAttended = totalAttended;
        EventAttendance.LastAttendanceDay = lastAttendanceDay;
        EventAttendance.LastWeekResetDay = lastWeekResetDay;
        EventAttendance.HasTeaCircleInvitation = hasTeaCircleInvitation;

        foreach (var eventId in attendedThisWeek)
        {
            EventAttendance.AttendedThisWeek.Add(eventId);
        }
    }

    internal void RestoreWeather(WeatherType weatherType)
    {
        CurrentWeather = WeatherModifiers.GetModifiers(weatherType);
    }

    public int GetEventCount(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return 0;
        }

        return _randomEventHistory.GetValueOrDefault(eventId);
    }

    internal void RestoreJobTrack(JobType jobType, int reliability, int shiftsCompleted, int lockoutUntilDay)
    {
        JobProgress.RestoreTrack(jobType, reliability, shiftsCompleted, lockoutUntilDay);
    }

    internal void RollDistrictConditionsForCurrentDay(Random random)
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

    internal void SetBaselineDistrictConditions()
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

    internal void CheckGameOverConditions()
    {
        var ending = EndingService.CheckFailureEndings(this);
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

    internal void QueueNarrativeFollowUpScenes()
    {
        var reachabilityContext = new NarrativeReachabilityContext(
            Clock.Day,
            CurrentWeather.Type,
            GetCurrentSeason(),
            GetActiveHolidayState().Id,
            GetActiveHolidayState().CurrentDay,
            Player.BackgroundType,
            GetCurrentDayOfWeek(),
            World.CurrentLocationId == LocationId.Home,
            HomeUpgrades.HasUpgrade(HomeUpgrade.Curtain),
            Player.Household.MotherHealth);

        TryQueueNarrativeTrigger(NarrativeReachabilityPlanner.GetWeatherTrigger(reachabilityContext, _storyFlags));
        TryQueueNarrativeTrigger(NarrativeReachabilityPlanner.GetSeasonalTrigger(reachabilityContext, _storyFlags));

        var loanSharkDebt = PlayerDebts.Debts.FirstOrDefault(static debt => debt.Source == DebtSource.LoanShark);
        var neighborDebt = PlayerDebts.Debts.Any(static debt => debt.Source is DebtSource.NeighborLoan or DebtSource.CommunityMutualAid);
        var mona = Relationships.GetNpcRelationship(NpcId.NeighborMona);
        var youssef = Relationships.GetNpcRelationship(NpcId.RunnerYoussef);
        var nadia = Relationships.GetNpcRelationship(NpcId.CafeOwnerNadia);
        var mariam = Relationships.GetNpcRelationship(NpcId.PharmacistMariam);
        var imbaba = Territory.GetControl(DistrictId.Imbaba);
        var communityDebtContext = new NarrativeCommunityDebtContext(
            Clock.Day,
            GetCurrentDayOfWeek(),
            Player.BackgroundType,
            EventAttendance.TotalAttended,
            EventAttendance.ConsecutiveSkips,
            EventAttendance.HasTeaCircleInvitation,
            PolicePressure,
            CrimesCommitted,
            HonestShiftsCompleted,
            mona.Trust,
            youssef.Trust,
            nadia.Trust,
            mariam.Trust,
            mona.WasHelped,
            youssef.WasHelped,
            loanSharkDebt is not null,
            loanSharkDebt?.DaysOverdue(Clock.Day) ?? 0,
            loanSharkDebt is null ? 0 : Math.Max(0, loanSharkDebt.DueDay - Clock.Day),
            neighborDebt,
            imbaba.Tension,
            imbaba.TensionLevel,
            imbaba.ControllingFaction == FactionId.DokkiThugs,
            imbaba.ControllingFaction == FactionId.ExPrisonerNetwork);

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
            var character = centralArc.KnotName switch
            {
                var knot when knot.StartsWith("central_mother_", StringComparison.Ordinal) => CentralCharacterId.Mother,
                var knot when knot.StartsWith("central_mona_", StringComparison.Ordinal) => CentralCharacterId.NeighborMona,
                var knot when knot.StartsWith("central_salma_", StringComparison.Ordinal) => CentralCharacterId.NurseSalma,
                var knot when knot.StartsWith("central_mahmoud_", StringComparison.Ordinal) => CentralCharacterId.HajjMahmoud,
                var knot when knot.StartsWith("central_ummkarim_", StringComparison.Ordinal) => CentralCharacterId.UmmKarim,
                _ => (CentralCharacterId?)null
            };
            if (character is not null)
            {
                CentralCharacterArcs.MarkBeat(character.Value);
            }
        }
    }

    internal CrimeModifierEvaluation EvaluateCrimeModifiers(CrimeAttempt attempt)
    {
        var modifiedAttempt = attempt;
        var activeModifiers = new List<string>();
        var signals = new HashSet<CrimeModifierSignal>();

        if (LastPublicFacingWorkDay == Clock.Day)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Max(5, modifiedAttempt.DetectionRisk - 8),
                PolicePressureIncrease = Math.Max(1, modifiedAttempt.PolicePressureIncrease - 4)
            };
            activeModifiers.Add("Same-day public-facing work gives you a thin alibi: lower risk and lower pressure.");
            signals.Add(CrimeModifierSignal.ThinAlibi);
        }

        if (Player.BackgroundType == BackgroundType.ReleasedPoliticalPrisoner)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Min(95, modifiedAttempt.DetectionRisk + 5),
                PolicePressureIncrease = modifiedAttempt.PolicePressureIncrease + 5
            };
            activeModifiers.Add("Released political prisoner background increases scrutiny and pressure.");
            signals.Add(CrimeModifierSignal.PrisonerScrutiny);
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

        if (CurrentWeather.CrimeDetectionModifier != 0)
        {
            modifiedAttempt = modifiedAttempt with
            {
                DetectionRisk = Math.Clamp(modifiedAttempt.DetectionRisk + CurrentWeather.CrimeDetectionModifier, 1, 95)
            };
            activeModifiers.Add($"{WeatherModifiers.GetDisplayName(CurrentWeather.Type)} weather: crime detection {CurrentWeather.CrimeDetectionModifier:+#;-#;0}.");
        }

        return new CrimeModifierEvaluation(modifiedAttempt, activeModifiers, signals);
    }

    private void ApplyCrimeModifierSideEffects(IReadOnlySet<CrimeModifierSignal> signals)
    {
        if (signals.Contains(CrimeModifierSignal.ThinAlibi))
        {
            RaiseEvent("The shift you worked today gives you a thin alibi and a cleaner reason to be seen moving.");
        }

        if (signals.Contains(CrimeModifierSignal.PrisonerScrutiny))
        {
            TryQueueNarrativeTrigger(CrimeNarrativePlanner.GetPrisonerHeatTrigger(Player.BackgroundType, _storyFlags));
        }
    }

    internal void ApplyRandomEvent(RandomEvent randomEvent)
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
            DistrictHeat.AddHeat(World.CurrentDistrict, effect.PolicePressureChange);
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
    {
        return Clock.DayOfWeek;
    }

    public DayScheduleModifiers GetCurrentSchedule()
    {
        return DayScheduleRegistry.GetModifiers(Clock.DayOfWeek);
    }

    public Season GetCurrentSeason()
    {
        return GameCalendar.GetSeason(Clock.Day);
    }

    public SeasonModifiers GetCurrentSeasonModifiers()
    {
        return SeasonModifiersRegistry.GetModifiers(GetCurrentSeason());
    }

    public ActiveHolidayState GetActiveHolidayState()
    {
        return HolidayRegistry.GetHolidayState(GameCalendar.GetDate(Clock.Day));
    }

    public void SetRamadanFasting(bool isFasting)
    {
        var holidayState = GetActiveHolidayState();
        if (!holidayState.IsRamadan)
        {
            return;
        }

        RamadanState = RamadanState with
        {
            IsActive = true,
            PlayerIsFasting = isFasting,
            DaysRemaining = holidayState.DaysRemaining
        };
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

    internal void ResolveWeeklyEconomy(Random random)
    {
        var hardshipModifier = NewsImpactCalculator.GetNpcHardshipModifier(News);
        NpcEconomyResolver.ResolveWeek(NpcEconomies, Relationships, Clock.Day, random, hardshipModifier);
        if (hardshipModifier > 0)
        {
            RaiseEvent($"City pressure is reaching household economies. Local hardship risk is up by {hardshipModifier}.");
        }

        var hajjEconomy = NpcEconomies.GetEconomy(NpcId.LandlordHajjMahmoud);
        if (hajjEconomy.WealthLevel == NpcWealthLevel.Struggling || hajjEconomy.WealthLevel == NpcWealthLevel.Poor)
        {
            _rentState.PayPartialDebt(-10);
            RaiseEvent("Hajj Mahmoud's money troubles make him meaner about rent. Rent pressure increases.");
        }

        var monaEconomy = NpcEconomies.GetEconomy(NpcId.NeighborMona);
        if (monaEconomy.WealthLevel == NpcWealthLevel.Struggling)
        {
            Player.Stats.ModifyStress(3);
            RaiseEvent("Mona is struggling. The worry weighs on you.");
        }

        var ummKarimEconomy = NpcEconomies.GetEconomy(NpcId.FixerUmmKarim);
        if (ummKarimEconomy.WealthLevel == NpcWealthLevel.Comfortable)
        {
            RaiseEvent("Umm Karim is doing well. She slips you an extra portion.");
        }

        var needingLoan = NpcEconomyResolver.GetNpcNeedingLoan(NpcEconomies, Relationships);
        if (needingLoan.HasValue)
        {
            var npcRel = Relationships.GetNpcRelationship(needingLoan.Value);
            if (npcRel.Trust >= 10)
            {
                RaiseEvent($"{needingLoan.Value} is in rough shape. They could use help.");
            }
        }

        PlayerDebts.ProcessInterest(Clock.Day);
        PlayerDebts.UpdateCollectionStates(Clock.Day);
    }

    internal void ProcessDailyDebt()
    {
        var result = DebtService.ProcessDailyLoanShark(PlayerDebts, Player.Stats, Clock.Day);
        if (!string.IsNullOrEmpty(result.Message))
        {
            RaiseEvent(result.Message);
        }

        if (result.TriggersDestitution)
        {
            var before = CaptureStats();
            EndingId = Endings.EndingId.Destitution;
            IsGameOver = true;
            GameOverReason = "The loan sharks come to collect. You cannot pay.";
            PendingEndingKnot = EndingKnotCatalog.Destitution;
            RecordMutation(MutationCategories.EndingTriggered, "ProcessDailyDebt", before, CaptureStats(), "Destitution ending triggered by loan shark violence");
        }
    }

    private int GetUmmKarimFoodDiscount()
    {
        var ummKarimEconomy = NpcEconomies.GetEconomy(NpcId.FixerUmmKarim);
        return ummKarimEconomy.WealthLevel == NpcWealthLevel.Comfortable ? -1 : 0;
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
    {
        if (!Phone.IsOperational())
        {
            Phone.DailyCreditDrain();
            PhoneMessages.MarkPendingAsMissed();
            return;
        }

        Phone.DailyCreditDrain();

        var newMessages = PhoneMessageGenerator.GenerateMessages(
            Clock.Day, Relationships, PolicePressure,
            Player.Household.MotherHealth, DistrictHeat,
            Player.BackgroundType, random);

        foreach (var message in newMessages)
        {
            PhoneMessages.AddMessage(message);
        }

        PhoneMessages.RemoveExpired(Clock.Day);

        if (newMessages.Count > 0)
        {
            var before = CaptureStats();
            RecordMutation(MutationCategories.Phone, "ProcessDailyPhone", before, CaptureStats(),
                $"Received {newMessages.Count} message(s)");
        }
    }

    public (bool Success, string Message) RefillPhoneCredit()
    {
        if (!Phone.IsOperational() && !Phone.HasPhone)
        {
            return (false, "You don't have a phone.");
        }

        if (Phone.PhoneLost)
        {
            return (false, "Your phone is lost.");
        }

        if (Player.Stats.Money < Phone.CreditWeekCost)
        {
            return (false, $"Not enough money (need {Phone.CreditWeekCost} LE, have {Player.Stats.Money} LE).");
        }

        var before = CaptureStats();
        Player.Stats.ModifyMoney(-Phone.CreditWeekCost);
        Phone.RefillCredit();
        Technology.RecordHandsetUse();
        PhoneMessages.DeliverMissedMessages();

        RecordMutation(MutationCategories.Phone, "RefillPhoneCredit", before, CaptureStats(),
            $"Refilled phone credit for {Phone.CreditWeekCost} LE");

        return (true, "Phone credit refilled for 7 days.");
    }

    public (bool Success, string Message) RespondToMessage(string messageId)
    {
        if (!Phone.IsOperational())
        {
            return (false, "Phone is not operational.");
        }

        var message = PhoneMessages.GetMessage(messageId);
        if (message is null)
        {
            return (false, "Message not found.");
        }

        if (message.Responded)
        {
            return (false, "Already responded to this message.");
        }

        if (message.Ignored)
        {
            return (false, "Message was ignored.");
        }

        if (message.IsExpired(Clock.Day))
        {
            return (false, "Message has expired.");
        }

        var missedCallCost = message.WasMissed ? 1 : 0;
        var totalMoneyCost = missedCallCost + message.ResponseMoneyCost;
        if (Player.Stats.Money < totalMoneyCost)
        {
            return message.WasMissed && message.ResponseMoneyCost == 0
                ? (false, "Not enough money to return this missed call (1 LE).")
                : (false, $"Not enough money (need {totalMoneyCost} LE).");
        }

        var responseTimeMinutes = message.ResponseTimeCost * 60;
        if (!CanCompleteActivityToday(responseTimeMinutes))
        {
            return (false, "Not enough time to respond today.");
        }

        var before = CaptureStats();

        if (totalMoneyCost > 0)
        {
            Player.Stats.ModifyMoney(-totalMoneyCost);
        }

        PhoneMessages.RespondToMessage(messageId);

        ApplyMessageResponseEffects(message);

        RecordMutation(MutationCategories.Phone, "RespondToMessage", before, CaptureStats(),
            $"Responded to message from {message.Sender}: {message.Content}");

        if (responseTimeMinutes > 0)
        {
            AdvanceTime(responseTimeMinutes);
        }

        return (true, $"Responded to {message.Sender}.");
    }

    private void ApplyMessageResponseEffects(PhoneMessage message)
    {
        switch (message.Type)
        {
            case PhoneMessageType.Opportunity:
            {
                if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
                {
                    Relationships.RecordFavor(npc, Clock.Day);
                }

                break;
            }
            case PhoneMessageType.Warning:
            {
                Player.Stats.ModifyStress(-3);
                break;
            }
            case PhoneMessageType.FamilyAlert:
            {
                RaiseEvent("You check on your mother after Mona's message.");
                break;
            }
            case PhoneMessageType.NetworkRequest:
            {
                if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
                {
                    Relationships.RecordFavor(npc, Clock.Day);
                }

                break;
            }
            case PhoneMessageType.Background:
            {
                if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
                {
                    Relationships.ModifyNpcTrust(npc, 1);
                }

                break;
            }
        }
    }

    public (bool Success, string Message, int TrustLoss) IgnoreMessage(string messageId)
    {
        if (!Phone.IsOperational())
        {
            return (false, "Phone is not operational.", 0);
        }

        var message = PhoneMessages.GetMessage(messageId);
        if (message is null)
        {
            return (false, "Message not found.", 0);
        }

        if (message.Responded || message.Ignored)
        {
            return (false, "Message already handled.", 0);
        }

        var before = CaptureStats();

        var ignoreCount = PhoneMessages.IgnoreMessage(messageId);
        var trustLoss = 0;

        if (Enum.TryParse<NpcId>(message.SenderNpcId, out var npc))
        {
            var trust = Relationships.GetNpcRelationship(npc).Trust;
            if (ContactErosionRule.ShouldErode(trust, ignoreCount))
            {
                trustLoss = 1;
                Relationships.ModifyNpcTrust(npc, -trustLoss);
            }
        }

        RecordMutation(MutationCategories.Phone, "IgnoreMessage", before, CaptureStats(),
            $"Ignored message from {message.Sender}");

        return (true, $"Ignored message from {message.Sender}.", trustLoss);
    }

    public (bool Success, string Message) ReplacePhone()
    {
        if (!Phone.PhoneLost)
        {
            return (false, "Your phone is not lost.");
        }

        const int replacementCost = PhoneState.ReplacementCost;
        if (Player.Stats.Money < replacementCost)
        {
            return (false, $"Not enough money (need {replacementCost} LE for replacement + credit).");
        }

        var before = CaptureStats();
        Player.Stats.ModifyMoney(-replacementCost);
        Phone.ReplacePhone();
        PhoneMessages.DeliverMissedMessages();

        RecordMutation(MutationCategories.Phone, "ReplacePhone", before, CaptureStats(),
            $"Replaced phone for {replacementCost} LE");

        return (true, "New phone purchased. Credit refilled for 7 days.");
    }

    internal void RestorePhoneState(bool hasPhone, int creditRemaining, int daysSinceCreditRefill,
        bool phoneLost, int? phoneLostDay, bool phoneRecovered)
    {
        Phone.Restore(hasPhone, creditRemaining, daysSinceCreditRefill, phoneLost, phoneLostDay, phoneRecovered);
    }

    internal void RestorePhoneMessages(IEnumerable<PhoneMessage> messages)
    {
        PhoneMessages.RestoreMessages(messages);
    }

    internal void ProcessDailyTips(Random random)
    {
        var newTips = TipGenerator.GenerateTips(
            Clock.Day, Relationships, DistrictHeat, NpcEconomies,
            Player.BackgroundType, CrimesCommitted,
            Relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud).Trust,
            random);

        foreach (var tip in newTips)
        {
            Tips.AddTip(tip);

            var deliveryMethod = TipDeliveryConfig.GetDeliveryMethod(tip, World.CurrentDistrict);
            if (deliveryMethod == TipDeliveryMethod.Phone || deliveryMethod == TipDeliveryMethod.Emergency)
            {
                if (Phone.IsOperational())
                {
                    PhoneMessages.AddMessage(new PhoneMessage
                    {
                        Type = PhoneMessageType.Tip,
                        Sender = NpcRegistry.GetName(tip.Source),
                        SenderNpcId = tip.Source.ToString(),
                        Content = tip.Content,
                        DayReceived = tip.DayGenerated,
                        ExpiresAfterDay = tip.ExpiresAfterDay,
                        RequiresResponse = false,
                        ResponseTimeCost = 0,
                        ResponseMoneyCost = 0
                    });
                    Tips.MarkAsDelivered(tip.Id);
                }
            }
        }

        ApplyTipIgnoreErosion();

        var removed = Tips.RemoveExpired(Clock.Day);
        if (newTips.Count > 0 || removed > 0)
        {
            var before = CaptureStats();
            RecordMutation(MutationCategories.Information, "ProcessDailyTips", before, CaptureStats(),
                $"Generated {newTips.Count} tip(s), expired {removed}");
        }
    }

    private void ApplyTipIgnoreErosion()
    {
        foreach (NpcId npc in Enum.GetValues<NpcId>())
        {
            var ignoredCount = Tips.GetIgnoredCount(npc);
            var trust = Relationships.GetNpcRelationship(npc).Trust;
            if (!ContactErosionRule.ShouldErode(trust, ignoredCount))
            {
                continue;
            }

            Relationships.ModifyNpcTrust(npc, -1);
            RaiseEvent($"{NpcRegistry.GetName(npc)} seems annoyed that you keep ignoring their advice. Trust -1.");
        }
    }

    public (bool Success, string Message) AcknowledgeTip(string tipId)
    {
        var tip = Tips.GetTip(tipId);
        if (tip is null)
        {
            return (false, "Tip not found.");
        }

        if (tip.Acknowledged)
        {
            return (false, "Already acknowledged.");
        }

        if (tip.Ignored)
        {
            return (false, "Tip was ignored.");
        }

        var before = CaptureStats();
        Tips.AcknowledgeTip(tipId);

        RecordMutation(MutationCategories.Information, "AcknowledgeTip", before, CaptureStats(),
            $"Acknowledged tip from {NpcRegistry.GetName(tip.Source)}: {tip.Content}");

        return (true, $"Acknowledged tip from {NpcRegistry.GetName(tip.Source)}.");
    }

    public (bool Success, string Message, int TrustLoss) IgnoreTipAction(string tipId)
    {
        var tip = Tips.GetTip(tipId);
        if (tip is null)
        {
            return (false, "Tip not found.", 0);
        }

        if (tip.Acknowledged || tip.Ignored)
        {
            return (false, "Tip already handled.", 0);
        }

        var before = CaptureStats();
        var ignoreCount = Tips.IgnoreTip(tipId);

        var trustLoss = 0;
        var trust = Relationships.GetNpcRelationship(tip.Source).Trust;
        if (ContactErosionRule.ShouldErode(trust, ignoreCount))
        {
            trustLoss = 1;
            Relationships.ModifyNpcTrust(tip.Source, -trustLoss);
        }

        RecordMutation(MutationCategories.Information, "IgnoreTip", before, CaptureStats(),
            $"Ignored tip from {NpcRegistry.GetName(tip.Source)}");

        return (true, $"Ignored tip from {NpcRegistry.GetName(tip.Source)}.", trustLoss);
    }

    internal void RestoreTips(IEnumerable<Tip> tips, Dictionary<NpcId, int> ignoredCounts)
    {
        Tips.RestoreTips(tips, ignoredCounts);
    }
}
