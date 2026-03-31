using Slums.Core.Characters;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class NutritionStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeNutritionDefaults()
    {
        var state = new NutritionState();

        await Assert.That(state.Satiety).IsEqualTo(75);
        await Assert.That(state.DaysUndereating).IsEqualTo(0);
        await Assert.That(state.LastMealQuality).IsEqualTo(MealQuality.None);
        await Assert.That(state.AteToday).IsFalse();
    }

    [Test]
    public async Task Eat_Scraps_ShouldIncreaseSatietyAndMarkMeal()
    {
        var state = new NutritionState();
        state.SetSatiety(40);

        state.Eat(MealQuality.Scraps);

        await Assert.That(state.Satiety).IsEqualTo(50);
        await Assert.That(state.LastMealQuality).IsEqualTo(MealQuality.Scraps);
        await Assert.That(state.AteToday).IsTrue();
    }

    [Test]
    public async Task Eat_BasicMeal_ShouldIncreaseSatietyAndMarkMeal()
    {
        var state = new NutritionState();
        state.SetSatiety(40);

        state.Eat(MealQuality.Basic);

        await Assert.That(state.Satiety).IsEqualTo(62);
        await Assert.That(state.LastMealQuality).IsEqualTo(MealQuality.Basic);
        await Assert.That(state.AteToday).IsTrue();
    }

    [Test]
    public async Task Eat_HotMeal_ShouldClampSatietyTo100()
    {
        var state = new NutritionState();
        state.SetSatiety(90);

        state.Eat(MealQuality.HotMeal);

        await Assert.That(state.Satiety).IsEqualTo(100);
        await Assert.That(state.LastMealQuality).IsEqualTo(MealQuality.HotMeal);
    }

    [Test]
    public async Task ResolveDay_WithoutFood_ShouldIncreaseDaysUndereating()
    {
        var state = new NutritionState();

        var result = state.ResolveDay();

        await Assert.That(state.DaysUndereating).IsEqualTo(1);
        await Assert.That(result.EnergyDelta).IsEqualTo(-12);
        await Assert.That(result.HealthDelta).IsEqualTo(0);
        await Assert.That(result.StressDelta).IsEqualTo(6);
        await Assert.That(state.Satiety).IsEqualTo(60);
    }

    [Test]
    public async Task ResolveDay_WithScraps_ShouldStillCountAsUndereating()
    {
        var state = new NutritionState();
        state.SetSatiety(40);
        state.Eat(MealQuality.Scraps);

        var result = state.ResolveDay();

        await Assert.That(state.DaysUndereating).IsEqualTo(1);
        await Assert.That(result.EnergyDelta).IsEqualTo(-5);
        await Assert.That(result.HealthDelta).IsEqualTo(0);
        await Assert.That(result.StressDelta).IsEqualTo(2);
        await Assert.That(state.Satiety).IsEqualTo(35);
    }

    [Test]
    public async Task ResolveDay_WithBasicMeal_ShouldResetDaysUndereating()
    {
        var state = new NutritionState();
        state.SetDaysUndereating(2);
        state.Eat(MealQuality.Basic);

        _ = state.ResolveDay();

        await Assert.That(state.DaysUndereating).IsEqualTo(0);
    }

    [Test]
    public async Task ResolveDay_WithHotMeal_ShouldReduceStress()
    {
        var state = new NutritionState();
        state.Eat(MealQuality.HotMeal);

        var result = state.ResolveDay();

        await Assert.That(result.EnergyDelta).IsEqualTo(0);
        await Assert.That(result.HealthDelta).IsEqualTo(0);
        await Assert.That(result.StressDelta).IsEqualTo(-3);
    }

    [Test]
    public async Task ResolveDay_AfterTwoDaysUndereating_ShouldApplyHealthPenalty()
    {
        var state = new NutritionState();
        state.SetDaysUndereating(1);

        var result = state.ResolveDay();

        await Assert.That(state.DaysUndereating).IsEqualTo(2);
        await Assert.That(result.HealthDelta).IsEqualTo(-5);
    }

    [Test]
    public async Task BeginNewDay_ShouldClearMealFlag()
    {
        var state = new NutritionState();
        state.Eat(MealQuality.Basic);

        state.BeginNewDay();

        await Assert.That(state.LastMealQuality).IsEqualTo(MealQuality.None);
        await Assert.That(state.AteToday).IsFalse();
    }
}