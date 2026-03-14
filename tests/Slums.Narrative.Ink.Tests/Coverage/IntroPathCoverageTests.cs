using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class IntroPathCoverageTests
{
    [Test]
    public async Task Intro_MedicalSchoolDropout_KnotExists()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("intro_medical");
        await Assert.That(knots).Contains("intro_medical");
    }

    [Test]
    public async Task Intro_ReleasedPoliticalPrisoner_KnotExists()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("intro_prisoner");
        await Assert.That(knots).Contains("intro_prisoner");
    }

    [Test]
    public async Task Intro_SudaneseRefugee_KnotExists()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("intro_sudanese");
        await Assert.That(knots).Contains("intro_sudanese");
    }

    [Test]
    public async Task Intro_Medical_ProducesTextAndChoices()
    {
        var sceneState = CreateSceneState(BackgroundType.MedicalSchoolDropout);
        var result = StoryTraversalHelper.ExplorePath("intro_medical", sceneState);

        result.Text.Should().NotBeEmpty("intro should produce narrative text");
        result.Choices.Should().NotBeEmpty("intro should offer choices");
    }

    [Test]
    public async Task Intro_Prisoner_ProducesTextAndChoices()
    {
        var sceneState = CreateSceneState(BackgroundType.ReleasedPoliticalPrisoner);
        var result = StoryTraversalHelper.ExplorePath("intro_prisoner", sceneState);

        result.Text.Should().NotBeEmpty("intro should produce narrative text");
        result.Choices.Should().NotBeEmpty("intro should offer choices");
    }

    [Test]
    public async Task Intro_Sudanese_ProducesTextAndChoices()
    {
        var sceneState = CreateSceneState(BackgroundType.SudaneseRefugee);
        var result = StoryTraversalHelper.ExplorePath("intro_sudanese", sceneState);

        result.Text.Should().NotBeEmpty("intro should produce narrative text");
        result.Choices.Should().NotBeEmpty("intro should offer choices");
    }

    [Test]
    public async Task Intro_Medical_AllChoicesReachable()
    {
        var sceneState = CreateSceneState(BackgroundType.MedicalSchoolDropout);
        var results = StoryTraversalHelper.ExploreAllChoices("intro_medical", sceneState, maxDepth: 3);

        results.Should().HaveCountGreaterThan(1, "intro should have multiple choice paths");
        foreach (var result in results)
        {
            result.Text.Should().NotBeEmpty($"path '{result.Path}' should produce text");
        }
    }

    [Test]
    public async Task Intro_Prisoner_AllChoicesReachable()
    {
        var sceneState = CreateSceneState(BackgroundType.ReleasedPoliticalPrisoner);
        var results = StoryTraversalHelper.ExploreAllChoices("intro_prisoner", sceneState, maxDepth: 3);

        results.Should().HaveCountGreaterThan(1, "intro should have multiple choice paths");
        foreach (var result in results)
        {
            result.Text.Should().NotBeEmpty($"path '{result.Path}' should produce text");
        }
    }

    [Test]
    public async Task Intro_Sudanese_AllChoicesReachable()
    {
        var sceneState = CreateSceneState(BackgroundType.SudaneseRefugee);
        var results = StoryTraversalHelper.ExploreAllChoices("intro_sudanese", sceneState, maxDepth: 3);

        results.Should().HaveCountGreaterThan(1, "intro should have multiple choice paths");
        foreach (var result in results)
        {
            result.Text.Should().NotBeEmpty($"path '{result.Path}' should produce text");
        }
    }

    [Test]
    public async Task Intro_Medical_ReferencesMother()
    {
        var sceneState = CreateSceneState(BackgroundType.MedicalSchoolDropout);
        var result = StoryTraversalHelper.ExplorePath("intro_medical", sceneState);

        var allText = string.Join(" ", result.Text);
        allText.Should().Contain("mother", "medical school dropout backstory centers on caring for sick mother");
    }

    [Test]
    public async Task Intro_Prisoner_ReferencesPolitics()
    {
        var sceneState = CreateSceneState(BackgroundType.ReleasedPoliticalPrisoner);
        var result = StoryTraversalHelper.ExplorePath("intro_prisoner", sceneState);

        var allText = string.Join(" ", result.Text);
        var hasRelevantContent = allText.Contains("prison", StringComparison.OrdinalIgnoreCase) ||
                                 allText.Contains("political", StringComparison.OrdinalIgnoreCase) ||
                                 allText.Contains("release", StringComparison.OrdinalIgnoreCase);
        hasRelevantContent.Should().BeTrue("prisoner backstory should reference political imprisonment");
    }

    [Test]
    public async Task Intro_Sudanese_ReferencesRefugeeStatus()
    {
        var sceneState = CreateSceneState(BackgroundType.SudaneseRefugee);
        var result = StoryTraversalHelper.ExplorePath("intro_sudanese", sceneState);

        var allText = string.Join(" ", result.Text);
        var hasRelevantContent = allText.Contains("sudan", StringComparison.OrdinalIgnoreCase) ||
                                 allText.Contains("refugee", StringComparison.OrdinalIgnoreCase) ||
                                 allText.Contains("border", StringComparison.OrdinalIgnoreCase) ||
                                 allText.Contains("cairo", StringComparison.OrdinalIgnoreCase);
        hasRelevantContent.Should().BeTrue("sudanese refugee backstory should reference displacement");
    }

    private static NarrativeSceneState CreateSceneState(BackgroundType backgroundType)
    {
        using var session = GameStateBuilder.BuildForBackground(backgroundType);
        return NarrativeSceneState.Create(session);
    }
}
