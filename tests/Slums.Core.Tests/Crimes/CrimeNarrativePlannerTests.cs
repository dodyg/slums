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

    [Test]
    public void GetPoliceEncounterTrigger_ShouldFireOnlyOnFirstHeatThresholdCrossing()
    {
        var flags = new HashSet<string>();

        var trigger = CrimeNarrativePlanner.GetPoliceEncounterTrigger(DistrictId.Imbaba, 59, 60, flags);

        trigger.Should().BeEquivalentTo(new NarrativeSceneTrigger(
            StoryFlags.GetPoliceEncounterSeenFlag(DistrictId.Imbaba),
            NarrativeKnots.CrimePoliceEncounter));
        CrimeNarrativePlanner.GetPoliceEncounterTrigger(DistrictId.Imbaba, 60, 65, flags).Should().BeNull();
        CrimeNarrativePlanner.GetPoliceEncounterTrigger(
            DistrictId.Imbaba,
            59,
            60,
            new HashSet<string> { StoryFlags.GetPoliceEncounterSeenFlag(DistrictId.Imbaba) }).Should().BeNull();
    }

    [Test]
    public void GetGangRetaliationTrigger_ShouldRequireDetectedCrimeInImbabaWithFactionStanding()
    {
        var relationships = new RelationshipState();
        relationships.SetFactionStanding(FactionId.ImbabaCrew, 10);

        CrimeNarrativePlanner.GetGangRetaliationTrigger(false, DistrictId.Imbaba, FactionId.ImbabaCrew, relationships, new HashSet<string>())
            .Should().BeNull();
        CrimeNarrativePlanner.GetGangRetaliationTrigger(true, DistrictId.Dokki, FactionId.ImbabaCrew, relationships, new HashSet<string>())
            .Should().BeNull();
        CrimeNarrativePlanner.GetGangRetaliationTrigger(true, DistrictId.Imbaba, FactionId.ImbabaCrew, relationships, new HashSet<string>())
            .Should().BeEquivalentTo(new NarrativeSceneTrigger(StoryFlags.CrimeGangRetaliation, NarrativeKnots.CrimeGangRetaliation));
    }
}
