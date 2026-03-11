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
}
