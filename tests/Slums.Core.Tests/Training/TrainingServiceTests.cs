using Slums.Core.State;
using Slums.Core.Skills;
using Slums.Core.Training;
using TUnit;

namespace Slums.Core.Tests.Training;

internal sealed class TrainingServiceTests
{
    [Test]
    public async Task GetAvailable_ShouldRespectTheSessionDailyTracker()
    {
        var session = new GameSession();
        session.Clock.SetTime(1, 19, 0);
        var exercise = TrainingRegistry.AllActivities.Single(static activity => activity.Type == TrainingActivityType.RooftopExercise);

        var before = TrainingService.GetAvailable(session);
        TrainingService.Perform(session, exercise);
        var after = TrainingService.GetAvailable(session);

        await Assert.That(before).Contains(exercise);
        await Assert.That(after).DoesNotContain(exercise);
    }

    [Test]
    public async Task Restore_ShouldHydrateTheExistingTrackerWithoutReplacingIt()
    {
        var session = new GameSession();
        var tracker = session.TrainedSkillsToday;

        TrainingService.Restore(session, new Dictionary<SkillId, bool> { [SkillId.Physical] = true });

        await Assert.That(session.TrainedSkillsToday).IsSameReferenceAs(tracker);
        await Assert.That(session.TrainedSkillsToday[SkillId.Physical]).IsTrue();
    }
}
