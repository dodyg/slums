using System.Text;
using Ink.Runtime;
using Microsoft.Extensions.Logging;
using Slums.Application.Narrative;

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
        InkVariableSynchronizer.SyncVariablesToInk(story, sceneState);
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

    public NarrativeOutcome? GetPendingOutcome() => _pendingOutcome;

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
            var outcome = InkTagEffectParser.ParseOutcome(tag);
            if (outcome is not null)
            {
                _pendingOutcome = NarrativeOutcomeMerger.MergeOutcome(_pendingOutcome, outcome);
            }
        }
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
