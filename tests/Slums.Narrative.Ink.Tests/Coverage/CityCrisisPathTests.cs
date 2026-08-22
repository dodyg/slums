using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Slums.Application.Narrative;
using Slums.Core.Narrative;
using Slums.Core.State;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class CityCrisisPathTests
{
    [Test]
    public void CrisisClassification_ShouldEmitTypedEvidenceEffect()
    {
        var service = new Slums.Narrative.Ink.InkNarrativeService(NullLogger<Slums.Narrative.Ink.InkNarrativeService>.Instance);
        var session = new GameSession();
        session.RestoreCityCrisisState(2, 0, 0, 70, CityCrisisDecision.None, CityCrisisResolution.Unresolved);

        service.StartScene(NarrativeKnots.CrisisClassification, NarrativeSceneState.Create(session));
        service.SelectChoice(0);

        service.GetPendingOutcome()!.Effects.Should().ContainSingle(effect => effect is CrisisEvidenceEffect);
    }
}
