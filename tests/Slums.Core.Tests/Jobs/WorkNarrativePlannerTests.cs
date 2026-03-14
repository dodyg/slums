using FluentAssertions;
using Slums.Core.Jobs;
using Slums.Core.Narrative;
using TUnit.Core;

namespace Slums.Core.Tests.Jobs;

internal sealed class WorkNarrativePlannerTests
{
    [Test]
    public void GetPublicWorkHeatPlan_ShouldReturnTrigger_ForPublicFacingWorkUnderRecentHeat()
    {
        var job = new JobShift
        {
            Type = JobType.ClinicReception
        };

        var plan = WorkNarrativePlanner.GetPublicWorkHeatPlan(
            currentDay: 6,
            lastCrimeDay: 5,
            policePressure: 60,
            storyFlags: new HashSet<string>(StringComparer.Ordinal),
            job);

        plan.Should().NotBeNull();
        plan!.StressDelta.Should().Be(4);
        plan.EmployerTrustDelta.Should().Be(-2);
        plan.NarrativeTrigger.Should().BeEquivalentTo(new NarrativeSceneTrigger(StoryFlags.EventPublicWorkHeatSeen, NarrativeKnots.EventPublicWorkHeat));
    }
}
