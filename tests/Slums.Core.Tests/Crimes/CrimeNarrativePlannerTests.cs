using FluentAssertions;
using Slums.Core.Crimes;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Crimes;

internal sealed class CrimeNarrativePlannerTests
{
    [Test]
    public void GetRouteSceneTrigger_ShouldUseDetectedSuccessVariant_ForDokkiDrop()
    {
        var trigger = CrimeNarrativePlanner.GetRouteSceneTrigger(
            CrimeType.DokkiDrop,
            new CrimeResult
            {
                Success = true,
                Detected = true
            });

        trigger.Should().BeEquivalentTo(new NarrativeSceneTrigger(StoryFlags.CrimeYoussefDropDetectedSeen, NarrativeKnots.CrimeYoussefDropDetected));
    }

    [Test]
    public void GetDetectedContactAftermath_ShouldReturnTrustedSquarePlan_WhenYoussefCanHelp()
    {
        var relationships = new RelationshipState();
        relationships.SetNpcRelationship(NpcId.RunnerYoussef, 20, 1);

        var plan = CrimeNarrativePlanner.GetDetectedContactAftermath(
            LocationId.Square,
            relationships,
            new CrimeResult
            {
                Success = false,
                Detected = true
            });

        plan.Should().NotBeNull();
        plan!.PolicePressureReduction.Should().Be(7);
        plan.HeatTrigger.Should().BeEquivalentTo(new NarrativeSceneTrigger(StoryFlags.CrimeYoussefTipoffSeen, NarrativeKnots.CrimeYoussefTipoff));
        plan.FailureTrigger.Should().BeEquivalentTo(new NarrativeSceneTrigger(StoryFlags.CrimeYoussefEscapeSeen, NarrativeKnots.CrimeYoussefEscape));
    }
}
