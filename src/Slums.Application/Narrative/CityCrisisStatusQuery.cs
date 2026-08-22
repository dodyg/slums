using Slums.Core.Narrative;
using Slums.Core.State;

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

public static class CityCrisisStatusQuery
{
    public static CityCrisisStatus Execute(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        var crisis = gameSession.CityCrisis;
        var obligation = crisis.Phase switch
        {
            CityCrisisPhase.IrregularClassification => "Collect meter and pump evidence before the appeal window closes.",
            CityCrisisPhase.Appeal => "Choose who will carry the appeal and what the neighborhood can afford to risk.",
            CityCrisisPhase.HeatEmergency => "Protect water, cooling, and your mother's medication through the next hot nights.",
            CityCrisisPhase.Commitment => "Commit a response before day 30.",
            CityCrisisPhase.Resolved => "The cooperative is living with the consequences of the response.",
            _ => "Listen for the next cooperative update."
        };

        if (crisis.PendingCallbackDecision != CityCrisisDecision.None && !crisis.CallbackQueued)
        {
            obligation = $"A consequence of the {crisis.PendingCallbackDecision} decision is due on day {crisis.CallbackDueDay}.";
        }

        return new CityCrisisStatus(
            crisis.Phase,
            crisis.BeatIndex,
            crisis.EvidenceCollected,
            crisis.ResourcesCommitted,
            crisis.CooperativeCondition,
            crisis.Decision,
            crisis.Resolution,
            crisis.PendingCallbackDecision,
            crisis.CallbackDueDay,
            obligation);
    }
}
