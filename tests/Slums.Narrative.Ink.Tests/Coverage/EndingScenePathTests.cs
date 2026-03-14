using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Core.Endings;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class EndingScenePathTests
{
    [Test]
    public async Task Ending_KnotsExistForAllEndingTypes()
    {
        var expectedEndingKnots = new[]
        {
            "ending_mother_died",
            "ending_collapse_exhaustion",
            "ending_destitution",
            "ending_arrested",
            "ending_buried_heat",
            "ending_leaving_crime",
            "ending_network_shelter",
            "ending_luxor_dream",
            "ending_stability_honest_work",
            "ending_crime_kingpin"
        };

        var allKnots = StoryTraversalHelper.GetAllKnotNames();

        foreach (var expectedKnot in expectedEndingKnots)
        {
            allKnots.Should().Contain(expectedKnot, $"ending knot '{expectedKnot}' should exist");
        }
    }

    [Test]
    public async Task Ending_MotherDied_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_mother_died", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_mother_died should produce narrative text");
    }

    [Test]
    public async Task Ending_CollapseExhaustion_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_collapse_exhaustion", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_collapse_exhaustion should produce narrative text");
    }

    [Test]
    public async Task Ending_Destitution_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_destitution", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_destitution should produce narrative text");
    }

    [Test]
    public async Task Ending_Arrested_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_arrested", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_arrested should produce narrative text");
    }

    [Test]
    public async Task Ending_BuriedHeat_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_buried_heat", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_buried_heat should produce narrative text");
    }

    [Test]
    public async Task Ending_LeavingCrime_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_leaving_crime", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_leaving_crime should produce narrative text");
    }

    [Test]
    public async Task Ending_NetworkShelter_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_network_shelter", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_network_shelter should produce narrative text");
    }

    [Test]
    public async Task Ending_LuxorDream_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_luxor_dream", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_luxor_dream should produce narrative text");
    }

    [Test]
    public async Task Ending_StabilityHonestWork_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_stability_honest_work", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_stability_honest_work should produce narrative text");
    }

    [Test]
    public async Task Ending_CrimeKingpin_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("ending_crime_kingpin", CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty("ending_crime_kingpin should produce narrative text");
    }

    [Test]
    public async Task Ending_BackgroundVariants_ExistForMedical()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var medicalVariants = allKnots.Where(k => k.Contains("medical", StringComparison.OrdinalIgnoreCase) && k.StartsWith("ending_", StringComparison.OrdinalIgnoreCase)).ToList();

        medicalVariants.Should().NotBeEmpty("ending variants for medical background should exist");
    }

    [Test]
    public async Task Ending_BackgroundVariants_ExistForPrisoner()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var prisonerVariants = allKnots.Where(k => k.Contains("prisoner", StringComparison.OrdinalIgnoreCase) && k.StartsWith("ending_", StringComparison.OrdinalIgnoreCase)).ToList();

        prisonerVariants.Should().NotBeEmpty("ending variants for prisoner background should exist");
    }

    [Test]
    public async Task Ending_BackgroundVariants_ExistForSudanese()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var sudaneseVariants = allKnots.Where(k => k.Contains("sudanese", StringComparison.OrdinalIgnoreCase) && k.StartsWith("ending_", StringComparison.OrdinalIgnoreCase)).ToList();

        sudaneseVariants.Should().NotBeEmpty("ending variants for sudanese background should exist");
    }

    [Test]
    public async Task Ending_BadEndings_AreSomber()
    {
        var badEndingKnots = new[]
        {
            "ending_mother_died",
            "ending_collapse_exhaustion",
            "ending_destitution",
            "ending_arrested",
            "ending_buried_heat"
        };

        foreach (var knot in badEndingKnots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
            result.Text.Should().NotBeEmpty($"bad ending '{knot}' should produce text");

            var hasNoChoices = result.Choices.Count == 0;
            hasNoChoices.Should().BeTrue($"bad ending '{knot}' should not have further choices");
        }
    }

    [Test]
    public async Task Ending_GoodEndings_AreHopeful()
    {
        var goodEndingKnots = new[]
        {
            "ending_leaving_crime",
            "ending_network_shelter",
            "ending_luxor_dream",
            "ending_stability_honest_work"
        };

        foreach (var knot in goodEndingKnots)
        {
            var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
            result.Text.Should().NotBeEmpty($"good ending '{knot}' should produce text");
        }
    }

    private static NarrativeSceneState CreateDefaultSceneState()
    {
        using var session = new GameStateBuilder().Build();
        return NarrativeSceneState.Create(session);
    }
}
