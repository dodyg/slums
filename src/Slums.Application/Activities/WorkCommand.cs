using Slums.Core.Jobs;
using Slums.Core.State;

namespace Slums.Application.Activities;

public sealed class WorkCommand
{
#pragma warning disable CA1822
    public JobResult Execute(GameSession gameSession, JobShift job, Random? random = null)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(job);
        return gameSession.WorkJob(job, random);
    }
}
