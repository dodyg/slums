using Slums.Core.Endings;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionRunSnapshot
{
    public Guid RunId { get; init; }
    public bool IsGameOver { get; init; }
    public string? GameOverReason { get; init; }
    public EndingId? EndingId { get; init; }
    public int DaysSurvived { get; init; }
    public string? PendingEndingKnot { get; init; }
    public int UnpaidRentDays { get; init; }
    public int AccumulatedRentDebt { get; init; }
    public bool FirstWarningGiven { get; init; }
    public bool FinalWarningGiven { get; init; }

    public static GameSessionRunSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        
        return new GameSessionRunSnapshot
        {
            RunId = gameSession.RunId,
            IsGameOver = gameSession.IsGameOver,
            GameOverReason = gameSession.GameOverReason,
            EndingId = gameSession.EndingId,
            DaysSurvived = gameSession.DaysSurvived,
            PendingEndingKnot = gameSession.PendingEndingKnot,
            UnpaidRentDays = gameSession.UnpaidRentDays,
            AccumulatedRentDebt = gameSession.AccumulatedRentDebt,
            FirstWarningGiven = gameSession.FirstWarningGiven,
            FinalWarningGiven = gameSession.FinalWarningGiven
        };
    }
}
