using Slums.Core.Narrative;

namespace Slums.Application.Narrative;

public sealed record CityCrisisStatus(
    CityCrisisPhase Phase,
    int BeatIndex,
    int EvidenceCollected,
    int ResourcesCommitted,
    int CooperativeCondition,
    CityCrisisDecision Decision,
    CityCrisisResolution Resolution,
    CityCrisisDecision PendingCallbackDecision,
    int CallbackDueDay,
    string ImmediateObligation);
