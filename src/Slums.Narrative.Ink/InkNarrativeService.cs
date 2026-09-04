using System.Text;
using Ink.Runtime;
using Microsoft.Extensions.Logging;
using Slums.Application.Narrative;
using Slums.Core.Relationships;
using Slums.Core.Economy;
using Slums.Core.Endings;
using Slums.Core.Narrative;

namespace Slums.Narrative.Ink;

public sealed class InkNarrativeService : INarrativeService
{
    private readonly ILogger<InkNarrativeService> _logger;
    private Story? _currentStory;
    private NarrativeOutcome? _pendingOutcome;

    public bool IsSceneActive => _currentStory is not null;
    public string? CurrentText { get; private set; }
    public IReadOnlyList<string> CurrentChoices { get; private set; } = [];
    public string? LastKnot { get; private set; }

    public InkNarrativeService(ILogger<InkNarrativeService> logger)
    {
        _logger = logger;
    }

    public void StartScene(string knotName, NarrativeSceneState sceneState)
    {
        ArgumentNullException.ThrowIfNull(knotName);
        ArgumentNullException.ThrowIfNull(sceneState);

        LastKnot = knotName;
        _pendingOutcome = null;
        _currentStory = null;
        var story = InkStoryFactory.Create(InkStoryLoader.LoadStoryJson());
        SyncVariablesToInk(story, sceneState);
        story.ChoosePathString(knotName);
        _currentStory = story;
        ContinueStory();
        LogSceneStarted(_logger, knotName);
    }

    public void RestoreProgress(string? lastKnot)
    {
        LastKnot = string.IsNullOrWhiteSpace(lastKnot) ? null : lastKnot;
        _pendingOutcome = null;
        _currentStory = null;
        CurrentText = null;
        CurrentChoices = [];
    }

    public void SelectChoice(int choiceIndex)
    {
        if (_currentStory is null || choiceIndex < 0 || choiceIndex >= _currentStory.currentChoices.Count)
        {
            LogInvalidChoice(_logger, choiceIndex);
            return;
        }

        _currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }

    public void EndScene()
    {
        _currentStory = null;
        CurrentText = null;
        CurrentChoices = [];
        LogSceneEnded(_logger);
    }

    public NarrativeOutcome? GetPendingOutcome()
    {
        return _pendingOutcome;
    }

    public void ClearPendingOutcome()
    {
        _pendingOutcome = null;
    }

    private void ContinueStory()
    {
        if (_currentStory is null)
        {
            return;
        }

        var textBuilder = new StringBuilder();

        while (_currentStory.canContinue)
        {
            var text = _currentStory.Continue();
            if (!string.IsNullOrEmpty(text))
            {
                textBuilder.AppendLine(text.Trim());
            }

            ProcessTags();
        }

        CurrentText = textBuilder.ToString().Trim();
        CurrentChoices = _currentStory.currentChoices.Select(static c => c.text).ToList();

        if (_currentStory.currentChoices.Count == 0 && !_currentStory.canContinue && string.IsNullOrWhiteSpace(CurrentText))
        {
            LogStoryEnded(_logger);
            _currentStory = null;
        }
    }

    private void ProcessTags()
    {
        if (_currentStory is null)
        {
            return;
        }

        foreach (var tag in _currentStory.currentTags)
        {
            ProcessTag(tag);
        }
    }

    private void ProcessTag(string tag)
    {
        var parts = tag.Split(':', 2);
        if (parts.Length != 2)
        {
            return;
        }

        var key = parts[0].Trim().ToUpperInvariant();
        var valueStr = parts[1].Trim();

        switch (key)
        {
            case "FLAG":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { SetFlag = valueStr, SetFlags = [valueStr] });
                return;

            case "MESSAGE":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Message = valueStr });
                return;

            case "NPC_TRUST":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseNpcTrustEffect(tag, valueStr)] });
                return;

            case "FACTION_REP":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseFactionReputationEffect(tag, valueStr)] });
                return;

            case "FAVOR":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new FavorEffect(ParseNpcTarget(tag, valueStr))] });
                return;

            case "REFUSAL":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new RefusalEffect(ParseNpcTarget(tag, valueStr))] });
                return;

            case "DEBT":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseDebtEffect(tag, valueStr)] });
                return;

            case "EMBARRASSED":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseBoolStateEffect(tag, valueStr, static (npc, value) => new EmbarrassedEffect(npc, value))] });
                return;

            case "HELPED":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseBoolStateEffect(tag, valueStr, static (npc, value) => new HelpedEffect(npc, value))] });
                return;

            case "MONEY":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { MoneyChange = ParseIntEffect(tag, valueStr) });
                return;

            case "HEALTH":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { HealthChange = ParseIntEffect(tag, valueStr) });
                return;

            case "ENERGY":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { EnergyChange = ParseIntEffect(tag, valueStr) });
                return;

            case "HUNGER":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { HungerChange = ParseIntEffect(tag, valueStr) });
                return;

            case "STRESS":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { StressChange = ParseIntEffect(tag, valueStr) });
                return;

            case "MOTHER_HEALTH":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { MotherHealthChange = ParseIntEffect(tag, valueStr) });
                return;

            case "FOOD":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { FoodChange = ParseIntEffect(tag, valueStr) });
                return;

            case "RENT_PAYMENT":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new RentPaymentEffect(ParseIntEffect(tag, valueStr))] });
                return;

            case "RENT_GRACE_DAYS":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new RentGraceDaysEffect(ParseIntEffect(tag, valueStr))] });
                return;

            case "DEBT_PAYMENT":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseDebtAmountEffect(tag, valueStr, static (source, amount) => new DebtPaymentEffect(source, amount))] });
                return;

            case "DEBT_DUE_EXTENSION":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseDebtAmountEffect(tag, valueStr, static (source, amount) => new DebtDueExtensionEffect(source, amount))] });
                return;

            case "RAMADAN_FASTING":
                if (!bool.TryParse(valueStr, out var isFasting))
                {
                    throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected true or false.");
                }
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new RamadanFastingEffect(isFasting)] });
                return;

            case "CRISIS_EVIDENCE":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new CrisisEvidenceEffect(ParsePositiveIntEffect(tag, valueStr))] });
                return;

            case "CRISIS_RESOURCES":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new CrisisResourcesEffect(ParsePositiveIntEffect(tag, valueStr))] });
                return;

            case "CRISIS_DECISION":
                if (!Enum.TryParse<CityCrisisDecision>(valueStr, out var decision) || !Enum.IsDefined(decision) || decision == CityCrisisDecision.None)
                {
                    throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown crisis decision '{valueStr}'.");
                }
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new CrisisDecisionEffect(decision)] });
                return;

            case "CRISIS_RESOLUTION":
                if (!Enum.TryParse<CityCrisisResolution>(valueStr, out var resolution) || !Enum.IsDefined(resolution) || resolution == CityCrisisResolution.Unresolved)
                {
                    throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown crisis resolution '{valueStr}'.");
                }
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new CrisisResolutionEffect(resolution)] });
                return;

            case "POLICE":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [new PolicePressureEffect(ParseIntEffect(tag, valueStr))] });
                return;

            case "ENDING_COMMIT":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseEndingCommitment(tag, valueStr)] });
                return;

            case "CENTRAL_DECISION":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Effects = [ParseCentralDecision(tag, valueStr)] });
                return;

            default:
                // Not an effect tag; scene markers (e.g. weather/season tags) are ignored.
                return;
        }
    }

    private static int ParseIntEffect(string tag, string valueStr)
    {
        if (!int.TryParse(valueStr, out var value))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected an integer value.");
        }

        return value;
    }

    private static int ParsePositiveIntEffect(string tag, string valueStr)
    {
        var value = ParseIntEffect(tag, valueStr);
        if (value <= 0)
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected a positive integer value.");
        }

        return value;
    }

    private static NpcId ParseNpcTarget(string tag, string valueStr)
    {
        var npc = valueStr.Split(',', StringSplitOptions.TrimEntries)[0];
        if (!Enum.TryParse<NpcId>(npc, out var npcId) || !Enum.IsDefined(npcId))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown NPC '{npc}'.");
        }

        return npcId;
    }

    private static NpcTrustEffect ParseNpcTrustEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !Enum.IsDefined(npcId) || !int.TryParse(parts[1], out var delta))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,delta'.");
        }

        return new NpcTrustEffect(npcId, delta);
    }

    private static FactionReputationEffect ParseFactionReputationEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<FactionId>(parts[0], out var factionId) || !Enum.IsDefined(factionId) || !int.TryParse(parts[1], out var delta))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'Faction,delta'.");
        }

        return new FactionReputationEffect(factionId, delta);
    }

    private static DebtEffect ParseDebtEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !Enum.IsDefined(npcId) || !bool.TryParse(parts[1], out var debtState))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,true|false'.");
        }

        return new DebtEffect(npcId, debtState);
    }

    private static TEffect ParseDebtAmountEffect<TEffect>(string tag, string valueStr, Func<DebtSource, int, TEffect> factory)
        where TEffect : NarrativeEffect
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<DebtSource>(parts[0], out var source) || !Enum.IsDefined(source) || !int.TryParse(parts[1], out var amount) || amount <= 0)
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'DebtSource,positiveAmount'.");
        }

        return factory(source, amount);
    }

    private static NarrativeEffect ParseBoolStateEffect(string tag, string valueStr, Func<NpcId, bool, NarrativeEffect> factory)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !Enum.IsDefined(npcId) || !bool.TryParse(parts[1], out var state))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,true|false'.");
        }

        return factory(npcId, state);
    }

    private static EndingCommitmentEffect ParseEndingCommitment(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<EndingId>(parts[0], out var ending) || !Enum.IsDefined(ending) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'EndingId,sacrifice'.");
        }

        return new EndingCommitmentEffect(ending, parts[1]);
    }

    private static CentralCharacterDecisionEffect ParseCentralDecision(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !Enum.TryParse<CentralCharacterId>(parts[0], out var character)
            || !Enum.IsDefined(character)
            || !Enum.TryParse<CentralArcDecision>(parts[1], out var decision)
            || !Enum.IsDefined(decision))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'Character,Decision'.");
        }

        return new CentralCharacterDecisionEffect(character, decision);
    }

    private static void SyncVariablesToInk(Story story, NarrativeSceneState sceneState)
    {
        TrySetGlobalVariable(story, "money", sceneState.Money);
        TrySetGlobalVariable(story, "health", sceneState.Health);
        TrySetGlobalVariable(story, "energy", sceneState.Energy);
        TrySetGlobalVariable(story, "hunger", sceneState.Hunger);
        TrySetGlobalVariable(story, "stress", sceneState.Stress);
        TrySetGlobalVariable(story, "mother_health", sceneState.MotherHealth);
        TrySetGlobalVariable(story, "food_stockpile", sceneState.FoodStockpile);
        TrySetGlobalVariable(story, "day", sceneState.Day);
        TrySetGlobalVariable(story, "district", sceneState.District);
        TrySetGlobalVariable(story, "weather", sceneState.Weather);
        TrySetGlobalVariable(story, "season", sceneState.Season);
        TrySetGlobalVariable(story, "holiday", sceneState.Holiday);
        TrySetGlobalVariable(story, "is_ramadan", sceneState.IsRamadan);
        TrySetGlobalVariable(story, "is_fasting", sceneState.IsRamadanFasting);
        TrySetGlobalVariable(story, "unpaid_rent_days", sceneState.UnpaidRentDays);
        TrySetGlobalVariable(story, "rent_debt", sceneState.RentDebt);
        TrySetGlobalVariable(story, "rent_grace_days", sceneState.RentGraceDays);
        TrySetGlobalVariable(story, "police_pressure", sceneState.PolicePressure);
        TrySetGlobalVariable(story, "operational_robot_count", sceneState.OperationalRobots.Count);
        TrySetGlobalVariable(story, "active_news_count", sceneState.ActiveNews.Count);
        TrySetGlobalVariable(story, "infrastructure_disruption_count", sceneState.Infrastructure.Count);
        TrySetGlobalVariable(story, "mona_trust", sceneState.RelationshipTrust.GetValueOrDefault("NeighborMona"));
        TrySetGlobalVariable(story, "salma_trust", sceneState.RelationshipTrust.GetValueOrDefault("NurseSalma"));
        TrySetGlobalVariable(story, "conversation_variant", sceneState.ConversationVariantId);
        TrySetGlobalVariable(story, "conversation_context", sceneState.ConversationContext);
        TrySetGlobalVariable(story, "conversation_npc", sceneState.ConversationNpc);

        var variantParts = sceneState.ConversationVariantId.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (variantParts.Length >= 2
            && int.TryParse(variantParts[^2], out var opener)
            && int.TryParse(variantParts[^1], out var body))
        {
        TrySetGlobalVariable(story, "conversation_opener", opener);
            TrySetGlobalVariable(story, "conversation_body", body);
        }

        TrySetGlobalVariable(story, "crisis_phase", sceneState.CrisisPhase.ToString());
        TrySetGlobalVariable(story, "crisis_evidence", sceneState.CrisisEvidenceCollected);
        TrySetGlobalVariable(story, "crisis_resources", sceneState.CrisisResourcesCommitted);
        TrySetGlobalVariable(story, "crisis_condition", sceneState.CrisisCooperativeCondition);
        TrySetGlobalVariable(story, "crisis_decision", sceneState.CrisisDecision.ToString());
        TrySetGlobalVariable(story, "crisis_resolution_state", sceneState.CrisisResolution.ToString());
        TrySetGlobalVariable(story, "pending_ending", sceneState.PendingEnding);
        TrySetGlobalVariable(story, "handset_data_exposure", sceneState.HandsetDataExposure);
        TrySetGlobalVariable(story, "microgrid_repair_debt", sceneState.MicrogridRepairDebt);
        TrySetGlobalVariable(story, "microgrid_storage_condition", sceneState.MicrogridStorageCondition);
        TrySetGlobalVariable(story, "transit_permit_review", sceneState.TransitPermitReview);
        TrySetGlobalVariable(story, "biometric_appeal_pending", sceneState.BiometricAppealPending);
        TrySetGlobalVariable(story, "last_telemedicine_triage_day", sceneState.LastTelemedicineTriageDay);
        TrySetGlobalVariable(story, "allocation_model_confidence", sceneState.AllocationModelConfidence);
        TrySetGlobalVariable(story, "mother_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("Mother", string.Empty));
        TrySetGlobalVariable(story, "mona_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("NeighborMona", string.Empty));
        TrySetGlobalVariable(story, "salma_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("NurseSalma", string.Empty));
        TrySetGlobalVariable(story, "mahmoud_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("HajjMahmoud", string.Empty));
        TrySetGlobalVariable(story, "ummkarim_arc_decision", sceneState.CentralDecisions.GetValueOrDefault("UmmKarim", string.Empty));

        if (!string.IsNullOrWhiteSpace(sceneState.Background))
        {
            TrySetGlobalVariable(story, "background", sceneState.Background);
        }

        if (!string.IsNullOrWhiteSpace(sceneState.Gender))
        {
            TrySetGlobalVariable(story, "gender", sceneState.Gender);
        }
    }

    private static void TrySetGlobalVariable(Story story, string variableName, object value)
    {
        if (!story.variablesState.GlobalVariableExistsWithName(variableName))
        {
            return;
        }

        story.variablesState[variableName] = value;
    }

    private static NarrativeOutcome MergeOutcome(NarrativeOutcome? existing, NarrativeOutcome next)
    {
        if (existing is null)
        {
            return next;
        }

        return existing with
        {
            MoneyChange = existing.MoneyChange + next.MoneyChange,
            HealthChange = existing.HealthChange + next.HealthChange,
            EnergyChange = existing.EnergyChange + next.EnergyChange,
            HungerChange = existing.HungerChange + next.HungerChange,
            StressChange = existing.StressChange + next.StressChange,
            MotherHealthChange = existing.MotherHealthChange + next.MotherHealthChange,
            FoodChange = existing.FoodChange + next.FoodChange,
            SetFlags = MergeFlags(existing, next),
            SetFlag = next.SetFlag ?? existing.SetFlag,
            Message = string.IsNullOrWhiteSpace(existing.Message) ? next.Message : string.Join(" ", new[] { existing.Message, next.Message }.Where(static message => !string.IsNullOrWhiteSpace(message))),
            Effects = existing.Effects.Concat(next.Effects).ToArray()
        };
    }

    private static string[] MergeFlags(NarrativeOutcome existing, NarrativeOutcome next)
    {
        var existingFlags = existing.SetFlags.Count > 0
            ? existing.SetFlags
            : existing.SetFlag is { } existingFlag ? [existingFlag] : [];
        var nextFlags = next.SetFlags.Count > 0
            ? next.SetFlags
            : next.SetFlag is { } nextFlag ? [nextFlag] : [];

        return existingFlags.Concat(nextFlags).ToArray();
    }

    private static readonly Action<ILogger, string, Exception?> LogSceneStartedDelegate =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "SceneStarted"), "Started Ink scene: {KnotName}");

    private static readonly Action<ILogger, int, Exception?> LogInvalidChoiceDelegate =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(2, "InvalidChoice"), "Invalid choice selection: {ChoiceIndex}");

    private static readonly Action<ILogger, Exception?> LogSceneEndedDelegate =
        LoggerMessage.Define(LogLevel.Debug, new EventId(3, "SceneEnded"), "Ended Ink scene");

    private static readonly Action<ILogger, Exception?> LogStoryEndedDelegate =
        LoggerMessage.Define(LogLevel.Debug, new EventId(4, "StoryEnded"), "Story reached natural end");

    private static void LogSceneStarted(ILogger logger, string knotName) =>
        LogSceneStartedDelegate(logger, knotName, null);

    private static void LogInvalidChoice(ILogger logger, int choiceIndex) =>
        LogInvalidChoiceDelegate(logger, choiceIndex, null);

    private static void LogSceneEnded(ILogger logger) =>
        LogSceneEndedDelegate(logger, null);

    private static void LogStoryEnded(ILogger logger) =>
        LogStoryEndedDelegate(logger, null);
}
