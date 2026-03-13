using Slums.Core.State;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionCrimeSnapshot
{
    public int PolicePressure { get; init; }

    public int TotalCrimeEarnings { get; init; }

    public int CrimesCommitted { get; init; }

    public int LastCrimeDay { get; init; }

    public bool HasCrimeCommittedToday { get; init; }

    public static GameSessionCrimeSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionCrimeSnapshot
        {
            PolicePressure = gameSession.PolicePressure,
            TotalCrimeEarnings = gameSession.TotalCrimeEarnings,
            CrimesCommitted = gameSession.CrimesCommitted,
            LastCrimeDay = gameSession.LastCrimeDay,
            HasCrimeCommittedToday = gameSession.HasCrimeCommittedToday
        };
    }
}
