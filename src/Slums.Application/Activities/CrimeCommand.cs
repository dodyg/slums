using Slums.Core.Crimes;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class CrimeCommand
{
#pragma warning disable CA1822
    public CrimeResult Execute(GameSession gameSession, CrimeAttempt attempt, Random? random = null)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(attempt);
        return gameSession.CommitCrime(attempt, random);
    }
}
