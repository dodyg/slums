namespace Slums.Core.Narrative;

/// <summary>Persistent state for the shared rooftop water-and-power cooperative crisis.</summary>
public sealed class CityCrisisState
{
    public int BeatIndex { get; private set; }
    public int EvidenceCollected { get; private set; }
    public int ResourcesCommitted { get; private set; }
    public int CooperativeCondition { get; private set; } = 70;
    public CityCrisisDecision Decision { get; private set; }
    public CityCrisisResolution Resolution { get; private set; }
    public int DecisionDay { get; private set; }
    public int CallbackDueDay { get; private set; }
    public CityCrisisDecision PendingCallbackDecision { get; private set; }
    public bool CallbackQueued { get; private set; }

    public CityCrisisPhase Phase => Resolution != CityCrisisResolution.Unresolved
        ? CityCrisisPhase.Resolved
        : BeatIndex switch
        {
            0 => CityCrisisPhase.NotDiscovered,
            1 => CityCrisisPhase.CooperativeReview,
            2 => CityCrisisPhase.IrregularClassification,
            3 => CityCrisisPhase.Appeal,
            4 => CityCrisisPhase.HeatEmergency,
            _ => CityCrisisPhase.Commitment
        };

    public bool HasDiscoveredEvidence => EvidenceCollected > 0;

    public void MarkBeatQueued()
    {
        BeatIndex = Math.Min(6, BeatIndex + 1);
    }

    public bool CollectEvidence(int amount = 1)
    {
        if (amount <= 0 || Phase is CityCrisisPhase.NotDiscovered or CityCrisisPhase.Resolved)
        {
            return false;
        }

        EvidenceCollected = Math.Min(5, EvidenceCollected + amount);
        CooperativeCondition = Math.Min(100, CooperativeCondition + amount * 2);
        return true;
    }

    public bool CommitResources(int amount)
    {
        if (amount <= 0 || Phase is CityCrisisPhase.NotDiscovered or CityCrisisPhase.Resolved)
        {
            return false;
        }

        ResourcesCommitted = Math.Min(100, ResourcesCommitted + amount);
        CooperativeCondition = Math.Min(100, CooperativeCondition + amount);
        return true;
    }

    public bool ChooseDecision(CityCrisisDecision decision, int currentDay = 0)
    {
        if (decision == CityCrisisDecision.None || Phase is CityCrisisPhase.NotDiscovered or CityCrisisPhase.Resolved)
        {
            return false;
        }

        Decision = decision;
        DecisionDay = Math.Max(0, currentDay);
        CallbackDueDay = Math.Max(0, currentDay + 3);
        PendingCallbackDecision = decision;
        CallbackQueued = false;
        return true;
    }

    /// <summary>Returns whether the selected route has a callback ready to enter.</summary>
    public bool HasDueCallback(int currentDay)
    {
        return PendingCallbackDecision != CityCrisisDecision.None
            && !CallbackQueued
            && currentDay >= CallbackDueDay
            && Resolution == CityCrisisResolution.Unresolved;
    }

    public void MarkCallbackQueued()
    {
        CallbackQueued = true;
    }

    public bool Resolve(CityCrisisResolution resolution)
    {
        if (resolution == CityCrisisResolution.Unresolved || Decision == CityCrisisDecision.None)
        {
            return false;
        }

        Resolution = resolution;
        CooperativeCondition = resolution switch
        {
            CityCrisisResolution.CooperativeProtected => Math.Max(CooperativeCondition, 70),
            CityCrisisResolution.SharedEmergencyPlan => Math.Max(CooperativeCondition, 55),
            CityCrisisResolution.AccessRestricted => Math.Min(CooperativeCondition, 35),
            CityCrisisResolution.DivertedAndExposed => Math.Min(CooperativeCondition, 25),
            _ => CooperativeCondition
        };
        return true;
    }

    public void Restore(
        int beatIndex,
        int evidenceCollected,
        int resourcesCommitted,
        int cooperativeCondition,
        CityCrisisDecision decision,
        CityCrisisResolution resolution,
        int decisionDay = 0,
        int callbackDueDay = 0,
        CityCrisisDecision pendingCallbackDecision = CityCrisisDecision.None,
        bool callbackQueued = false)
    {
        BeatIndex = Math.Clamp(beatIndex, 0, 6);
        EvidenceCollected = Math.Clamp(evidenceCollected, 0, 5);
        ResourcesCommitted = Math.Clamp(resourcesCommitted, 0, 100);
        CooperativeCondition = Math.Clamp(cooperativeCondition, 0, 100);
        Decision = decision;
        Resolution = resolution;
        DecisionDay = Math.Max(0, decisionDay);
        CallbackDueDay = Math.Max(0, callbackDueDay);
        PendingCallbackDecision = pendingCallbackDecision;
        CallbackQueued = callbackQueued;
    }
}
