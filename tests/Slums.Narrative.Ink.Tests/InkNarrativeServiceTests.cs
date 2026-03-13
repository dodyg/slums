using Ink.Runtime;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Narrative;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Narrative.Ink.Tests;

internal sealed class InkNarrativeServiceTests
{
    private static void StartScene(Slums.Narrative.Ink.InkNarrativeService service, string knotName)
    {
        using var state = new GameSession();
        service.StartScene(knotName, NarrativeSceneState.Create(state));
    }

    [Test]
    public void StartScene_ShouldLoadMedicalIntroText()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        using var state = new GameSession();

        service.StartScene("intro_medical", NarrativeSceneState.Create(state));

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Three years of medical school.");
        service.CurrentChoices.Should().ContainInOrder("Check on her", "Look for work instead");
    }

    [Test]
    public void SelectChoice_ShouldAdvanceMedicalIntroScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        using var state = new GameSession();
        service.StartScene("intro_medical", NarrativeSceneState.Create(state));

        service.SelectChoice(0);

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("You kneel beside her mattress.");
        service.CurrentChoices.Should().ContainInOrder("Use your medical knowledge to help her", "Promise to find the money for a real doctor");
    }

    [Test]
    public void StartScene_ShouldEndScene_WhenKnotDoesNotExist()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        using var state = new GameSession();

        FluentActions.Invoking(() => service.StartScene("missing_knot", NarrativeSceneState.Create(state)))
            .Should()
            .Throw<StoryException>();
    }

    [Test]
    public void NarrativeAssembly_ShouldEmbedTheStoryJson()
    {
        var resourceNames = typeof(Slums.Narrative.Ink.InkNarrativeService).Assembly.GetManifestResourceNames();

        resourceNames.Should().Contain("Slums.Narrative.Ink.Content.main.json");
    }

    [Test]
    public void EndScene_ShouldClearNarrativeState()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        StartScene(service, "intro_medical");

        service.EndScene();

        service.IsSceneActive.Should().BeFalse();
        service.CurrentText.Should().BeNull();
        service.CurrentChoices.Should().BeEmpty();
        service.GetPendingOutcome().Should().BeNull();
    }

    [Test]
    public void RestoreProgress_ShouldRememberLastKnot_AndClearActiveScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "fixer_first_contact");
        service.SelectChoice(0);

        service.RestoreProgress("crime_warning");

        service.IsSceneActive.Should().BeFalse();
        service.CurrentText.Should().BeNull();
        service.CurrentChoices.Should().BeEmpty();
        service.GetPendingOutcome().Should().BeNull();
        service.LastKnot.Should().Be("crime_warning");
    }

    [Test]
    public void StartScene_ShouldLoadInkNpcConversationWithChoices()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "landlord_rent_negotiation");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hajj Mahmoud");
        service.CurrentChoices.Should().ContainInOrder("Answer politely and ask for time", "Answer defiantly");
    }

    [Test]
    public void SelectChoice_ShouldAccumulateOutcome_FromInkTags()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "fixer_first_contact");
        service.SelectChoice(0);

        var outcome = service.GetPendingOutcome();
        outcome.Should().NotBeNull();
        outcome!.NpcTrustTarget.Should().NotBeNull();
        outcome.FactionReputationChange.Should().BeGreaterThan(0);
        outcome.SetFlag.Should().Be("fixer_met");
    }

    [Test]
    public void StartScene_ShouldLoadNewNpcConversationWithChoices()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "nurse_salma");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Nurse Salma");
        service.CurrentChoices.Should().ContainInOrder("Ask about extra shifts", "Ask quietly about cheap medicine for your mother");
    }

    [Test]
    public void StartScene_ShouldLoadCrimeContactConversationWithChoices()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "hanan_fence");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hanan");
        service.CurrentChoices.Should().ContainInOrder("Ask what kind of goods move quietly this week", "Ask for easy money");
    }

    [Test]
    public void StartScene_ShouldLoadCrimeAftermathCoverScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_hanan_cover");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hanan never admits she helped.");
    }

    [Test]
    public void StartScene_ShouldLoadCrimeFailureRescueScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_youssef_escape");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Youssef keeps you moving");
    }

    [Test]
    public void StartScene_ShouldLoadHananRouteScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_hanan_fence_success");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hanan takes the wrapped bundle");
    }

    [Test]
    public void StartScene_ShouldLoadYoussefRouteDetectedScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_youssef_drop_detected");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("The handoff lands");
    }

    [Test]
    public void StartScene_ShouldLoadUmmKarimRouteFailureScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_ummkarim_errand_failure");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Umm Karim does not raise her voice");
    }

    [Test]
    public void StartScene_ShouldLoadSafaaRouteScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_safaa_skim_success");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("depot is chaos anyway");
    }

    [Test]
    public void StartScene_ShouldLoadNpcMemoryVariant()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "nurse_salma_debt");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Salma does not mention the medicine");
    }

    [Test]
    public void StartScene_ShouldLoadNewNarrativeEnhancementNpcVariants()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "fixer_double_life");
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("two stories belong to the same woman");

        StartScene(service, "neighbor_mona_heat");
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("does not start with gossip this time");
    }

    [Test]
    public void StartScene_ShouldLoadNewNpcVariantScenes()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "landlord_rent_broke");
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("week has gone badly");

        StartScene(service, "mariam_pharmacy_urgent");
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("urgency before the details are finished");

        StartScene(service, "safaa_depot_regular");
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("being expected");
    }

    [Test]
    public void StartScene_ShouldLoadDistrictEventScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "event_dokki_checkpoint_sweep");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("checkpoint appears");
    }

    [Test]
    public void StartScene_ShouldLoadNewSpilloverEventScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "event_mother_wrong_money");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("looks at the money longer than she looks at you");
    }

    [Test]
    public void StartScene_ShouldLoadExpandedEndingScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "ending_network_shelter");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("difficult to erase");
    }

    [Test]
    public void StartScene_ShouldLoadAllFormerlyAbruptEndingScenes()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var expectations = new Dictionary<string, string>
        {
            ["ending_destitution"] = "stops offering you choices",
            ["ending_mother_died"] = "room goes quiet",
            ["ending_collapse"] = "not one dramatic blow",
            ["ending_crime_kingpin"] = "better-lit cage"
        };

        foreach (var expectation in expectations)
        {
            StartScene(service, expectation.Key);

            service.IsSceneActive.Should().BeTrue();
            service.CurrentText.Should().Contain(expectation.Value);
        }
    }

    [Test]
    public void StartScene_ShouldLoadEndingVariantScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "ending_network_shelter_salma");

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Salma never lets hardship become abstract");
    }
}
