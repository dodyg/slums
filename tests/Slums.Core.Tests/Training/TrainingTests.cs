using Slums.Core.Characters;
using Slums.Core.Entertainment;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Training;
using Slums.Core.World;

namespace Slums.Core.Tests.Training;

internal sealed class TrainingTests
{
    [Test]
    public async Task TrainingRegistry_ShouldReturnAllActivities()
    {
        var activities = TrainingRegistry.AllActivities;

        await Assert.That(activities.Count).IsEqualTo(4);
    }

    [Test]
    public async Task TrainingRegistry_ShouldHaveCorrectActivityTypes()
    {
        var activities = TrainingRegistry.AllActivities;

        await Assert.That(activities.Any(a => a.Type == TrainingActivityType.StudyMedical)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == TrainingActivityType.PracticePersuasion)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == TrainingActivityType.StreetDice)).IsTrue();
        await Assert.That(activities.Any(a => a.Type == TrainingActivityType.RooftopExercise)).IsTrue();
    }

    [Test]
    public async Task GameSession_GetAvailableTrainingActivities_ShouldReturnExerciseAtHome()
    {
        using var state = new GameSession();

        var activities = state.GetAvailableTrainingActivities();

        await Assert.That(activities.Any(a => a.Type == TrainingActivityType.RooftopExercise)).IsTrue();
    }

    [Test]
    public async Task GameSession_GetAvailableTrainingActivities_ShouldRequireNpcTrust()
    {
        using var state = new GameSession();

        var medicalTraining = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.StudyMedical);
        await Assert.That(medicalTraining).IsNotNull();

        var available = state.GetAvailableTrainingActivities();
        await Assert.That(available.Any(a => a.Type == TrainingActivityType.StudyMedical)).IsFalse();
    }

    [Test]
    public async Task GameSession_GetAvailableTrainingActivities_ShouldShowMedicalWithEnoughTrust()
    {
        using var state = new GameSession();
        state.Relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);

        var available = state.GetAvailableTrainingActivities();
        await Assert.That(available.Any(a => a.Type == TrainingActivityType.StudyMedical)).IsTrue();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldSucceedAndGrantSkill()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        await Assert.That(exercise).IsNotNull();

        var oldLevel = state.Player.Skills.GetLevel(SkillId.Physical);
        var result = state.TryPerformTraining(exercise);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Skills.GetLevel(SkillId.Physical)).IsEqualTo(oldLevel + 1);
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldFailWhenEnergyTooLow()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(5);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var result = state.TryPerformTraining(exercise);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldFailWhenMoneyTooLow()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Player.Stats.SetMoney(0);
        state.Clock.SetTime(1, 19, 0);

        var dice = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.StreetDice);
        var available = state.GetAvailableTrainingActivities();
        await Assert.That(available.Any(a => a.Type == TrainingActivityType.StreetDice)).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldFailOutsideEveningHours()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 10, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var result = state.TryPerformTraining(exercise);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldFailWhenSkillAtCap()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Player.Skills.SetLevel(SkillId.Physical, 10);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var result = state.TryPerformTraining(exercise);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldFailWhenAlreadyTrainedToday()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var first = state.TryPerformTraining(exercise);
        await Assert.That(first).IsTrue();

        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 20, 0);
        var second = state.TryPerformTraining(exercise);
        await Assert.That(second).IsFalse();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldFailWhenNotAtHome()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Market);
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var result = state.TryPerformTraining(exercise);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GameSession_EndDay_ShouldResetTrainingTracker()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Player.Stats.SetMoney(1000);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var trained = state.TryPerformTraining(exercise);
        await Assert.That(trained).IsTrue();
        await Assert.That(state.TrainedSkillsToday).IsNotEmpty();

        state.EndDay();

        await Assert.That(state.TrainedSkillsToday).IsEmpty();
    }

    [Test]
    public async Task GameSession_TryPerformTraining_BackgroundMedicalDropout_ShouldReduceStressOnStudyMedical()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        state.Relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);
        state.Player.Stats.SetEnergy(100);
        state.Player.Stats.SetStress(50);
        state.Clock.SetTime(1, 19, 0);

        var study = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.StudyMedical);
        var result = state.TryPerformTraining(study);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Stress).IsLessThan(50);
    }

    [Test]
    public async Task GameSession_TryPerformTraining_BackgroundRefugee_ShouldReduceEnergyOnExercise()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 18, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var energyBefore = state.Player.Stats.Energy;
        var result = state.TryPerformTraining(exercise);

        await Assert.That(result).IsTrue();
        var expectedEnergy = energyBefore - (exercise.EnergyCost - 3);
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(expectedEnergy);
    }

    [Test]
    public async Task GameSession_TryPerformTraining_BackgroundPrisoner_ShouldReduceEnergyOnStreetDice()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        state.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 10, 0);
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 19, 0);

        var dice = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.StreetDice);
        var energyBefore = state.Player.Stats.Energy;
        var result = state.TryPerformTraining(dice);

        await Assert.That(result).IsTrue();
        var expectedEnergy = energyBefore - (dice.EnergyCost - 3);
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(expectedEnergy);
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldRecordMutation()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 19, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        state.TryPerformTraining(exercise);

        await Assert.That(state.Mutations.Count).IsGreaterThan(0);
        var mutation = state.Mutations[^1];
        await Assert.That(mutation.Category).IsEqualTo("Training");
    }

    [Test]
    public async Task GameSession_TryPerformTraining_ShouldDeductTimeAndEnergy()
    {
        using var state = new GameSession();
        state.Player.Stats.SetEnergy(100);
        state.Clock.SetTime(1, 18, 0);

        var exercise = TrainingRegistry.AllActivities.First(a => a.Type == TrainingActivityType.RooftopExercise);
        var energyBefore = state.Player.Stats.Energy;

        state.TryPerformTraining(exercise);

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(energyBefore - exercise.EnergyCost);
        await Assert.That(state.Clock.Hour).IsGreaterThanOrEqualTo(18);
    }
}
