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
}
