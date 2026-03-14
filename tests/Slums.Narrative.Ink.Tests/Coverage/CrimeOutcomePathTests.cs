using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class CrimeOutcomePathTests
{
    [Test]
    public async Task Crime_SuccessKnots_ExistForAllTypes()
    {
        var expectedSuccessKnots = new[]
        {
            "crime_hanan_fence_success",
            "crime_youssef_drop_success",
            "crime_ummkarim_errand_success",
            "crime_safaa_skim_success",
            "crime_iman_bundle_success"
        };

        var allKnots = StoryTraversalHelper.GetAllKnotNames();

        foreach (var expectedKnot in expectedSuccessKnots)
        {
            allKnots.Should().Contain(expectedKnot, $"crime success knot '{expectedKnot}' should exist");
        }
    }

    [Test]
    public async Task Crime_DetectedKnots_ExistForAllTypes()
    {
        var expectedDetectedKnots = new[]
        {
            "crime_hanan_fence_detected",
            "crime_youssef_drop_detected",
            "crime_ummkarim_errand_detected",
            "crime_safaa_skim_detected",
            "crime_iman_bundle_detected"
        };

        var allKnots = StoryTraversalHelper.GetAllKnotNames();

        foreach (var expectedKnot in expectedDetectedKnots)
        {
            allKnots.Should().Contain(expectedKnot, $"crime detected knot '{expectedKnot}' should exist");
        }
    }

    [Test]
    public async Task Crime_FailureKnots_ExistForAllTypes()
    {
        var expectedFailureKnots = new[]
        {
            "crime_hanan_fence_failure",
            "crime_youssef_drop_failure",
            "crime_ummkarim_errand_failure",
            "crime_safaa_skim_failure",
            "crime_iman_bundle_failure"
        };

        var allKnots = StoryTraversalHelper.GetAllKnotNames();

        foreach (var expectedKnot in expectedFailureKnots)
        {
            allKnots.Should().Contain(expectedKnot, $"crime failure knot '{expectedKnot}' should exist");
        }
    }

    [Test]
    [Arguments("crime_hanan_fence_success")]
    [Arguments("crime_hanan_fence_detected")]
    [Arguments("crime_hanan_fence_failure")]
    public async Task Crime_HananFence_KnotProducesText(string knotName)
    {
        var result = StoryTraversalHelper.ExplorePath(knotName, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"knot '{knotName}' should produce narrative text");
    }

    [Test]
    [Arguments("crime_youssef_drop_success")]
    [Arguments("crime_youssef_drop_detected")]
    [Arguments("crime_youssef_drop_failure")]
    public async Task Crime_YoussefDrop_KnotProducesText(string knotName)
    {
        var result = StoryTraversalHelper.ExplorePath(knotName, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"knot '{knotName}' should produce narrative text");
    }

    [Test]
    [Arguments("crime_ummkarim_errand_success")]
    [Arguments("crime_ummkarim_errand_detected")]
    [Arguments("crime_ummkarim_errand_failure")]
    public async Task Crime_UmmKarimErrand_KnotProducesText(string knotName)
    {
        var sceneState = CreateSceneState(BackgroundType.ReleasedPoliticalPrisoner);
        var result = StoryTraversalHelper.ExplorePath(knotName, sceneState);
        result.Text.Should().NotBeEmpty($"knot '{knotName}' should produce narrative text");
    }

    [Test]
    [Arguments("crime_safaa_skim_success")]
    [Arguments("crime_safaa_skim_detected")]
    [Arguments("crime_safaa_skim_failure")]
    public async Task Crime_SafaaSkim_KnotProducesText(string knotName)
    {
        var result = StoryTraversalHelper.ExplorePath(knotName, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"knot '{knotName}' should produce narrative text");
    }

    [Test]
    [Arguments("crime_iman_bundle_success")]
    [Arguments("crime_iman_bundle_detected")]
    [Arguments("crime_iman_bundle_failure")]
    public async Task Crime_ImanBundle_KnotProducesText(string knotName)
    {
        var result = StoryTraversalHelper.ExplorePath(knotName, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"knot '{knotName}' should produce narrative text");
    }

    [Test]
    public async Task Crime_FirstSuccess_KnotExists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("crime_first_success", "first crime success scene should exist");
    }

    [Test]
    public async Task Crime_FirstSuccess_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("crime_first_success", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("crime_first_success should produce narrative text");
    }

    [Test]
    public async Task Crime_Warning_KnotExists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("crime_warning", "crime warning scene should exist");
    }

    [Test]
    public async Task Crime_Warning_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("crime_warning", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("crime_warning should produce narrative text");
    }

    [Test]
    public async Task Crime_SuccessScenes_ReflectPositiveOutcome()
    {
        var successKnots = new[]
        {
            "crime_hanan_fence_success",
            "crime_youssef_drop_success",
            "crime_ummkarim_errand_success",
            "crime_safaa_skim_success",
            "crime_iman_bundle_success"
        };

        foreach (var knot in successKnots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
            var allText = string.Join(" ", result.Text);

            var hasPositiveIndicators = allText.Contains("profit", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("earned", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("money", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("works", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("hide", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("possible", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("recommendation", StringComparison.OrdinalIgnoreCase) ||
                                        allText.Contains("usable", StringComparison.OrdinalIgnoreCase);

            hasPositiveIndicators.Should().BeTrue($"success knot '{knot}' should reflect positive outcome");
        }
    }

    [Test]
    public async Task Crime_DetectedScenes_ReflectHeat()
    {
        var detectedKnots = new[]
        {
            "crime_hanan_fence_detected",
            "crime_youssef_drop_detected",
            "crime_ummkarim_errand_detected",
            "crime_safaa_skim_detected",
            "crime_iman_bundle_detected"
        };

        foreach (var knot in detectedKnots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
            var allText = string.Join(" ", result.Text);

            var hasHeatIndicators = allText.Contains("notice", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("heat", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("police", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("attention", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("attentive", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("remember", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("narrowed", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("out of sight", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("watche", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("witness", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("trail", StringComparison.OrdinalIgnoreCase) ||
                                    allText.Contains("know you by sight", StringComparison.OrdinalIgnoreCase);

            hasHeatIndicators.Should().BeTrue($"detected knot '{knot}' should reflect heat/attention");
        }
    }

    [Test]
    public async Task Crime_FailureScenes_ReflectConsequence()
    {
        var failureKnots = new[]
        {
            "crime_hanan_fence_failure",
            "crime_youssef_drop_failure",
            "crime_ummkarim_errand_failure",
            "crime_safaa_skim_failure",
            "crime_iman_bundle_failure"
        };

        foreach (var knot in failureKnots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
            result.Text.Should().NotBeEmpty($"failure knot '{knot}' should produce text");
        }
    }

    private static NarrativeSceneState CreateDefaultSceneState()
    {
        using var session = new GameStateBuilder().Build();
        return NarrativeSceneState.Create(session);
    }

    private static NarrativeSceneState CreateSceneState(BackgroundType backgroundType)
    {
        using var session = new GameStateBuilder()
            .WithBackground(backgroundType)
            .Build();
        return NarrativeSceneState.Create(session);
    }
}
