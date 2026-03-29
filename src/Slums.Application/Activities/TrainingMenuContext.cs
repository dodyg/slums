using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Training;

namespace Slums.Application.Activities;

public sealed record TrainingMenuContext(
    Slums.Core.Characters.PlayerCharacter Player,
    IReadOnlyList<TrainingActivity> Activities,
    string? LocationName,
    int Money,
    int Energy,
    int Hour,
    IReadOnlyDictionary<SkillId, bool> TrainedToday)
{
    public static TrainingMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var location = gameSession.World.GetCurrentLocation();
        var activities = gameSession.GetAvailableTrainingActivities();

        return new TrainingMenuContext(
            gameSession.Player,
            activities,
            location?.Name,
            gameSession.Player.Stats.Money,
            gameSession.Player.Stats.Energy,
            gameSession.Clock.Hour,
            gameSession.TrainedSkillsToday);
    }
}
