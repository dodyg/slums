using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Narrative.Ink.Tests;

internal sealed class InkNarrativeServiceTests
{
    [Test]
    public void StartScene_ShouldLoadMedicalIntroText()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameState();

        service.StartScene("intro_medical", state);

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Three years of medical school.");
        service.CurrentChoices.Should().ContainInOrder("Check on her", "Look for work instead");
    }

    [Test]
    public void SelectChoice_ShouldAdvanceMedicalIntroScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameState();
        service.StartScene("intro_medical", state);

        service.SelectChoice(0);

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("You kneel beside her mattress.");
        service.CurrentChoices.Should().ContainInOrder("Use your medical knowledge to help her", "Promise to find the money for a real doctor");
    }

    [Test]
    public void StartScene_ShouldEndScene_WhenKnotDoesNotExist()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("missing_knot", new GameState());

        service.IsSceneActive.Should().BeFalse();
        service.CurrentText.Should().BeNull();
        service.CurrentChoices.Should().BeEmpty();
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
        service.StartScene("intro_medical", new GameState());

        service.EndScene();

        service.IsSceneActive.Should().BeFalse();
        service.CurrentText.Should().BeNull();
        service.CurrentChoices.Should().BeEmpty();
        service.GetPendingOutcome().Should().BeNull();
    }

    [Test]
    public void StartScene_ShouldLoadInkNpcConversationWithChoices()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("landlord_rent_negotiation", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hajj Mahmoud");
        service.CurrentChoices.Should().ContainInOrder("Answer politely and ask for time", "Answer defiantly");
    }

    [Test]
    public void SelectChoice_ShouldAccumulateOutcome_FromInkTags()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("fixer_first_contact", new GameState());
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

        service.StartScene("nurse_salma", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Nurse Salma");
        service.CurrentChoices.Should().ContainInOrder("Ask about extra shifts", "Ask quietly about cheap medicine for your mother");
    }

    [Test]
    public void StartScene_ShouldLoadCrimeContactConversationWithChoices()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("hanan_fence", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hanan");
        service.CurrentChoices.Should().ContainInOrder("Ask what kind of goods move quietly this week", "Ask for easy money");
    }

    [Test]
    public void StartScene_ShouldLoadCrimeAftermathCoverScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("crime_hanan_cover", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hanan never admits she helped.");
    }

    [Test]
    public void StartScene_ShouldLoadCrimeFailureRescueScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("crime_youssef_escape", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Youssef keeps you moving");
    }

    [Test]
    public void StartScene_ShouldLoadHananRouteScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("crime_hanan_fence_success", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hanan takes the wrapped bundle");
    }

    [Test]
    public void StartScene_ShouldLoadYoussefRouteDetectedScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("crime_youssef_drop_detected", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("The handoff lands");
    }

    [Test]
    public void StartScene_ShouldLoadUmmKarimRouteFailureScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("crime_ummkarim_errand_failure", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Umm Karim does not raise her voice");
    }

    [Test]
    public void StartScene_ShouldLoadSafaaRouteScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("crime_safaa_skim_success", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("depot is chaos anyway");
    }

    [Test]
    public void StartScene_ShouldLoadNpcMemoryVariant()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("nurse_salma_debt", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Salma does not mention the medicine");
    }

    [Test]
    public void StartScene_ShouldLoadNewNarrativeEnhancementNpcVariants()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("fixer_double_life", new GameState());
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("two stories belong to the same woman");

        service.StartScene("neighbor_mona_heat", new GameState());
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("does not start with gossip this time");
    }

    [Test]
    public void StartScene_ShouldLoadNewNpcVariantScenes()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("landlord_rent_broke", new GameState());
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("week has gone badly");

        service.StartScene("mariam_pharmacy_urgent", new GameState());
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("urgency before the details are finished");

        service.StartScene("safaa_depot_regular", new GameState());
        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("being expected");
    }

    [Test]
    public void StartScene_ShouldLoadDistrictEventScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("event_dokki_checkpoint_sweep", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("checkpoint appears");
    }

    [Test]
    public void StartScene_ShouldLoadNewSpilloverEventScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("event_mother_wrong_money", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("looks at the money longer than she looks at you");
    }

    [Test]
    public void StartScene_ShouldLoadExpandedEndingScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("ending_network_shelter", new GameState());

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
            service.StartScene(expectation.Key, new GameState());

            service.IsSceneActive.Should().BeTrue();
            service.CurrentText.Should().Contain(expectation.Value);
        }
    }

    [Test]
    public void StartScene_ShouldLoadEndingVariantScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        service.StartScene("ending_network_shelter_salma", new GameState());

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Salma never lets hardship become abstract");
    }
}
