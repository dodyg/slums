using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Narrative.Ink.Tests;

internal sealed class FallbackNarrativeTests
{
    [Test]
    public void StartScene_ShouldUseFallbackScene_ForNpcConversation()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameState();

        service.StartScene("landlord_rent_negotiation", state);

        service.IsSceneActive.Should().BeTrue();
        service.CurrentText.Should().Contain("Hajj Mahmoud");
        service.CurrentChoices.Should().HaveCount(2);
    }

    [Test]
    public void SelectChoice_ShouldAccumulateOutcome_ForFallbackScene()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var state = new GameState();

        service.StartScene("fixer_first_contact", state);
        service.SelectChoice(0);

        var outcome = service.GetPendingOutcome();
        outcome.Should().NotBeNull();
        outcome!.NpcTrustTarget.Should().NotBeNull();
        outcome.FactionReputationChange.Should().BeGreaterThan(0);
    }
}