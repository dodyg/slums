using FluentAssertions;
using Slums.Core.Narrative;
using TUnit;

namespace Slums.Core.Tests.Narrative;

internal sealed class CityCrisisStateTests
{
    [Test]
    public void State_ShouldAdvanceThroughTheScheduledBeats()
    {
        var state = new CityCrisisState();

        CityCrisisPlanner.GetNextBeat(1, state)!.KnotName.Should().Be(NarrativeKnots.CrisisIntroduction);
        state.MarkBeatQueued();
        CityCrisisPlanner.GetNextBeat(5, state).Should().BeNull();
        CityCrisisPlanner.GetNextBeat(6, state)!.KnotName.Should().Be(NarrativeKnots.CrisisClassification);
    }

    [Test]
    public void State_ShouldApplyEvidenceDecisionAndResolutionConsequences()
    {
        var state = new CityCrisisState();
        state.Restore(2, 0, 0, 70, CityCrisisDecision.None, CityCrisisResolution.Unresolved);

        state.CollectEvidence(2).Should().BeTrue();
        state.ChooseDecision(CityCrisisDecision.MutualAid).Should().BeTrue();
        state.CommitResources(4).Should().BeTrue();
        state.Resolve(CityCrisisResolution.SharedEmergencyPlan).Should().BeTrue();

        state.Phase.Should().Be(CityCrisisPhase.Resolved);
        state.EvidenceCollected.Should().Be(2);
        state.ResourcesCommitted.Should().Be(4);
        state.CooperativeCondition.Should().Be(78);
    }

    [Test]
    public void State_ShouldRejectInvalidOrRepeatedTerminalActions()
    {
        var state = new CityCrisisState();

        state.CollectEvidence(1).Should().BeFalse();
        state.ChooseDecision(CityCrisisDecision.None).Should().BeFalse();
        state.Resolve(CityCrisisResolution.AccessRestricted).Should().BeFalse();
    }
}
