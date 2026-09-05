using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Training;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class TrainingMenuQueryTests
{
    [Test]
    public void GetStatuses_ShouldReturnAllAvailableActivities()
    {
        var state = new GameSession();
        var context = TrainingMenuContext.Create(state);
        var query = new TrainingMenuQuery();

        var statuses = query.GetStatuses(context);

        statuses.Should().NotBeEmpty();
    }

    [Test]
    public void GetStatuses_ShouldShowActivityUnavailable_WhenWrongTime()
    {
        var state = new GameSession();
        state.Clock.SetTime(1, 10, 0);
        var context = TrainingMenuContext.Create(state);
        var query = new TrainingMenuQuery();

        var statuses = query.GetStatuses(context);

        foreach (var status in statuses)
        {
            status.RightTime.Should().BeFalse();
            status.CanTrain.Should().BeFalse();
        }
    }

    [Test]
    public void GetStatuses_ShouldShowActivityAvailable_WhenEveningAndEnoughResources()
    {
        var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 19, 0);
        var context = TrainingMenuContext.Create(state);
        var query = new TrainingMenuQuery();

        var statuses = query.GetStatuses(context);

        var exercise = statuses.FirstOrDefault(s => s.Activity.Type == TrainingActivityType.RooftopExercise);
        exercise.Should().NotBeNull();
        exercise!.CanTrain.Should().BeTrue();
        exercise.UnavailabilityReason.Should().BeNull();
    }

    [Test]
    public void GetStatuses_ShouldShowActivityUnavailable_WhenEnergyTooLow()
    {
        var state = new GameSession();
        state.Player.Stats.SetEnergy(5);
        state.Clock.SetTime(1, 19, 0);
        var context = TrainingMenuContext.Create(state);
        var query = new TrainingMenuQuery();

        var statuses = query.GetStatuses(context);

        foreach (var status in statuses)
        {
            status.HasEnergy.Should().BeFalse();
            status.CanTrain.Should().BeFalse();
        }
    }

    [Test]
    public void GetStatuses_ShouldShowUnavailabilityReason_WhenWrongTime()
    {
        var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 10, 0);
        var context = TrainingMenuContext.Create(state);
        var query = new TrainingMenuQuery();

        var statuses = query.GetStatuses(context);

        foreach (var status in statuses)
        {
            status.UnavailabilityReason.Should().Contain("evening");
        }
    }

    [Test]
    public void GetStatuses_ShouldReflectTrainedTodayState()
    {
        var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 18, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        state.TryPerformTraining(exercise);

        var context = TrainingMenuContext.Create(state);
        var query = new TrainingMenuQuery();

        context.TrainedToday.Should().ContainKey(Slums.Core.Skills.SkillId.Physical);
        context.Activities.Should().Contain(a => a.Type == TrainingActivityType.RooftopExercise);
        query.GetStatuses(context).Single(status => status.Activity.Type == TrainingActivityType.RooftopExercise)
            .UnavailabilityReason.Should().Contain("Already trained");
    }
}
