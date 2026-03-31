using Slums.Core.Characters;
using Slums.Core.Relationships;

namespace Slums.Core.Narrative;

public static class NarrativeSignalRules
{
    public static bool HasPendingFirstCrimeAftermath(IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        return !storyFlags.Contains(StoryFlags.CrimeFirstSuccess);
    }

    public static bool HasPendingCrimeWarning(int policePressure, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);

        return policePressure >= 80 &&
               !storyFlags.Contains(StoryFlags.CrimeWarning);
    }

    public static bool HasPendingClinicFirstVisit(IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        return !storyFlags.Contains(StoryFlags.MotherClinicFirstVisit);
    }

    public static bool HasPendingMedicalClinicReflection(PlayerCharacter player, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(storyFlags);

        return player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
               !storyFlags.Contains(StoryFlags.BackgroundMedicalClinicSeen);
    }

    public static bool HasPendingMotherWrongMoney(PlayerCharacter player, int totalCrimeEarnings, int crimesCommitted, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(storyFlags);

        return totalCrimeEarnings >= 150 &&
               crimesCommitted >= 2 &&
               player.Household.MotherHealth < 65 &&
               !storyFlags.Contains(StoryFlags.EventMotherWrongMoneySeen);
    }

    public static bool HasPendingNeighborWatch(int policePressure, RelationshipState relationships, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);

        return policePressure >= 60 &&
               relationships.GetNpcRelationship(NpcId.NeighborMona).Trust >= 15 &&
               !storyFlags.Contains(StoryFlags.EventNeighborWatchSeen);
    }

    public static bool HasPendingPublicWorkHeat(int currentDay, int lastCrimeDay, int policePressure)
    {
        return lastCrimeDay > 0 &&
               currentDay - lastCrimeDay <= 1 &&
               policePressure >= 60;
    }

    public static bool HasPendingSalmaMedicineHelp(PlayerCharacter player, RelationshipState relationships)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relationships);

        return player.BackgroundType == BackgroundType.MedicalSchoolDropout &&
               relationships.GetNpcRelationship(NpcId.NurseSalma).Trust >= 12 &&
               player.Household.MotherHealth < 65;
    }

    public static bool HasPendingPrisonerHeat(BackgroundType backgroundType, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);

        return backgroundType == BackgroundType.ReleasedPoliticalPrisoner &&
               !storyFlags.Contains(StoryFlags.BackgroundPrisonerHeatSeen);
    }

    public static bool HasPendingSudaneseSolidarity(BackgroundType backgroundType, string eventId, IReadOnlySet<string> storyFlags)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventId);
        ArgumentNullException.ThrowIfNull(storyFlags);

        return backgroundType == BackgroundType.SudaneseRefugee &&
               string.Equals(eventId, "NeighborhoodSolidarity", StringComparison.Ordinal) &&
               !storyFlags.Contains(StoryFlags.BackgroundSudaneseSolidaritySeen);
    }

    public static bool HasPendingArrestCloseCall(int policePressure, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        return policePressure >= 90 &&
               !storyFlags.Contains(StoryFlags.EventArrestCloseCallSeen);
    }

    public static bool HasPendingHonestMilestone(int honestShiftsCompleted, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        return honestShiftsCompleted >= 10 &&
               !storyFlags.Contains(StoryFlags.EventHonestMilestoneSeen);
    }

    public static bool HasPendingCommunityAftermath(bool attendedSolidarityEvent, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);
        return attendedSolidarityEvent &&
               !storyFlags.Contains(StoryFlags.EventCommunityAftermathSeen);
    }

    public static bool HasPendingEmbarrassmentRecovery(RelationshipState relationships, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);
        var abuSamir = relationships.GetNpcRelationship(NpcId.WorkshopBossAbuSamir);
        return abuSamir.WasEmbarrassed &&
               abuSamir.Trust >= 5 &&
               !storyFlags.Contains(StoryFlags.EventEmbarrassmentRecoverySeen);
    }

    public static bool HasPendingPrisonerKhalid(BackgroundType backgroundType, RelationshipState relationships, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);
        return backgroundType == BackgroundType.ReleasedPoliticalPrisoner &&
               relationships.GetNpcRelationship(NpcId.OfficerKhalid).Trust <= 0 &&
               !storyFlags.Contains(StoryFlags.BackgroundPrisonerKhalidSeen);
    }

    public static bool HasPendingSudaneseMariam(BackgroundType backgroundType, RelationshipState relationships, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);
        return backgroundType == BackgroundType.SudaneseRefugee &&
               relationships.GetNpcRelationship(NpcId.PharmacistMariam).Trust >= 10 &&
               !storyFlags.Contains(StoryFlags.BackgroundSudaneseMariamSeen);
    }

    public static bool HasPendingYoussefEmbedded(int crimesCommitted, RelationshipState relationships, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);
        return crimesCommitted >= 3 &&
               relationships.GetNpcRelationship(NpcId.RunnerYoussef).Trust >= 15 &&
               !storyFlags.Contains(StoryFlags.EventYoussefEmbeddedSeen);
    }

    public static bool HasPendingNadiaSuspicion(int honestShiftsCompleted, int crimesCommitted, RelationshipState relationships, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(storyFlags);
        return honestShiftsCompleted >= 3 &&
               crimesCommitted > 0 &&
               relationships.GetNpcRelationship(NpcId.CafeOwnerNadia).Trust >= 10 &&
               !storyFlags.Contains(StoryFlags.EventNadiaSuspicionSeen);
    }
}
