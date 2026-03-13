using System.Reflection;
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
        var story = new Story(LoadStoryResource());
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

        if (_currentStory.currentChoices.Count == 0 && !_currentStory.canContinue)
        {
            LogStoryEnded(_logger);
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

        if (key == "FLAG")
        {
            _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { SetFlag = valueStr });
            return;
        }

        if (key == "MESSAGE")
        {
            _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { Message = valueStr });
            return;
        }

        if (key == "NPC_TRUST")
        {
            var trustParts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
            if (trustParts.Length == 2 && Enum.TryParse<NpcId>(trustParts[0], out var npcId) && int.TryParse(trustParts[1], out var trustDelta))
            {
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { NpcTrustTarget = npcId, NpcTrustChange = trustDelta });
            }

            return;
        }

        if (key == "FACTION_REP")
        {
            var factionParts = valueStr.Split(',', 2, StringSplitOptions.TrimEntries);
            if (factionParts.Length == 2 && Enum.TryParse<FactionId>(factionParts[0], out var factionId) && int.TryParse(factionParts[1], out var reputationDelta))
            {
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { FactionTarget = factionId, FactionReputationChange = reputationDelta });
            }

            return;
        }

        if (TryProcessRelationshipMemoryTag(key, valueStr))
        {
            return;
        }

        if (!int.TryParse(valueStr, out var value))
        {
            return;
        }

        _pendingOutcome ??= new NarrativeOutcome();

        _pendingOutcome = key switch
        {
            "MONEY" => _pendingOutcome with { MoneyChange = _pendingOutcome.MoneyChange + value },
            "HEALTH" => _pendingOutcome with { HealthChange = _pendingOutcome.HealthChange + value },
            "ENERGY" => _pendingOutcome with { EnergyChange = _pendingOutcome.EnergyChange + value },
            "HUNGER" => _pendingOutcome with { HungerChange = _pendingOutcome.HungerChange + value },
            "STRESS" => _pendingOutcome with { StressChange = _pendingOutcome.StressChange + value },
            "MOTHER_HEALTH" => _pendingOutcome with { MotherHealthChange = _pendingOutcome.MotherHealthChange + value },
            "FOOD" => _pendingOutcome with { FoodChange = _pendingOutcome.FoodChange + value },
            _ => _pendingOutcome
        };
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
    }

    private static void TrySetGlobalVariable(Story story, string variableName, object value)
    {
        if (!story.variablesState.GlobalVariableExistsWithName(variableName))
        {
            return;
        }

        story.variablesState[variableName] = value;
    }

    private bool TryProcessRelationshipMemoryTag(string key, string valueStr)
    {
        var npcParts = valueStr.Split(',', StringSplitOptions.TrimEntries);
        if (npcParts.Length == 0 || !Enum.TryParse<NpcId>(npcParts[0], out var npcId))
        {
            return false;
        }

        switch (key)
        {
            case "FAVOR":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { FavorTarget = npcId });
                return true;
            case "REFUSAL":
                _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { RefusalTarget = npcId });
                return true;
            case "DEBT":
                if (npcParts.Length >= 2 && bool.TryParse(npcParts[1], out var debtState))
                {
                    _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { DebtTarget = npcId, DebtState = debtState });
                    return true;
                }

                return false;
            case "EMBARRASSED":
                if (npcParts.Length >= 2 && bool.TryParse(npcParts[1], out var embarrassedState))
                {
                    _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { EmbarrassedTarget = npcId, EmbarrassedState = embarrassedState });
                    return true;
                }

                return false;
            case "HELPED":
                if (npcParts.Length >= 2 && bool.TryParse(npcParts[1], out var helpedState))
                {
                    _pendingOutcome = MergeOutcome(_pendingOutcome, new NarrativeOutcome { HelpedTarget = npcId, HelpedState = helpedState });
                    return true;
                }

                return false;
            default:
                return false;
        }
    }

    private static string LoadStoryResource()
    {
        var filesystemPath = System.IO.Path.Combine("content", "ink", "main.json");
        if (File.Exists(filesystemPath))
        {
            return File.ReadAllText(filesystemPath);
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Slums.Narrative.Ink.Content.main.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Could not find Ink story at {filesystemPath} or as embedded resource: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
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
            NpcTrustTarget = next.NpcTrustTarget ?? existing.NpcTrustTarget,
            NpcTrustChange = existing.NpcTrustChange + next.NpcTrustChange,
            FactionTarget = next.FactionTarget ?? existing.FactionTarget,
            FactionReputationChange = existing.FactionReputationChange + next.FactionReputationChange,
            FavorTarget = next.FavorTarget ?? existing.FavorTarget,
            RefusalTarget = next.RefusalTarget ?? existing.RefusalTarget,
            DebtTarget = next.DebtTarget ?? existing.DebtTarget,
            DebtState = next.DebtState ?? existing.DebtState,
            EmbarrassedTarget = next.EmbarrassedTarget ?? existing.EmbarrassedTarget,
            EmbarrassedState = next.EmbarrassedState ?? existing.EmbarrassedState,
            HelpedTarget = next.HelpedTarget ?? existing.HelpedTarget,
            HelpedState = next.HelpedState ?? existing.HelpedState
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
