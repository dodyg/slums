using Slums.Core.Characters;
using Slums.Core.Community;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class NarrativeFollowUpPlannerExtendedTests
{
    [Test]
    public async Task GetEndOfDayTriggers_FiresArrestCloseCall_WhenPressureHigh()
    {
        using var session = new GameSession();
        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: false,
            session.Player,
            totalCrimeEarnings: 0,
            crimesCommitted: 0,
            policePressure: 92,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.EventArrestCloseCall);
    }

    [Test]
    public async Task GetEndOfDayTriggers_DoesNotFireArrestCloseCall_WhenPressureLow()
    {
        using var session = new GameSession();
        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: false,
            session.Player,
            totalCrimeEarnings: 0,
            crimesCommitted: 0,
            policePressure: 50,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).DoesNotContain(NarrativeKnots.EventArrestCloseCall);
    }

    [Test]
    public async Task GetEndOfDayTriggers_FiresPrisonerKhalid_WhenPrisonerAndLowTrust()
    {
        using var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, -5, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: false,
            session.Player,
            totalCrimeEarnings: 0,
            crimesCommitted: 0,
            policePressure: 30,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.BackgroundPrisonerKhalid);
    }

    [Test]
    public async Task GetEndOfDayTriggers_FiresSudaneseMariam_WhenSudaneseAndHighTrust()
    {
        using var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        session.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 12, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: false,
            session.Player,
            totalCrimeEarnings: 0,
            crimesCommitted: 0,
            policePressure: 30,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.BackgroundSudaneseMariam);
    }

    [Test]
    public async Task GetEndOfDayTriggers_FiresYoussefEmbedded_WhenCrimesAndTrustHigh()
    {
        using var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 18, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: false,
            session.Player,
            totalCrimeEarnings: 0,
            crimesCommitted: 3,
            policePressure: 30,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.EventYoussefEmbedded);
    }

    [Test]
    public async Task GetEndOfDayTriggers_FiresMultipleTriggers_WhenConditionsMet()
    {
        using var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, -5, session.Clock.Day);
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 18, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetEndOfDayTriggers(
            crimeCommittedToday: false,
            session.Player,
            totalCrimeEarnings: 0,
            crimesCommitted: 3,
            policePressure: 92,
            session.Relationships,
            flags);

        await Assert.That(triggers.Count).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task GetWorkFollowUpTriggers_FiresHonestMilestone_When10Shifts()
    {
        using var session = new GameSession();
        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetWorkFollowUpTriggers(
            honestShiftsCompleted: 10,
            crimesCommitted: 0,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.EventHonestMilestone);
    }

    [Test]
    public async Task GetWorkFollowUpTriggers_DoesNotFireHonestMilestone_WhenBelow10()
    {
        using var session = new GameSession();
        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetWorkFollowUpTriggers(
            honestShiftsCompleted: 9,
            crimesCommitted: 0,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).DoesNotContain(NarrativeKnots.EventHonestMilestone);
    }

    [Test]
    public async Task GetWorkFollowUpTriggers_FiresEmbarrassmentRecovery_WhenTrustRecovered()
    {
        using var session = new GameSession();
        session.Relationships.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
        session.Relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 7, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetWorkFollowUpTriggers(
            honestShiftsCompleted: 5,
            crimesCommitted: 0,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.EventEmbarrassmentRecovery);
    }

    [Test]
    public async Task GetWorkFollowUpTriggers_FiresNadiaSuspicion_WhenDoubleLife()
    {
        using var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 12, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);

        var triggers = NarrativeFollowUpPlanner.GetWorkFollowUpTriggers(
            honestShiftsCompleted: 3,
            crimesCommitted: 1,
            session.Relationships,
            flags);

        var knotNames = triggers.Select(t => t.KnotName).ToList();
        await Assert.That(knotNames).Contains(NarrativeKnots.EventNadiaSuspicion);
    }

    [Test]
    public async Task GetCommunityAftermathTrigger_ReturnsTrigger_WhenAttended2()
    {
        using var session = new GameSession();
        session.EventAttendance.TotalAttended = 2;

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var trigger = NarrativeFollowUpPlanner.GetCommunityAftermathTrigger(session.EventAttendance, flags);

        await Assert.That(trigger).IsNotNull();
        await Assert.That(trigger!.KnotName).IsEqualTo(NarrativeKnots.EventCommunityAftermath);
    }

    [Test]
    public async Task GetCommunityAftermathTrigger_ReturnsNull_WhenNotAttendedEnough()
    {
        using var session = new GameSession();
        session.EventAttendance.TotalAttended = 1;

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var trigger = NarrativeFollowUpPlanner.GetCommunityAftermathTrigger(session.EventAttendance, flags);

        await Assert.That(trigger).IsNull();
    }
}
