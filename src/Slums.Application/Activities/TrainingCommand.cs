using Slums.Core.State;
using Slums.Core.Training;

namespace Slums.Application.Activities;

public sealed class TrainingCommand
{
    public bool Execute(GameSession gameSession, TrainingActivity activity)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentNullException.ThrowIfNull(activity);
        return gameSession.TryPerformTraining(activity);
    }
}
