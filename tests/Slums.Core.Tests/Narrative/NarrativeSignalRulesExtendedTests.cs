using Slums.Core.Characters;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Narrative;

internal sealed class NarrativeSignalRulesExtendedTests
{
    [Test]
    public async Task HasPendingArrestCloseCall_ReturnsTrue_WhenPressure90AndNotSeen()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingArrestCloseCall(90, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingArrestCloseCall_ReturnsFalse_WhenPressureBelow90()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingArrestCloseCall(89, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingArrestCloseCall_ReturnsFalse_WhenAlreadySeen()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal) { StoryFlags.EventArrestCloseCallSeen };
        var result = NarrativeSignalRules.HasPendingArrestCloseCall(95, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingHonestMilestone_ReturnsTrue_When10ShiftsAndNotSeen()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingHonestMilestone(10, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingHonestMilestone_ReturnsFalse_WhenBelow10Shifts()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingHonestMilestone(9, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingHonestMilestone_ReturnsFalse_WhenAlreadySeen()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal) { StoryFlags.EventHonestMilestoneSeen };
        var result = NarrativeSignalRules.HasPendingHonestMilestone(15, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingCommunityAftermath_ReturnsTrue_WhenAttended2AndNotSeen()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingCommunityAftermath(true, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingCommunityAftermath_ReturnsFalse_WhenNotAttended()
    {
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingCommunityAftermath(false, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingEmbarrassmentRecovery_ReturnsTrue_WhenEmbarrassedAndTrustRecovered()
    {
        var session = new GameSession();
        session.Relationships.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
        session.Relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 7, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingEmbarrassmentRecovery(session.Relationships, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingEmbarrassmentRecovery_ReturnsFalse_WhenTrustTooLow()
    {
        var session = new GameSession();
        session.Relationships.SetEmbarrassedState(NpcId.WorkshopBossAbuSamir, true);
        session.Relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 3, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingEmbarrassmentRecovery(session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingEmbarrassmentRecovery_ReturnsFalse_WhenNotEmbarrassed()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.WorkshopBossAbuSamir, 10, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingEmbarrassmentRecovery(session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingPrisonerKhalid_ReturnsTrue_WhenPrisonerAndLowTrust()
    {
        var session = new GameSession();
        session.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, -5, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingPrisonerKhalid(
            BackgroundType.ReleasedPoliticalPrisoner, session.Relationships, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingPrisonerKhalid_ReturnsFalse_WhenTrustPositive()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, 5, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingPrisonerKhalid(
            BackgroundType.ReleasedPoliticalPrisoner, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingPrisonerKhalid_ReturnsFalse_WhenNotPrisoner()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.OfficerKhalid, -5, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingPrisonerKhalid(
            BackgroundType.MedicalSchoolDropout, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingSudaneseMariam_ReturnsTrue_WhenSudaneseAndTrustHigh()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 12, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingSudaneseMariam(
            BackgroundType.SudaneseRefugee, session.Relationships, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingSudaneseMariam_ReturnsFalse_WhenTrustTooLow()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 8, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingSudaneseMariam(
            BackgroundType.SudaneseRefugee, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingSudaneseMariam_ReturnsFalse_WhenNotSudanese()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.PharmacistMariam, 15, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingSudaneseMariam(
            BackgroundType.MedicalSchoolDropout, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingYoussefEmbedded_ReturnsTrue_When3CrimesAndHighTrust()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 18, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingYoussefEmbedded(3, session.Relationships, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingYoussefEmbedded_ReturnsFalse_WhenBelow3Crimes()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 18, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingYoussefEmbedded(2, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingYoussefEmbedded_ReturnsFalse_WhenTrustTooLow()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 10, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingYoussefEmbedded(3, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingNadiaSuspicion_ReturnsTrue_WhenDoubleLifeAndHighTrust()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 12, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingNadiaSuspicion(3, 1, session.Relationships, flags);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasPendingNadiaSuspicion_ReturnsFalse_WhenNoCrimes()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 12, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingNadiaSuspicion(3, 0, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingNadiaSuspicion_ReturnsFalse_WhenNoHonestWork()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 12, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingNadiaSuspicion(2, 1, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasPendingNadiaSuspicion_ReturnsFalse_WhenTrustTooLow()
    {
        var session = new GameSession();
        session.Relationships.SetNpcRelationship(NpcId.CafeOwnerNadia, 8, session.Clock.Day);

        var flags = new HashSet<string>(StringComparer.Ordinal);
        var result = NarrativeSignalRules.HasPendingNadiaSuspicion(3, 1, session.Relationships, flags);
        await Assert.That(result).IsFalse();
    }
}
