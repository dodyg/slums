using Slums.Core.State;
using Slums.Core.Training;

namespace Slums.Application.Activities;

public sealed class TrainingCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, TrainingActivity activity)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(activity);
        return gameSession.TryPerformTraining(activity);
    }
}
