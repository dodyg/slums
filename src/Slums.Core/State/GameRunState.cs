using Slums.Core.Endings;

namespace Slums.Core.State;

internal sealed class GameRunState
{
    public Guid RunId { get; set; } = Guid.NewGuid();

    public bool IsGameOver { get; set; }

    public string? GameOverReason { get; set; }

    public EndingId? EndingId { get; set; }

    public int DaysSurvived { get; set; }

    public string? PendingEndingKnot { get; set; }
}
