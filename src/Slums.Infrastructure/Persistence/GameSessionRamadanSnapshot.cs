using Slums.Core.Calendar;
using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionRamadanSnapshot
{
    public bool IsActive { get; init; }
    public bool PlayerIsFasting { get; init; }
    public int DaysFasting { get; init; }
    public int DaysRemaining { get; init; }

    public static GameSessionRamadanSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionRamadanSnapshot
        {
            IsActive = gameSession.RamadanState.IsActive,
            PlayerIsFasting = gameSession.RamadanState.PlayerIsFasting,
            DaysFasting = gameSession.RamadanState.DaysFasting,
            DaysRemaining = gameSession.RamadanState.DaysRemaining
        };
    }

    public RamadanState Restore()
    {
        return new RamadanState
        {
            IsActive = IsActive,
            PlayerIsFasting = PlayerIsFasting,
            DaysFasting = DaysFasting,
            DaysRemaining = DaysRemaining
        };
    }
}
