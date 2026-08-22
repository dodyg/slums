using Ink.Runtime;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Narrative.Ink.Tests;

internal sealed class InkNarrativeServiceTests
{
    private static void StartScene(Slums.Narrative.Ink.InkNarrativeService service, string knotName)
    {
        var state = new GameSession();
        service.StartScene(knotName, NarrativeSceneState.Create(state));
    }

    [Test]
    public void StartScene_ShouldLoadMedicalIntroText()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameSession();

        service.StartScene("intro_medical", NarrativeSceneState.Create(state));

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Cairo, 2030.");
        service.CurrentText.Should().Contain("translation app");
        service.CurrentText.Should().Contain("Three years of medical school.");
        service.CurrentChoices.Should().ContainInOrder("Check on her", "Look for work instead");
    }

    [Test]
    public void StartScene_ShouldBranchOnSynchronizedMoney()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var lowMoneyState = new GameSession();
        lowMoneyState.Player.Stats.SetMoney(20);

        service.StartScene("intro_done", NarrativeSceneState.Create(lowMoneyState));

        service.CurrentText.Should().Contain("wallet is already thin");

        var comfortableMoneyState = new GameSession();
        comfortableMoneyState.Player.Stats.SetMoney(80);
        service.StartScene("intro_done", NarrativeSceneState.Create(comfortableMoneyState));

        service.CurrentText.Should().Contain("count the notes twice");
        service.CurrentText.Should().NotContain("wallet is already thin");
    }

    [Test]
    public void SelectChoice_ShouldAdvanceMedicalIntroScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameSession();
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
        var state = new GameSession();

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
        outcome!.Effects.OfType<NpcTrustEffect>().Where(effect => effect.Change > 0).Should().NotBeEmpty();
        outcome.Effects.OfType<FactionReputationEffect>().Where(effect => effect.Change > 0).Should().NotBeEmpty();
        outcome.SetFlag.Should().Be("fixer_met");
    }

    [Test]
    public void SelectChoice_ShouldPreserveMultipleFlagsFromOneScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "crime_first_success");
        service.SelectChoice(2);

        var outcome = service.GetPendingOutcome();
        outcome.Should().NotBeNull();
        outcome!.SetFlags.Should().ContainInOrder("first_crime_reflected", "crime_consequence_seen");
    }

    [Test]
    public void EventRamadanIftar_ShouldApplyTrustToBothNpcs()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);

        StartScene(service, "event_ramadan_iftar");
        service.SelectChoice(0);

        var outcome = service.GetPendingOutcome();
        outcome.Should().NotBeNull();

        outcome!.Effects.OfType<NpcTrustEffect>().Where(effect => effect.Npc == NpcId.NeighborMona && effect.Change == 2)
            .Should().NotBeEmpty("the communal iftar should raise Neighbor Mona's trust");
        outcome.Effects.OfType<NpcTrustEffect>().Where(effect => effect.Npc == NpcId.LandlordHajjMahmoud && effect.Change == 1)
            .Should().NotBeEmpty("the communal iftar should raise Landlord Hajj Mahmoud's trust");

        // Applying the outcome must change both NPCs, not just the last tag's target.
        var session = new GameSession();
        session.ApplyOutcome(outcome);

        session.Relationships.GetNpcRelationship(NpcId.NeighborMona).Trust.Should().Be(2);
        session.Relationships.GetNpcRelationship(NpcId.LandlordHajjMahmoud).Trust.Should().Be(1);
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
    public void StartScene_ShouldLoadActiveFormerlyAbruptEndingScenes()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var expectations = new Dictionary<string, string>
        {
            ["ending_destitution"] = "stops offering you choices",
            ["ending_mother_died"] = "room goes quiet",
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
