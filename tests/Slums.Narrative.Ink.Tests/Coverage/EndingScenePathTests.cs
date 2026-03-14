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
            EndingKnotCatalog.MotherDied,
            EndingKnotCatalog.CollapseFromExhaustion,
            EndingKnotCatalog.Destitution,
            EndingKnotCatalog.Arrested,
            EndingKnotCatalog.Eviction,
            EndingKnotCatalog.BuriedByHeat,
            EndingKnotCatalog.LeavingCrime,
            EndingKnotCatalog.NetworkShelter,
            EndingKnotCatalog.QuitTheLuxorDream,
            EndingKnotCatalog.StabilityHonestWork,
            EndingKnotCatalog.CrimeKingpin
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
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.MotherDied, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.MotherDied} should produce narrative text");
    }

    [Test]
    public async Task Ending_CollapseExhaustion_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.CollapseFromExhaustion, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.CollapseFromExhaustion} should produce narrative text");
    }

    [Test]
    public async Task Ending_Destitution_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.Destitution, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.Destitution} should produce narrative text");
    }

    [Test]
    public async Task Ending_Arrested_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.Arrested, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.Arrested} should produce narrative text");
    }

    [Test]
    public async Task Ending_Eviction_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.Eviction, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.Eviction} should produce narrative text");
    }

    [Test]
    public async Task Ending_BuriedHeat_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.BuriedByHeat, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.BuriedByHeat} should produce narrative text");
    }

    [Test]
    public async Task Ending_LeavingCrime_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.LeavingCrime, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.LeavingCrime} should produce narrative text");
    }

    [Test]
    public async Task Ending_NetworkShelter_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.NetworkShelter, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.NetworkShelter} should produce narrative text");
    }

    [Test]
    public async Task Ending_LuxorDream_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.QuitTheLuxorDream, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.QuitTheLuxorDream} should produce narrative text");
    }

    [Test]
    public async Task Ending_StabilityHonestWork_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.StabilityHonestWork, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.StabilityHonestWork} should produce narrative text");
    }

    [Test]
    public async Task Ending_CrimeKingpin_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath(EndingKnotCatalog.CrimeKingpin, CreateDefaultSceneState());
        result.Text.Should().NotBeEmpty($"{EndingKnotCatalog.CrimeKingpin} should produce narrative text");
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
            EndingKnotCatalog.MotherDied,
            EndingKnotCatalog.CollapseFromExhaustion,
            EndingKnotCatalog.Destitution,
            EndingKnotCatalog.Arrested,
            EndingKnotCatalog.Eviction,
            EndingKnotCatalog.BuriedByHeat
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
            EndingKnotCatalog.LeavingCrime,
            EndingKnotCatalog.NetworkShelter,
            EndingKnotCatalog.QuitTheLuxorDream,
            EndingKnotCatalog.StabilityHonestWork
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
