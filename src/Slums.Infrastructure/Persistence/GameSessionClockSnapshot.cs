using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionClockSnapshot
{
    public int Day { get; init; }

    public int Hour { get; init; }

    public int Minute { get; init; }

    public static GameSessionClockSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionClockSnapshot
        {
            Day = gameSession.Clock.Day,
            Hour = gameSession.Clock.Hour,
            Minute = gameSession.Clock.Minute
        };
    }
}
