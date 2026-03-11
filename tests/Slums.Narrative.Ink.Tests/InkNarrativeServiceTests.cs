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
    public void SelectChoice_ShouldEndScene_WhenCompiledStoryCannotAdvanceChoice()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameState();
        service.StartScene("intro_medical", state);

        service.SelectChoice(0);

        service.IsSceneActive.Should().BeFalse();
        service.CurrentText.Should().BeNull();
        service.CurrentChoices.Should().BeEmpty();
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
}
