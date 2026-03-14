using System.Diagnostics;
using System.Reflection;
using Ink.Runtime;
using Slums.Application.Narrative;

namespace Slums.Narrative.Ink.Tests.Helpers;

internal static class StoryTraversalHelper
{
    public static Story LoadStory()
    {
        var filesystemPath = System.IO.Path.Combine("content", "ink", "main.json");
        if (File.Exists(filesystemPath))
        {
            return new Story(File.ReadAllText(filesystemPath));
        }

        var assembly = typeof(InkNarrativeService).Assembly;
        var resourceName = "Slums.Narrative.Ink.Content.main.json";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not find Ink story resource: {resourceName}");

        using var reader = new StreamReader(stream);
        return new Story(reader.ReadToEnd());
    }

    public static IReadOnlyList<string> GetAllKnotNames()
    {
        var story = LoadStory();
        var knots = new List<string>();

        foreach (var knot in story.mainContentContainer.namedOnlyContent.Keys)
        {
            knots.Add(knot);
        }

        return knots;
    }

    public static IReadOnlyList<string> GetKnotsMatchingPrefix(string prefix)
    {
        return GetAllKnotNames()
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static StoryPathResult ExplorePath(string knotName, NarrativeSceneState? sceneState = null)
    {
        var story = LoadStory();

        if (sceneState is not null)
        {
            SyncVariablesToInk(story, sceneState);
        }

        story.ChoosePathString(knotName);

        var text = new List<string>();
        var choices = new List<string>();
        var outcomes = new List<string>();

        while (story.canContinue)
        {
            var content = story.Continue();
            if (!string.IsNullOrWhiteSpace(content))
            {
                text.Add(content.Trim());
            }

            outcomes.AddRange(story.currentTags);
        }

        choices.AddRange(story.currentChoices.Select(c => c.text));

        return new StoryPathResult(knotName, text, choices, outcomes);
    }

    public static IReadOnlyList<StoryPathResult> ExploreAllChoices(string knotName, NarrativeSceneState? sceneState = null, int maxDepth = 5)
    {
        var results = new List<StoryPathResult>();
        ExploreChoicesRecursive(knotName, sceneState, maxDepth, results, []);
        return results;
    }

    private static void ExploreChoicesRecursive(
        string knotName,
        NarrativeSceneState? sceneState,
        int remainingDepth,
        List<StoryPathResult> results,
        HashSet<string> visited)
    {
        if (remainingDepth <= 0 || visited.Contains(knotName))
        {
            return;
        }

        visited.Add(knotName);

        var result = ExplorePath(knotName, sceneState);
        results.Add(result);

        if (result.Choices.Count == 0)
        {
            return;
        }

        var choiceList = result.Choices.ToList();
        for (var choiceIndex = 0; choiceIndex < choiceList.Count; choiceIndex++)
        {
            var choiceText = choiceList[choiceIndex];

            try
            {
                var story = LoadStory();
                if (sceneState is not null)
                {
                    SyncVariablesToInk(story, sceneState);
                }

                story.ChoosePathString(knotName);

                while (story.canContinue)
                {
                    story.Continue();
                }

                if (choiceIndex < story.currentChoices.Count)
                {
                    story.ChooseChoiceIndex(choiceIndex);

                    var branchText = new List<string>();
                    var branchOutcomes = new List<string>();

                    while (story.canContinue)
                    {
                        var content = story.Continue();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            branchText.Add(content.Trim());
                        }
                        branchOutcomes.AddRange(story.currentTags);
                    }

                    if (branchText.Count > 0 || branchOutcomes.Count > 0)
                    {
                        results.Add(new StoryPathResult($"{knotName} -> {choiceText}", branchText, story.currentChoices.Select(c => c.text).ToList(), branchOutcomes));
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Trace.TraceWarning("Skipping story branch '{0}' choice '{1}' due to invalid story state.", knotName, choiceText);
            }
            catch (ArgumentException)
            {
                Trace.TraceWarning("Skipping story branch '{0}' choice '{1}' due to an invalid story path.", knotName, choiceText);
            }
        }
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
        if (story.variablesState.GlobalVariableExistsWithName(variableName))
        {
            story.variablesState[variableName] = value;
        }
    }
}

internal sealed record StoryPathResult(
    string Path,
    IReadOnlyList<string> Text,
    IReadOnlyList<string> Choices,
    IReadOnlyList<string> OutcomeTags);
