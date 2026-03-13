using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionWorkSnapshot
{
    public int TotalHonestWorkEarnings { get; init; }

    public int HonestShiftsCompleted { get; init; }

    public int LastHonestWorkDay { get; init; }

    public int LastPublicFacingWorkDay { get; init; }

    public static GameSessionWorkSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionWorkSnapshot
        {
            TotalHonestWorkEarnings = gameSession.TotalHonestWorkEarnings,
            HonestShiftsCompleted = gameSession.HonestShiftsCompleted,
            LastHonestWorkDay = gameSession.LastHonestWorkDay,
            LastPublicFacingWorkDay = gameSession.LastPublicFacingWorkDay
        };
    }
}
