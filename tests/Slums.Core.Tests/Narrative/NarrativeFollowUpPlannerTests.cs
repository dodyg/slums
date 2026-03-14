using FluentAssertions;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class NarrativeFollowUpPlannerTests
{
    [Test]
    public void GetEndOfDayTriggers_ShouldReturnBothKnownCrimeFollowUps_WhenSignalsArePending()
    {
        using var session = new GameSession();
        session.Player.Household.SetMotherHealth(50);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 18, session.Clock.Day);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: true,
            session.Player,
            totalCrimeEarnings: 150,
            crimesCommitted: 2,
            policePressure: 60,
            session.Relationships,
            storyFlags: new HashSet<string>(StringComparer.Ordinal));

        triggers.Should().ContainEquivalentOf(new NarrativeSceneTrigger(StoryFlags.EventMotherWrongMoneySeen, NarrativeKnots.EventMotherWrongMoney));
        triggers.Should().ContainEquivalentOf(new NarrativeSceneTrigger(StoryFlags.EventNeighborWatchSeen, NarrativeKnots.EventNeighborWatch));
    }
}
