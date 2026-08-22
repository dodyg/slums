using FluentAssertions;
using Slums.Core.Narrative;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class CentralCharacterArcStateTests
{
    [Test]
    public void Decision_ShouldBeTypedToItsCharacterAndPersistable()
    {
        var state = new CentralCharacterArcState();

        state.RecordDecision(CentralCharacterId.NeighborMona, CentralArcDecision.MonaShareRota).Should().BeTrue();
        state.RecordDecision(CentralCharacterId.NeighborMona, CentralArcDecision.SalmaPublishEvidence).Should().BeFalse();

        var restored = new CentralCharacterArcState();
        restored.Restore(
            state.Beats.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value),
            state.Decisions.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.ToString()));

        restored.GetDecision(CentralCharacterId.NeighborMona).Should().Be(CentralArcDecision.MonaShareRota);
        restored.GetBeat(CentralCharacterId.NeighborMona).Should().Be(2);
    }

    [Test]
    public void Planner_ShouldReleaseCentralBeatsInOrder()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);

        CentralCharacterArcPlanner.GetNextTrigger(10, flags)!.KnotName.Should().Be(NarrativeKnots.CentralMotherArc);
        flags.Add("central_mother_arc_seen");
        CentralCharacterArcPlanner.GetNextTrigger(10, flags)!.KnotName.Should().Be(NarrativeKnots.CentralMonaArc);
    }
}
