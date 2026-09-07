using Slums.Core.Jobs;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class WorkCommand
{
    public JobResult Execute(GameSession gameSession, JobShift job, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(job);
        return gameSession.WorkJob(job, random);
    }
}
