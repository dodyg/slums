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

    private bool CanCompleteActivityToday(int durationMinutes)
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

        if (WeatherActivityRules.BlocksTravelTo(CurrentWeather, location.District))
        {
            var reason = WeatherActivityRules.GetTravelBlockReason(CurrentWeather, location.District);
            RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, CaptureStats(), reason);
            RaiseEvent(reason);
            return false;
        }

        var travelCost = GetTravelCost(location);
        var travelEnergyCost = GetTravelEnergyCost(location);
        var travelTimeMinutes = GetTravelTimeMinutes(location);

        if (Player.Stats.Money < travelCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "TryTravelTo", before, CaptureStats(), $"Not enough money (need {travelCost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent("Not enough money for transport.");
            return false;
        }

        Player.Stats.ModifyMoney(-travelCost);
        Player.Stats.ModifyEnergy(-travelEnergyCost);
        ApplyCargoMuleWear();
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

        World.TravelTo(locationId);

        RaiseEvent($"Traveled to {location.Name}.");
        RecordMutation(MutationCategories.Travel, "TryTravelTo", before, CaptureStats(), $"Traveled to {location.Name} (cost {travelCost} LE)");
        AdvanceTime(travelTimeMinutes);
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

        if (WeatherActivityRules.BlocksTravelTo(CurrentWeather, location.District))
        {
            var reason = WeatherActivityRules.GetTravelBlockReason(CurrentWeather, location.District);
            RecordMutation(MutationCategories.GuardRejected, "TryWalkTo", before, CaptureStats(), reason);
            RaiseEvent(reason);
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

        World.TravelTo(locationId);

        RaiseEvent($"Walked to {location.Name}. The streets took their toll.");
        RecordMutation(MutationCategories.Travel, "TryWalkTo", before, CaptureStats(), $"Walked to {location.Name}");
        AdvanceTime(walkTimeMinutes);
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
        return EntertainmentService.GetAvailableActivities(this);
    }

    public bool TryPerformEntertainment(EntertainmentActivity activity)
    {
        return EntertainmentService.Perform(this, activity);
    }

    public IReadOnlyList<CommunityEventDefinition> GetAvailableCommunityEvents()
    {
        var events = new List<CommunityEventDefinition>();
        var dayOfWeek = Clock.DayOfWeek;
        var isRamadan = RamadanState.IsActive;

        foreach (var evt in CommunityEventRegistry.AllEvents)
        {
            if (evt.RequiresFriday && dayOfWeek != GameDayOfWeek.Friday)
            {
                continue;
            }

            if (evt.RequiresRamadan && !isRamadan)
            {
                continue;
            }

            if (evt.RequiresNpcInvitation && !EventAttendance.HasTeaCircleInvitation)
            {
                continue;
            }

            if (evt.HasPickpocketRisk && World.CurrentDistrict != DistrictId.Imbaba)
            {
                continue;
            }

            events.Add(evt);
        }

        return events;
    }

    public bool AttendCommunityEvent(CommunityEventId eventId, Random? random = null)
    {
        var definition = CommunityEventRegistry.GetById(eventId);
        if (definition is null)
        {
            return false;
        }

        var before = CaptureStats();
        random ??= _sharedRandom;

        var available = GetAvailableCommunityEvents();
        if (available.All(e => e.Id != eventId))
        {
            RaiseEvent($"{definition.Name} is not available right now.");
            RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, CaptureStats(), "Event not available");
            return false;
        }

        if (EventAttendance.AttendedThisWeek.Contains(eventId))
        {
            RaiseEvent($"You already attended {definition.Name} this week.");
            RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, CaptureStats(), "Already attended this week");
            return false;
        }

        if (Player.Stats.Money < definition.MoneyCost)
        {
            RaiseEvent($"You cannot afford the {definition.MoneyCost} LE contribution.");
            RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, CaptureStats(), $"Cannot afford {definition.MoneyCost} LE");
            return false;
        }

        if (!CanCompleteActivityToday(definition.TimeCostMinutes))
        {
            RaiseEvent("Not enough time in the day for that.");
            RecordMutation(MutationCategories.GuardRejected, "AttendCommunityEvent", before, CaptureStats(), "Not enough time");
            return false;
        }

        if (definition.MoneyCost > 0)
        {
            Player.Stats.ModifyMoney(-definition.MoneyCost);
        }

        Player.Stats.ModifyStress(definition.StressChange);

        var trustGained = ApplyCommunityEventTrust(definition, random);
        var backgroundBonus = ApplyBackgroundEventBonus(definition);

        if (definition.ProvidesFoodAccess)
        {
            Player.Nutrition.Eat(MealQuality.Basic);
        }

        if (definition.HasPickpocketRisk)
        {
#pragma warning disable CA5394
            var roll = random.Next(100);
            if (roll < 10)
            {
                var stolen = random.Next(5, 16);
#pragma warning restore CA5394
                Player.Stats.ModifyMoney(-stolen);
                RaiseEvent($"A pickpocket slips away with {stolen} LE from your pocket!");
            }
        }

        EventAttendance.RecordAttendance(eventId, Clock.Day);

        var trustMessage = trustGained > 0 ? $" Trust +{trustGained} with neighbors." : "";
        var backgroundMessage = backgroundBonus > 0 ? $" Background bonus: +{backgroundBonus} trust." : "";
        RaiseEvent($"You attend {definition.Name}. Stress {definition.StressChange}.{trustMessage}{backgroundMessage}");
        RecordMutation(MutationCategories.Community, "AttendCommunityEvent", before, CaptureStats(), $"{definition.Name} (stress {definition.StressChange}, trust gained: {trustGained})");
        AdvanceTime(definition.TimeCostMinutes);
        return true;
    }

    /// <summary>
    /// Accepts one early-run emergency support package tied to the selected background.
    /// </summary>
    public bool RequestEmergencySupport()
    {
        var before = CaptureStats();
        if (!CanRequestEmergencySupport)
        {
            RaiseEvent("No emergency community support is available for this run.");
            RecordMutation(MutationCategories.GuardRejected, "RequestEmergencySupport", before, CaptureStats(), "Support already claimed, expired, or background not selected");
            return false;
        }

        _runState.EmergencySupportClaimed = true;
        switch (Player.BackgroundType)
        {
            case BackgroundType.MedicalSchoolDropout:
                Player.Household.AddMedicine(2);
                Relationships.ModifyNpcTrust(NpcId.NurseSalma, 2);
                RaiseEvent("Salma puts two clinic doses aside for your mother. You spend an hour collecting them and promising to return the favor.");
                break;
            case BackgroundType.ReleasedPoliticalPrisoner:
                Player.Stats.ModifyMoney(30);
                Player.Household.AddStaples(1);
                Relationships.ModifyNpcTrust(NpcId.NeighborMona, 2);
                RaiseEvent("Mona gathers a small mutual-aid envelope and one food parcel. It is help, not a solution, and it costs an hour to arrange safely.");
                break;
            case BackgroundType.SudaneseRefugee:
                Player.Household.AddStaples(3);
                Player.Stats.ModifyStress(-4);
                Relationships.ModifyNpcTrust(NpcId.NeighborMona, 2);
                RaiseEvent("The Sudanese women's kitchen sends bread, beans, and tea upstairs. You spend an hour carrying containers back through the lane.");
                break;
            default:
                throw new InvalidOperationException($"Unsupported background {Player.BackgroundType}.");
        }

        AdvanceTime(EmergencySupportDurationMinutes);
        RecordMutation(MutationCategories.Community, "RequestEmergencySupport", before, CaptureStats(), $"Emergency support claimed for {Player.BackgroundType}");
        return true;
    }

    private int ApplyCommunityEventTrust(CommunityEventDefinition definition, Random random)
    {
        var communityNpcs = new[] { NpcId.LandlordHajjMahmoud, NpcId.FixerUmmKarim, NpcId.NeighborMona, NpcId.NurseSalma, NpcId.CafeOwnerNadia };
        var count = Math.Min(definition.TrustGainCount, communityNpcs.Length);
#pragma warning disable CA5394
        var selected = communityNpcs.OrderBy(_ => random.Next()).Take(count).ToArray();
#pragma warning restore CA5394

        var totalTrust = 0;
        foreach (var npcId in selected)
        {
            var trust = definition.TrustGainAmount;
            Relationships.ModifyNpcTrust(npcId, trust);
            totalTrust += trust;
        }

        return totalTrust;
    }

    private int ApplyBackgroundEventBonus(CommunityEventDefinition definition)
    {
        var bonus = 0;
        var background = Player.BackgroundType;

        if (background == BackgroundType.SudaneseRefugee && definition.Id == CommunityEventId.FridayRooftopGathering)
        {
            bonus = 2;
            Relationships.ModifyNpcTrust(NpcId.NeighborMona, bonus);
        }
        else if (background == BackgroundType.ReleasedPoliticalPrisoner)
        {
            if (EventAttendance.TotalAttended <= 3)
            {
                return 0;
            }

            bonus = 1;
        }
        else if (background == BackgroundType.MedicalSchoolDropout)
        {
            bonus = 1;
            Relationships.ModifyNpcTrust(NpcId.NurseSalma, bonus);
        }

        return bonus;
    }

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

        const int clinicVisitMinutes = 90;

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
        ApplySkillGain(SkillId.Medical);

        RaiseEvent($"You take your mother into {clinicStatus.LocationName}. The visit costs {clinicStatus.VisitCost} LE. Her health improves by {healthChange}.");
        if (NarrativeSignalRules.HasPendingClinicFirstVisit(_storyFlags))
        {
            TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.MotherClinicFirstVisit, NarrativeKnots.MotherClinicFirstVisit));
        }

        RecordMutation(MutationCategories.Clinic, "TakeMotherToClinic", before, CaptureStats(), $"Clinic visit at {clinicStatus.LocationName} (cost {clinicStatus.VisitCost} LE, health +{healthChange})");
        AdvanceTime(clinicVisitMinutes);
        return new MotherClinicVisitResult(true, clinicStatus.VisitCost, healthChange);
    }

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

        var travelBlocked = WeatherActivityRules.BlocksTravelTo(CurrentWeather, location.District);
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
            IsValidOption: !travelBlocked);
    }

    public TravelAndClinicVisitResult TravelAndTakeMotherToClinic(LocationId clinicLocationId)
    {
        var before = CaptureStats();
        var clinicLocation = WorldState.AllLocations.FirstOrDefault(candidate => candidate.Id == clinicLocationId);
        if (clinicLocation is not null && WeatherActivityRules.BlocksTravelTo(CurrentWeather, clinicLocation.District))
        {
            var reason = WeatherActivityRules.GetTravelBlockReason(CurrentWeather, clinicLocation.District);
            RecordMutation(MutationCategories.GuardRejected, "TravelAndTakeMotherToClinic", before, CaptureStats(), reason);
            RaiseEvent(reason);
            return new TravelAndClinicVisitResult(false, 0, 0, 0, 0);
        }

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
        ApplyCargoMuleWear();
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

        var repairDrone = Player.Robotics.Robots.FirstOrDefault(robot => robot.Type == RobotType.RepairDrone && robot.IsOperational);
        if (repairDrone is not null)
        {
            repairDrone.Damage(RobotCapabilityRules.ClinicWear);
            RaiseEvent($"The Repair Drone's triage reader takes {RobotCapabilityRules.ClinicWear} condition wear. Condition: {repairDrone.Condition}%.");
        }

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

        if (WeatherActivityRules.BlocksTravelTo(CurrentWeather, location.District))
        {
            return WeatherActivityRules.GetTravelBlockReason(CurrentWeather, location.District);
        }

        var summaries = new List<string>();
        if (CurrentWeather.TravelCostModifier != 0)
        {
            summaries.Add($"{WeatherModifiers.GetDisplayName(CurrentWeather.Type)} weather adds {CurrentWeather.TravelCostModifier} LE to transport.");
        }

        var infrastructureTravel = InfrastructureImpactCalculator.GetTravelCostModifier(Infrastructure, location.District);
        var newsTravel = NewsImpactCalculator.GetTravelCostModifier(News, location.District);
        if (infrastructureTravel != 0)
        {
            summaries.Add($"Transport service pressure adds {infrastructureTravel} LE and time to this trip.");
        }
        if (newsTravel != 0)
        {
            summaries.Add($"City news adds {newsTravel} LE to fares in this area.");
        }

        var districtCondition = GetActiveDistrictConditionDefinition(location.District);
        if (districtCondition is not null)
        {
            var effect = districtCondition.Effect;
            if (effect.TravelCostModifier != 0 || effect.TravelTimeMinutesModifier != 0 || effect.TravelEnergyModifier != 0)
            {
                summaries.Add($"{districtCondition.Title}: {districtCondition.GameplaySummary}");
            }
        }

        return summaries.Count == 0 ? null : string.Join(" ", summaries);
    }

    private int GetTravelCost(Location destination)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(destination.District);
        var modifiedCost = _locationPricingService.GetTravelCost(destination, Relationships)
            + (districtCondition?.Effect.TravelCostModifier ?? 0)
            + CurrentWeather.TravelCostModifier
            + InfrastructureImpactCalculator.GetTravelCostModifier(Infrastructure, destination.District)
            + NewsImpactCalculator.GetTravelCostModifier(News, destination.District);
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

        var modifiedCost = _locationPricingService.GetClinicVisitCost(location, Relationships, Player.Skills)
            + (districtCondition?.Effect.ClinicVisitCostModifier ?? 0)
            - scheduleDiscount
            - RobotCapabilityRules.GetClinicCostReduction(Player.Robotics);
        return Math.Max(1, modifiedCost);
    }

    private int GetTravelEnergyCost(Location destination)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(destination.District);
        var modifiedCost = _locationPricingService.GetTravelEnergyCost(destination, Relationships)
            + (districtCondition?.Effect.TravelEnergyModifier ?? 0)
            - RobotCapabilityRules.GetTransitEnergyReduction(Player.Robotics);
        return Math.Max(1, modifiedCost);
    }

    private void ApplyCargoMuleWear()
    {
        var cargoMule = Player.Robotics.Robots.FirstOrDefault(robot => robot.Type == RobotType.CargoMule && robot.IsOperational);
        if (cargoMule is null)
        {
            return;
        }

        cargoMule.Damage(RobotCapabilityRules.TransitWear);
        RaiseEvent($"The Cargo Mule takes {RobotCapabilityRules.TransitWear} condition wear on the route. Condition: {cargoMule.Condition}%.");
    }

    private int GetTravelTimeMinutes(Location destination)
    {
        var districtCondition = GetActiveDistrictConditionDefinition(destination.District);
        var modifiedMinutes = destination.TravelTimeMinutes
            + (districtCondition?.Effect.TravelTimeMinutesModifier ?? 0)
            + InfrastructureImpactCalculator.GetTravelTimeModifier(Infrastructure, destination.District);
        return Math.Max(1, modifiedMinutes);
    }

    public int CurrentDay => Clock.Day;

    public int CurrentWeek => ((Clock.Day - 1) / 7) + 1;

    public bool CanUseHouseholdAssets()
    {
        return World.CurrentLocationId == LocationId.FishMarket
            || World.CurrentLocationId == LocationId.PlantShop
            || World.CurrentLocationId == LocationId.Workshop
            || (World.CurrentLocationId == LocationId.Home
                && (Player.HouseholdAssets.HasAnyAssets || Player.HouseholdAssets.HasStreetCatEncounter || Player.Robotics.HasAnyRobots));
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

    public bool BuyRobot(RobotType robotType)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Workshop)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, CaptureStats(), "Not at workshop");
            RaiseEvent("Abu Samir only sells machines from the workshop bench.");
            return false;
        }

        var definition = RobotRegistry.GetByType(robotType);
        if (!Player.Robotics.CanPurchaseRobot)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, CaptureStats(), "Robot limit reached");
            RaiseEvent($"The flat and the alley can only support {RobotRegistry.MaxOwnedRobots} machines at once.");
            return false;
        }

        if (Player.Robotics.Robots.Any(robot => robot.Type == robotType))
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, CaptureStats(), "Already own this robot model");
            RaiseEvent($"You already own a {definition.Name}.");
            return false;
        }

        if (Player.Stats.Money < definition.PurchaseCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, CaptureStats(), $"Not enough money (need {definition.PurchaseCost} LE)");
            RaiseEvent($"You need {definition.PurchaseCost} LE for the {definition.Name}; the seller will not extend credit.");
            return false;
        }

        Player.Stats.ModifyMoney(-definition.PurchaseCost);
        Player.Robotics.PurchaseRobot(robotType, Clock.Day);
        RaiseEvent($"You buy a {definition.Name} for {definition.PurchaseCost} LE. It works, but its warranty expired years ago.");
        RecordMutation(MutationCategories.HouseholdAsset, "BuyRobot", before, CaptureStats(), $"Bought {definition.Name} for {definition.PurchaseCost} LE");
        return true;
    }

    public bool BuyRobotParts(int quantity = 1)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Workshop)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, CaptureStats(), "Not at workshop");
            RaiseEvent("You need Abu Samir's workshop bench to buy robot parts.");
            return false;
        }

        if (quantity <= 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        }

        var cost = quantity * RobotRegistry.PartsPurchaseCost;
        if (!Player.Robotics.CanBuyParts(quantity))
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, CaptureStats(), "Parts storage limit reached");
            RaiseEvent($"You can carry at most {RobotRegistry.MaxParts} spare robot parts in the flat.");
            return false;
        }

        if (Player.Stats.Money < cost)
        {
            RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, CaptureStats(), $"Not enough money (need {cost} LE)");
            RaiseEvent($"You need {cost} LE for {quantity} robot part{(quantity == 1 ? string.Empty : "s")}.");
            return false;
        }

        Player.Stats.ModifyMoney(-cost);
        Player.Robotics.AddParts(quantity);
        RaiseEvent($"You buy {quantity} robot part{(quantity == 1 ? string.Empty : "s")} for {cost} LE and wrap them against the dust.");
        RecordMutation(MutationCategories.HouseholdAsset, "BuyRobotParts", before, CaptureStats(), $"Bought {quantity} robot parts for {cost} LE");
        return true;
    }

    public bool RepairRobot(Guid robotId)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Workshop)
        {
            RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, CaptureStats(), "Not at workshop");
            RaiseEvent("Repairs have to happen at Abu Samir's workshop bench.");
            return false;
        }

        var robot = Player.Robotics.GetRobot(robotId);
        if (robot is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, CaptureStats(), "Robot not found");
            RaiseEvent("You cannot repair a machine that is not yours.");
            return false;
        }

        if (robot.Condition >= 100)
        {
            RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, CaptureStats(), "Robot already fully repaired");
            RaiseEvent("That machine is already running as well as its old parts allow.");
            return false;
        }

        if (Player.Robotics.Parts <= 0)
        {
            RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, CaptureStats(), "No robot parts");
            RaiseEvent("You need at least one spare robot part before Abu Samir will open the casing.");
            return false;
        }

        var definition = RobotRegistry.GetByType(robot.Type);
        if (Player.Stats.Money < definition.RepairCost)
        {
            RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, CaptureStats(), $"Not enough money (need {definition.RepairCost} LE)");
            RaiseEvent($"Bench time and solder cost {definition.RepairCost} LE, even when you bring the part.");
            return false;
        }

        Player.Stats.ModifyMoney(-definition.RepairCost);
        Player.Robotics.TryRepairRobot(robotId);
        RaiseEvent($"Abu Samir uses one spare part to bring your {definition.Name} up to {robot.Condition}% condition.");
        RecordMutation(MutationCategories.HouseholdAsset, "RepairRobot", before, CaptureStats(), $"Repaired {definition.Name} for {definition.RepairCost} LE and one part");
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

    public bool UpgradeFishTank(FishTankUpgradeType upgradeType)
    {
        var before = CaptureStats();
        if (World.CurrentLocationId != LocationId.Home)
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), "Not at home");
            RaiseEvent("You need to be home to work on the fish tank.");
            return false;
        }

        var fishTank = Player.HouseholdAssets.GetFishTank();
        if (fishTank is null)
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), "No fish tank");
            RaiseEvent("You don't have a fish tank to upgrade.");
            return false;
        }

        var cost = FishTankUpgradeCatalog.GetCost(upgradeType);
        if (Player.Stats.Money < cost)
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), $"Not enough money (need {cost} LE, have {Player.Stats.Money} LE)");
            RaiseEvent($"Not enough money. {FishTankUpgradeCatalog.GetName(upgradeType)} costs {cost} LE.");
            return false;
        }

        if (!Player.HouseholdAssets.TryUpgradeFishTank(upgradeType, CurrentWeek))
        {
            RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), $"{FishTankUpgradeCatalog.GetName(upgradeType)} already active");
            RaiseEvent($"{FishTankUpgradeCatalog.GetName(upgradeType)} is already active for the fish tank.");
            return false;
        }

        Player.Stats.ModifyMoney(-cost);
        RaiseEvent($"Fish Tank: {FishTankUpgradeCatalog.GetName(upgradeType)} added for {cost} LE.");
        RecordMutation(MutationCategories.HouseholdAsset, "UpgradeFishTank", before, CaptureStats(), $"Upgraded fish tank with {FishTankUpgradeCatalog.GetName(upgradeType)} for {cost} LE");
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

    public void ApplyRentPayment(int amount)
    {
        if (amount <= 0 || AccumulatedRentDebt <= 0)
        {
            return;
        }

        var payment = Math.Min(Math.Min(amount, AccumulatedRentDebt), Player.Stats.Money);
        if (payment <= 0)
        {
            return;
        }

        Player.Stats.ModifyMoney(-payment);
        _rentState.PayPartialDebt(payment);
        RaiseAutoTransaction($"Paid {payment} LE toward rent arrears.");
    }

    public void GrantRentGraceDays(int days)
    {
        _rentState.AddGraceDays(days);
        if (days > 0)
        {
            RaiseEvent($"The landlord grants {days} day{(days == 1 ? string.Empty : "s")} of rent grace.");
        }
    }

    public void ApplyDebtPayment(DebtSource source, int amount)
    {
        var result = DebtService.Repay(
            source,
            Math.Min(amount, Player.Stats.Money),
            Player,
            PlayerDebts,
            Relationships,
            DistrictHeat,
            World.CurrentDistrict);
        if (!result.Success || result.Payment <= 0)
        {
            return;
        }

        if (result.FullyRepaid)
        {
            var creditorName = result.CreditorNpc?.ToString() ?? source.ToString();
            RaiseAutoTransaction($"Debt to {creditorName} fully repaid: {result.Payment} LE.");
        }
        else
        {
            RaiseAutoTransaction($"Repaid {result.Payment} LE toward {source} debt. Remaining: {result.Remaining} LE.");
        }
    }

    public void ExtendDebtDueDate(DebtSource source, int days)
    {
        if (PlayerDebts.ExtendDueDate(source, days))
        {
            RaiseEvent($"The {source} due date moves back {days} day{(days == 1 ? string.Empty : "s")}.");
        }
    }

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
    {
        Player.HouseholdAssets.Restore(pets, plants, hasStreetCatEncounter, lastStreetCatEncounterDay, totalHerbEarnings);
        Player.Robotics.Restore(robots ?? [], robotParts);
    }

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
                DistrictHeat.AddHeat(World.CurrentDistrict, result.PolicePressureIncrease);
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

    internal void RestoreInvestmentState(
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

    internal void ResolveWeeklyHouseholdAssets()
    {
        var resolution = Player.HouseholdAssets.ResolveWeeklyNeglect(CurrentWeek);
        if (resolution.StressPenalty <= 0)
        {
            return;
        }

        Player.Stats.ModifyStress(resolution.StressPenalty);
        RaiseAutoTransaction($"Skipping household care all week weighs on your mother. Stress +{resolution.StressPenalty}.");
    }

    internal void TryRollStreetCatEncounter(Random random)
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
            Player.Skills.GetLevel(SkillId.Medical),
            Player.Skills.GetLevel(SkillId.Physical),
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
    {
        var result = DebtService.BorrowFromNpc(npc, amount, Clock.Day, Player, Relationships, NpcEconomies, PlayerDebts);
        if (!result.Success)
        {
            return result;
        }

        var before = CaptureStats();
        var debt = PlayerDebts.Debts[^1];
        RecordMutation(MutationCategories.Economy, "TryBorrowFromNpc", before, CaptureStats(), $"Borrowed {result.Amount} LE from {npc} (due day {debt.DueDay})");
        RaiseAutoTransaction($"Borrowed {result.Amount} LE from {npc}.");

        return result;
    }

    public (bool Success, int Amount, string Message) TryBorrowFromLandlord(int amount)
    {
        var result = DebtService.BorrowFromLandlord(amount, Clock.Day, Player, Relationships, _rentState, PlayerDebts);
        if (!result.Success)
        {
            return result;
        }

        var before = CaptureStats();
        RecordMutation(MutationCategories.Economy, "TryBorrowFromLandlord", before, CaptureStats(), $"Landlord advance: {result.Amount} LE (added to rent debt)");
        RaiseAutoTransaction($"Hajj Mahmoud advances you {result.Amount} LE. It's added to your rent debt.");

        return result;
    }

    public (bool Success, int Amount, string Message) TryBorrowFromLoanShark(int amount)
    {
        var result = DebtService.BorrowFromLoanShark(
            amount,
            Clock.Day,
            Player.BackgroundType,
            Player,
            PlayerDebts,
            DistrictHeat,
            World.CurrentDistrict,
            _sharedRandom);
        if (!result.Success)
        {
            return (false, 0, result.Message);
        }

        var before = CaptureStats();
        RecordMutation(MutationCategories.Economy, "TryBorrowFromLoanShark", before, CaptureStats(), $"Loan shark: {result.Amount} LE at {result.InterestBasisPoints}bps, due day {Clock.Day + 7}");
        RaiseAutoTransaction($"A loan shark hands you {result.Amount} LE. The interest is brutal. Due in 7 days.");

        return (true, result.Amount, result.Message);
    }

    public (bool Success, string Message) TryLendToNpc(NpcId npc, int amount)
    {
        if (amount <= 0)
        {
            return (false, "Invalid amount.");
        }

        if (Player.Stats.Money < amount)
        {
            return (false, "You can't afford that.");
        }

        Player.Stats.ModifyMoney(-amount);
        Relationships.ModifyNpcTrust(npc, 4);
        Relationships.RecordFavor(npc, Clock.Day, hasUnpaidDebt: true);
        Relationships.SetHelpedState(npc, true);

        NpcEconomies.AddDebt(DebtorId.Player, new DebtorId.NpcDebtor(npc), amount);

        var before = CaptureStats();
        RecordMutation(MutationCategories.Economy, "TryLendToNpc", before, CaptureStats(), $"Lent {amount} LE to {npc}");
        RaiseAutoTransaction($"You lend {amount} LE to {npc}.");

        return (true, $"You lend {npc} {amount} LE. They'll remember this.");
    }

    public (bool Success, string Message) RefuseNpcLoan(NpcId npc)
    {
#pragma warning disable CA5394
        var trustLoss = _sharedRandom.Next(2, 6);
#pragma warning restore CA5394

        Relationships.ModifyNpcTrust(npc, -trustLoss);
        Relationships.RecordRefusal(npc, Clock.Day);

        var before = CaptureStats();
        RecordMutation(MutationCategories.Economy, "RefuseNpcLoan", before, CaptureStats(), $"Refused loan to {npc}, trust -{trustLoss}");

        return (true, $"{npc} asked for help. You said no. Trust -{trustLoss}.");
    }

    public (bool Success, int Remaining, string Message) RepayDebt(DebtSource source, int amount)
    {
        var result = DebtService.Repay(source, amount, Player, PlayerDebts, Relationships, DistrictHeat, World.CurrentDistrict);
        if (!result.Success)
        {
            return (false, result.Remaining, result.Message);
        }

        if (result.FullyRepaid)
        {
            var creditorName = result.CreditorNpc?.ToString() ?? source.ToString();
            RaiseAutoTransaction($"Debt to {creditorName} fully repaid: {result.Payment} LE.");
        }
        else
        {
            RaiseAutoTransaction($"Repaid {result.Payment} LE toward {source} debt. Remaining: {result.Remaining} LE.");
        }

        var before = CaptureStats();
        RecordMutation(MutationCategories.Economy, "RepayDebt", before, CaptureStats(), $"Repaid {result.Payment} LE ({source}), remaining {result.Remaining} LE");

        return (true, result.Remaining, result.Message);
    }

    internal void RestoreEconomyState(
        IEnumerable<(NpcId Npc, NpcWealthLevel WealthLevel, int Generosity,
            Dictionary<DebtorId, int> OwedTo, Dictionary<DebtorId, int> OwedBy,
            int LastHardshipDay, int LastWindfallDay, int GenerousUntilDay)> npcEconomies,
        IEnumerable<PlayerDebt> playerDebts)
    {
        ArgumentNullException.ThrowIfNull(npcEconomies);
        ArgumentNullException.ThrowIfNull(playerDebts);

        foreach (var entry in npcEconomies)
        {
            NpcEconomies.RestoreEntry(
                entry.Npc, entry.WealthLevel, entry.Generosity,
                entry.OwedTo, entry.OwedBy,
                entry.LastHardshipDay, entry.LastWindfallDay, entry.GenerousUntilDay);
        }

        PlayerDebts.RestoreDebts(playerDebts);
    }

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
