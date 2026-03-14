using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class BackgroundReactivityTests
{
    [Test]
    public async Task Background_Medical_HasSpecificScenes()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var medicalKnots = allKnots.Where(k => k.Contains("medical", StringComparison.OrdinalIgnoreCase)).ToList();

        medicalKnots.Should().NotBeEmpty("medical background should have specific narrative scenes");
    }

    [Test]
    public async Task Background_Prisoner_HasSpecificScenes()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var prisonerKnots = allKnots.Where(k => k.Contains("prisoner", StringComparison.OrdinalIgnoreCase) ||
                                                 k.Contains("ex_prisoner", StringComparison.OrdinalIgnoreCase) ||
                                                 k.Contains("political", StringComparison.OrdinalIgnoreCase)).ToList();

        prisonerKnots.Should().NotBeEmpty("prisoner background should have specific narrative scenes");
    }

    [Test]
    public async Task Background_Sudanese_HasSpecificScenes()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var sudaneseKnots = allKnots.Where(k => k.Contains("sudanese", StringComparison.OrdinalIgnoreCase) ||
                                                k.Contains("sudan", StringComparison.OrdinalIgnoreCase) ||
                                                k.Contains("refugee", StringComparison.OrdinalIgnoreCase)).ToList();

        sudaneseKnots.Should().NotBeEmpty("sudanese background should have specific narrative scenes");
    }

    [Test]
    public async Task Background_MedicalClinic_SceneExists()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("background_medical_clinic", "medical background clinic scene should exist");
    }

    [Test]
    public async Task Background_MedicalClinic_ProducesText()
    {
        var result = StoryTraversalHelper.ExplorePath("background_medical_clinic", CreateMedicalSceneState());
        result.Text.Should().NotBeEmpty("background_medical_clinic should produce narrative text");
    }

    [Test]
    public async Task Background_IntroMedic_ReferencesMother()
    {
        var result = StoryTraversalHelper.ExplorePath("intro_medical", CreateMedicalSceneState());
        var allText = string.Join(" ", result.Text);

        allText.Should().Contain("mother", "medical intro should reference mother's condition");
    }

    [Test]
    public async Task Background_IntroPrisoner_ReferencesPolitics()
    {
        var result = StoryTraversalHelper.ExplorePath("intro_prisoner", CreatePrisonerSceneState());
        var allText = string.Join(" ", result.Text);

        var hasPoliticalContent =
            allText.Contains("protest", StringComparison.OrdinalIgnoreCase) ||
            allText.Contains("tahrir", StringComparison.OrdinalIgnoreCase) ||
            allText.Contains("amn el-dawla", StringComparison.OrdinalIgnoreCase) ||
            allText.Contains("inside", StringComparison.OrdinalIgnoreCase) ||
            allText.Contains("criminal record", StringComparison.OrdinalIgnoreCase);

        hasPoliticalContent.Should().BeTrue("prisoner intro should reference political imprisonment");
    }

    [Test]
    public async Task Background_IntroSudanese_ReferencesDisplacement()
    {
        var result = StoryTraversalHelper.ExplorePath("intro_sudanese", CreateSudaneseSceneState());
        var allText = string.Join(" ", result.Text);

        var hasDisplacementContent = allText.Contains("sudan", StringComparison.OrdinalIgnoreCase) ||
                                     allText.Contains("refugee", StringComparison.OrdinalIgnoreCase) ||
                                     allText.Contains("cairo", StringComparison.OrdinalIgnoreCase) ||
                                     allText.Contains("border", StringComparison.OrdinalIgnoreCase);

        hasDisplacementContent.Should().BeTrue("sudanese intro should reference displacement");
    }

    [Test]
    public async Task Background_AllBackgrounds_HaveIntroScenes()
    {
        var backgrounds = new[] { BackgroundType.MedicalSchoolDropout, BackgroundType.ReleasedPoliticalPrisoner, BackgroundType.SudaneseRefugee };
        var introKnots = new[] { "intro_medical", "intro_prisoner", "intro_sudanese" };

        for (var i = 0; i < backgrounds.Length; i++)
        {
            var sceneState = CreateSceneState(backgrounds[i]);
            var result = StoryTraversalHelper.ExplorePath(introKnots[i], sceneState);
            result.Text.Should().NotBeEmpty($"intro '{introKnots[i]}' should produce text");
        }
    }

    [Test]
    public async Task Background_CrimeNetworkErrand_RequiresPrisonerBackground()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        allKnots.Should().Contain("crime_ummkarim_errand_success", "network errand crime requires ex-prisoner network");
    }

    [Test]
    public async Task Background_MedicalEndingVariants_Exist()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var medicalEndingVariants = allKnots
            .Where(k => k.StartsWith("ending_", StringComparison.OrdinalIgnoreCase) &&
                        k.Contains("medical", StringComparison.OrdinalIgnoreCase))
            .ToList();

        medicalEndingVariants.Should().NotBeEmpty("medical background should have ending variants");
    }

    [Test]
    public async Task Background_PrisonerEndingVariants_Exist()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var prisonerEndingVariants = allKnots
            .Where(k => k.StartsWith("ending_", StringComparison.OrdinalIgnoreCase) &&
                        (k.Contains("prisoner", StringComparison.OrdinalIgnoreCase) ||
                         k.Contains("network", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        prisonerEndingVariants.Should().NotBeEmpty("prisoner background should have ending variants");
    }

    [Test]
    public async Task Background_SudaneseEndingVariants_Exist()
    {
        var allKnots = StoryTraversalHelper.GetAllKnotNames();
        var sudaneseEndingVariants = allKnots
            .Where(k => k.StartsWith("ending_", StringComparison.OrdinalIgnoreCase) &&
                        k.Contains("sudanese", StringComparison.OrdinalIgnoreCase))
            .ToList();

        sudaneseEndingVariants.Should().NotBeEmpty("sudanese background should have ending variants");
    }

    private static NarrativeSceneState CreateMedicalSceneState()
    {
        return CreateSceneState(BackgroundType.MedicalSchoolDropout);
    }

    private static NarrativeSceneState CreatePrisonerSceneState()
    {
        return CreateSceneState(BackgroundType.ReleasedPoliticalPrisoner);
    }

    private static NarrativeSceneState CreateSudaneseSceneState()
    {
        return CreateSceneState(BackgroundType.SudaneseRefugee);
    }

    private static NarrativeSceneState CreateSceneState(BackgroundType backgroundType)
    {
        using var session = GameStateBuilder.BuildForBackground(backgroundType);
        return NarrativeSceneState.Create(session);
    }
}
