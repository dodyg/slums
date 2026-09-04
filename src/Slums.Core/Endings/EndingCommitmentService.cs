using Slums.Core.Diagnostics;
using Slums.Core.State;

namespace Slums.Core.Endings;

internal static class EndingCommitmentService
{
    internal static IReadOnlyList<EndingId> GetAvailableEndingChoices(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return EndingService.GetAvailableEndings(session);
    }

    internal static bool TryChooseEnding(GameSession session, EndingId endingId)
    {
        ArgumentNullException.ThrowIfNull(session);

        var before = session.CaptureStats();
        if (session.PendingEndingId is not null || session.IsGameOver || !EndingService.GetAvailableEndings(session).Contains(endingId))
        {
            session.RaiseEvent("That long-term path is not ready yet.");
            session.RecordMutation(MutationCategories.GuardRejected, "ChooseEnding", before, session.CaptureStats(), $"Ending {endingId} is not available");
            return false;
        }

        session.PendingEndingId = endingId;
        session.PendingEndingKnot = EndingKnotCatalog.Commitment;
        session.RecordMutation(MutationCategories.EndingTriggered, "ChooseEnding", before, session.CaptureStats(), $"Ending commitment opened: {endingId}");
        return true;
    }

    internal static void CommitEnding(GameSession session, EndingId endingId, string sacrifice)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(sacrifice);

        var before = session.CaptureStats();
        if (session.PendingEndingId != endingId || session.IsGameOver)
        {
            throw new InvalidOperationException($"Ending '{endingId}' is not the pending commitment.");
        }

        session.EndingId = endingId;
        session.FinalSacrifice = sacrifice;
        session.PendingEndingId = null;
        session.IsGameOver = true;
        session.GameOverReason = EndingService.GetMessage(endingId);
        session.PendingEndingKnot = EndingService.GetInkKnot(session, endingId);
        session.RecordMutation(MutationCategories.EndingTriggered, "CommitEnding", before, session.CaptureStats(), $"Ending committed: {endingId}; sacrifice: {sacrifice}");
    }

    internal static bool TryTakePendingEndingKnot(GameSession session, out string knotName)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!string.IsNullOrWhiteSpace(session.PendingEndingKnot))
        {
            knotName = session.PendingEndingKnot;
            session.PendingEndingKnot = null;
            return true;
        }

        knotName = string.Empty;
        return false;
    }

    internal static void CheckGameOverConditions(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var ending = EndingService.CheckFailureEndings(session);
        if (ending is null)
        {
            return;
        }

        var before = session.CaptureStats();
        session.EndingId = ending;
        session.IsGameOver = true;
        session.GameOverReason = EndingService.GetMessage(ending.Value);
        session.PendingEndingKnot = EndingService.GetInkKnot(session, ending.Value);
        session.RecordMutation(MutationCategories.EndingTriggered, "CheckGameOverConditions", before, session.CaptureStats(), $"Ending triggered: {ending}");
    }
}
