using System.Text;
using Ink.Runtime;
using Microsoft.Extensions.Logging;
using Slums.Application.Narrative;
using Slums.Core.Relationships;

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
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { SetFlag = valueStr });
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

    private static NpcId ParseNpcTarget(string tag, string valueStr)
    {
        var npc = valueStr.Split(',', StringSplitOptions.TrimEntries)[0];
        if (!Enum.TryParse<NpcId>(npc, out var npcId))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': unknown NPC '{npc}'.");
        }

        return npcId;
    }

    private static NpcTrustEffect ParseNpcTrustEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !int.TryParse(parts[1], out var delta))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,delta'.");
        }

        return new NpcTrustEffect(npcId, delta);
    }

    private static FactionReputationEffect ParseFactionReputationEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<FactionId>(parts[0], out var factionId) || !int.TryParse(parts[1], out var delta))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'Faction,delta'.");
        }

        return new FactionReputationEffect(factionId, delta);
    }

    private static DebtEffect ParseDebtEffect(string tag, string valueStr)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !bool.TryParse(parts[1], out var debtState))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,true|false'.");
        }

        return new DebtEffect(npcId, debtState);
    }

    private static NarrativeEffect ParseBoolStateEffect(string tag, string valueStr, Func<NpcId, bool, NarrativeEffect> factory)
    {
        var parts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !Enum.TryParse<NpcId>(parts[0], out var npcId) || !bool.TryParse(parts[1], out var state))
        {
            throw new InvalidOperationException($"Malformed narrative effect tag '{tag}': expected 'NPC,true|false'.");
        }

        return factory(npcId, state);
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
            SetFlag = next.SetFlag ?? existing.SetFlag,
            Message = string.IsNullOrWhiteSpace(existing.Message) ? next.Message : string.Join(" ", new[] { existing.Message, next.Message }.Where(static message => !string.IsNullOrWhiteSpace(message))),
            Effects = existing.Effects.Concat(next.Effects).ToArray()
        };
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
