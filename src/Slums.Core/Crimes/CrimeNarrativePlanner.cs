using Slums.Core.Characters;
using Slums.Core.Narrative;
using Slums.Core.Relationships;
using Slums.Core.World;

namespace Slums.Core.Crimes;

public static class CrimeNarrativePlanner
{
    public static NarrativeSceneTrigger? GetFirstSuccessTrigger(IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);

        return NarrativeSignalRules.HasPendingFirstCrimeAftermath(storyFlags)
            ? new NarrativeSceneTrigger(StoryFlags.CrimeFirstSuccess, NarrativeKnots.CrimeFirstSuccess)
            : null;
    }

    public static NarrativeSceneTrigger? GetCrimeWarningTrigger(int policePressure, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);

        return NarrativeSignalRules.HasPendingCrimeWarning(policePressure, storyFlags)
            ? new NarrativeSceneTrigger(StoryFlags.CrimeWarning, NarrativeKnots.CrimeWarning)
            : null;
    }

    public static NarrativeSceneTrigger? GetPrisonerHeatTrigger(BackgroundType backgroundType, IReadOnlySet<string> storyFlags)
    {
        ArgumentNullException.ThrowIfNull(storyFlags);

        return NarrativeSignalRules.HasPendingPrisonerHeat(backgroundType, storyFlags)
            ? new NarrativeSceneTrigger(StoryFlags.BackgroundPrisonerHeatSeen, NarrativeKnots.BackgroundPrisonerHeat)
            : null;
    }

    public static NarrativeSceneTrigger? GetRouteSceneTrigger(CrimeType crimeType, CrimeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return crimeType switch
        {
            CrimeType.MarketFencing => BuildRouteTrigger(
                result,
                StoryFlags.CrimeHananFenceSuccessSeen,
                NarrativeKnots.CrimeHananFenceSuccess,
                StoryFlags.CrimeHananFenceDetectedSeen,
                NarrativeKnots.CrimeHananFenceDetected,
                StoryFlags.CrimeHananFenceFailureSeen,
                NarrativeKnots.CrimeHananFenceFailure),
            CrimeType.DokkiDrop => BuildRouteTrigger(
                result,
                StoryFlags.CrimeYoussefDropSuccessSeen,
                NarrativeKnots.CrimeYoussefDropSuccess,
                StoryFlags.CrimeYoussefDropDetectedSeen,
                NarrativeKnots.CrimeYoussefDropDetected,
                StoryFlags.CrimeYoussefDropFailureSeen,
                NarrativeKnots.CrimeYoussefDropFailure),
            CrimeType.NetworkErrand => BuildRouteTrigger(
                result,
                StoryFlags.CrimeUmmKarimErrandSuccessSeen,
                NarrativeKnots.CrimeUmmKarimErrandSuccess,
                StoryFlags.CrimeUmmKarimErrandDetectedSeen,
                NarrativeKnots.CrimeUmmKarimErrandDetected,
                StoryFlags.CrimeUmmKarimErrandFailureSeen,
                NarrativeKnots.CrimeUmmKarimErrandFailure),
            CrimeType.DepotFareSkim => BuildRouteTrigger(
                result,
                StoryFlags.CrimeSafaaSkimSuccessSeen,
                NarrativeKnots.CrimeSafaaSkimSuccess,
                StoryFlags.CrimeSafaaSkimDetectedSeen,
                NarrativeKnots.CrimeSafaaSkimDetected,
                StoryFlags.CrimeSafaaSkimFailureSeen,
                NarrativeKnots.CrimeSafaaSkimFailure),
            CrimeType.ShubraBundleLift => BuildRouteTrigger(
                result,
                StoryFlags.CrimeImanBundleSuccessSeen,
                NarrativeKnots.CrimeImanBundleSuccess,
                StoryFlags.CrimeImanBundleDetectedSeen,
                NarrativeKnots.CrimeImanBundleDetected,
                StoryFlags.CrimeImanBundleFailureSeen,
                NarrativeKnots.CrimeImanBundleFailure),
            CrimeType.WorkshopContraband => BuildRouteTrigger(
                result,
                StoryFlags.CrimeWorkshopContrabandSuccessSeen,
                NarrativeKnots.CrimeWorkshopContrabandSuccess,
                StoryFlags.CrimeWorkshopContrabandDetectedSeen,
                NarrativeKnots.CrimeWorkshopContrabandDetected,
                StoryFlags.CrimeWorkshopContrabandFailureSeen,
                NarrativeKnots.CrimeWorkshopContrabandFailure),
            CrimeType.BulaqProtectionRacket => BuildRouteTrigger(
                result,
                StoryFlags.CrimeBulaqProtectionSuccessSeen,
                NarrativeKnots.CrimeBulaqProtectionSuccess,
                StoryFlags.CrimeBulaqProtectionDetectedSeen,
                NarrativeKnots.CrimeBulaqProtectionDetected,
                StoryFlags.CrimeBulaqProtectionFailureSeen,
                NarrativeKnots.CrimeBulaqProtectionFailure),
            _ => null
        };
    }

    public static CrimeContactAftermathPlan? GetDetectedContactAftermath(LocationId locationId, RelationshipState relationships, CrimeResult result)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Detected)
        {
            return null;
        }

        if (locationId == LocationId.Market && HasTrustedContact(relationships, NpcId.FenceHanan))
        {
            return new CrimeContactAftermathPlan(
                5,
                "Hanan quietly shifts attention away from your name. The market heat eases a little.",
                new NarrativeSceneTrigger(StoryFlags.CrimeHananCoverSeen, NarrativeKnots.CrimeHananCover),
                12,
                4,
                "Hanan still manages to move a sliver of the loss. The night hurts less than it should have.",
                new NarrativeSceneTrigger(StoryFlags.CrimeHananSalvageSeen, NarrativeKnots.CrimeHananSalvage));
        }

        if (locationId == LocationId.Square && HasTrustedContact(relationships, NpcId.RunnerYoussef))
        {
            return new CrimeContactAftermathPlan(
                7,
                "Youssef tips you off and sends you moving before the wrong questions settle on you.",
                new NarrativeSceneTrigger(StoryFlags.CrimeYoussefTipoffSeen, NarrativeKnots.CrimeYoussefTipoff),
                0,
                6,
                "Youssef gets you clear before panic turns into a worse mistake.",
                new NarrativeSceneTrigger(StoryFlags.CrimeYoussefEscapeSeen, NarrativeKnots.CrimeYoussefEscape));
        }

        if (locationId == LocationId.Depot && HasTrustedContact(relationships, NpcId.DispatcherSafaa))
        {
            return new CrimeContactAftermathPlan(
                5,
                "Safaa reroutes the gossip faster than the depot can pin it to you. The heat slips sideways.",
                new NarrativeSceneTrigger(StoryFlags.CrimeSafaaRerouteSeen, NarrativeKnots.CrimeSafaaReroute),
                8,
                4,
                "Safaa turns a blown move into something survivable and keeps one driver's mouth shut.",
                new NarrativeSceneTrigger(StoryFlags.CrimeSafaaSalvageSeen, NarrativeKnots.CrimeSafaaSalvage));
        }

        if (locationId == LocationId.Laundry && HasTrustedContact(relationships, NpcId.LaundryOwnerIman))
        {
            return new CrimeContactAftermathPlan(
                4,
                "Iman clocks the wrong kind of attention in the lane and sends you out the back before it settles on your face.",
                new NarrativeSceneTrigger(StoryFlags.CrimeImanCoverSeen, NarrativeKnots.CrimeImanCover),
                0,
                5,
                "Iman does not ask questions. She only gets you clear before panic starts showing.",
                new NarrativeSceneTrigger(StoryFlags.CrimeImanExitSeen, NarrativeKnots.CrimeImanExit));
        }

        return null;
    }

    private static bool HasTrustedContact(RelationshipState relationships, NpcId npcId)
    {
        return relationships.GetNpcRelationship(npcId).Trust >= 15;
    }

    private static NarrativeSceneTrigger BuildRouteTrigger(
        CrimeResult result,
        string successFlag,
        string successKnot,
        string detectedSuccessFlag,
        string detectedSuccessKnot,
        string failureFlag,
        string failureKnot)
    {
        if (result.Success && result.Detected)
        {
            return new NarrativeSceneTrigger(detectedSuccessFlag, detectedSuccessKnot);
        }

        if (result.Success)
        {
            return new NarrativeSceneTrigger(successFlag, successKnot);
        }

        return new NarrativeSceneTrigger(failureFlag, failureKnot);
    }
}
