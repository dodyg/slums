using Slums.Core.Crimes;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class CrimeCommand
{
    public CrimeResult Execute(GameSession gameSession, CrimeAttempt attempt, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(attempt);
        return gameSession.CommitCrime(attempt, random);
    }
}
