using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Narrative;
using Slums.Core.State;
using TUnit;

namespace Slums.Application.Tests.Narrative;

internal sealed class CityCrisisCommandTests
{
    [Test]
    public void Commands_ShouldMutateTheCanonicalSessionState()
    {
        var session = new GameSession();
        session.RestoreCityCrisisState(2, 0, 0, 70, CityCrisisDecision.None, CityCrisisResolution.Unresolved);

        CollectCrisisEvidenceCommand.Execute(session, 2).Should().BeTrue();
        CommitCrisisResourcesCommand.Execute(session, 5).Should().BeTrue();
        ChooseCrisisDecisionCommand.Execute(session, CityCrisisDecision.EvidenceAppeal).Should().BeTrue();
        session.ResolveCityCrisis(CityCrisisResolution.CooperativeProtected).Should().BeTrue();

        var status = CityCrisisStatusQuery.Execute(session);
        status.Phase.Should().Be(CityCrisisPhase.Resolved);
        status.EvidenceCollected.Should().Be(2);
        status.ResourcesCommitted.Should().Be(5);
        status.Decision.Should().Be(CityCrisisDecision.EvidenceAppeal);
    }

    [Test]
    public void NarrativeOutcome_ShouldApplyCrisisEffectsAndPolicePressure()
    {
        var session = new GameSession();
        session.RestoreCityCrisisState(2, 0, 0, 70, CityCrisisDecision.None, CityCrisisResolution.Unresolved);
        var initialPressure = session.PolicePressure;

        ApplyNarrativeOutcomeCommand.Execute(session, NarrativeKnots.CrisisCommitment, new NarrativeOutcome
        {
            Effects =
            [
                new CrisisEvidenceEffect(1),
                new CrisisResourcesEffect(3),
                new CrisisDecisionEffect(CityCrisisDecision.Diversion),
                new PolicePressureEffect(5)
            ]
        });

        session.CityCrisis.EvidenceCollected.Should().Be(1);
        session.CityCrisis.ResourcesCommitted.Should().Be(3);
        session.CityCrisis.Decision.Should().Be(CityCrisisDecision.Diversion);
        session.PolicePressure.Should().Be(initialPressure + 5);
    }
}
