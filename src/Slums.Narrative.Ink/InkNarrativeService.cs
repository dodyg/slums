using System.Globalization;
using System.Reflection;
using System.Text;
using Ink.Runtime;
using Microsoft.Extensions.Logging;
using Slums.Application.Narrative;
using Slums.Core.State;

namespace Slums.Narrative.Ink;

public sealed class InkNarrativeService : INarrativeService
{
    private readonly ILogger<InkNarrativeService> _logger;
    private Story? _currentStory;
    private NarrativeOutcome? _pendingOutcome;

    public bool IsSceneActive => _currentStory is not null;
    public string? CurrentText { get; private set; }
    public IReadOnlyList<string> CurrentChoices { get; private set; } = [];

    public InkNarrativeService(ILogger<InkNarrativeService> logger)
    {
        _logger = logger;
    }

    public void StartScene(string knotName, GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(knotName);
        ArgumentNullException.ThrowIfNull(gameState);

        string storyJson;
        try
        {
            storyJson = LoadStoryResource();
        }
        catch (InvalidOperationException ex)
        {
            LogStoryLoadFailed(_logger, ex);
            EndScene();
            return;
        }

        try
        {
            _currentStory = new Story(storyJson);
            SyncVariablesToInk(gameState);
            _currentStory.ChoosePathString(knotName);
            ContinueStory();
            LogSceneStarted(_logger, knotName);
        }
        catch (StoryException ex)
        {
            LogSceneStartFailed(_logger, knotName, ex);
            EndScene();
        }
        catch (ArgumentException ex)
        {
            LogSceneStartFailed(_logger, knotName, ex);
            EndScene();
        }
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

    private void SyncVariablesToInk(GameState gameState)
    {
        if (_currentStory is null)
        {
            return;
        }

        _currentStory.variablesState["money"] = gameState.Player.Stats.Money;
        _currentStory.variablesState["health"] = gameState.Player.Stats.Health;
        _currentStory.variablesState["energy"] = gameState.Player.Stats.Energy;
        _currentStory.variablesState["hunger"] = gameState.Player.Stats.Hunger;
        _currentStory.variablesState["stress"] = gameState.Player.Stats.Stress;
        _currentStory.variablesState["mother_health"] = gameState.Player.Household.MotherHealth;
        _currentStory.variablesState["food_stockpile"] = gameState.Player.Household.FoodStockpile;
        _currentStory.variablesState["day"] = gameState.Clock.Day;

        if (gameState.Player.Background is not null)
        {
            _currentStory.variablesState["background"] = gameState.Player.Background.Type.ToString();
        }
    }

    private static string LoadStoryResource()
    {
        var filesystemPath = Path.Combine("content", "ink", "main.json");
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

    private static readonly Action<ILogger, string, Exception?> LogSceneStartedDelegate =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "SceneStarted"), "Started Ink scene: {KnotName}");

    private static readonly Action<ILogger, string, Exception?> LogSceneStartFailedDelegate =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(2, "SceneStartFailed"), "Failed to start Ink scene: {KnotName}");

    private static readonly Action<ILogger, Exception?> LogStoryLoadFailedDelegate =
        LoggerMessage.Define(LogLevel.Error, new EventId(3, "StoryLoadFailed"), "Failed to load Ink story resource");

    private static readonly Action<ILogger, int, Exception?> LogInvalidChoiceDelegate =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(4, "InvalidChoice"), "Invalid choice selection: {ChoiceIndex}");

    private static readonly Action<ILogger, Exception?> LogSceneEndedDelegate =
        LoggerMessage.Define(LogLevel.Debug, new EventId(5, "SceneEnded"), "Ended Ink scene");

    private static readonly Action<ILogger, Exception?> LogStoryEndedDelegate =
        LoggerMessage.Define(LogLevel.Debug, new EventId(6, "StoryEnded"), "Story reached natural end");

    private static void LogSceneStarted(ILogger logger, string knotName) => LogSceneStartedDelegate(logger, knotName, null);
    private static void LogSceneStartFailed(ILogger logger, string knotName, Exception ex) => LogSceneStartFailedDelegate(logger, knotName, ex);
    private static void LogStoryLoadFailed(ILogger logger, Exception ex) => LogStoryLoadFailedDelegate(logger, ex);
    private static void LogInvalidChoice(ILogger logger, int index) => LogInvalidChoiceDelegate(logger, index, null);
    private static void LogSceneEnded(ILogger logger) => LogSceneEndedDelegate(logger, null);
    private static void LogStoryEnded(ILogger logger) => LogStoryEndedDelegate(logger, null);
}
