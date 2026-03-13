using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.Narrative;

internal sealed class NarrativeSceneExtensionsTests
{
    [Test]
    public async Task ApplyOutcome_ShouldModifyMoney_WhenMoneyChangeIsNonZero()
    {
        using var state = new GameSession();
        var initialMoney = state.Player.Stats.Money;
        var outcome = new NarrativeOutcome { MoneyChange = 50 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Money).IsEqualTo(initialMoney + 50);
    }

    [Test]
    public async Task ApplyOutcome_ShouldReduceMoney_WhenMoneyChangeIsNegative()
    {
        using var state = new GameSession();
        var initialMoney = state.Player.Stats.Money;
        var outcome = new NarrativeOutcome { MoneyChange = -30 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Money).IsEqualTo(initialMoney - 30);
    }

    [Test]
    public async Task ApplyOutcome_ShouldModifyHealth_WhenHealthChangeIsNonZero()
    {
        using var state = new GameSession();
        var initialHealth = state.Player.Stats.Health;
        var outcome = new NarrativeOutcome { HealthChange = -20 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Health).IsEqualTo(initialHealth - 20);
    }

    [Test]
    public async Task ApplyOutcome_ShouldModifyEnergy_WhenEnergyChangeIsNonZero()
    {
        using var state = new GameSession();
        var initialEnergy = state.Player.Stats.Energy;
        var outcome = new NarrativeOutcome { EnergyChange = -15 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(initialEnergy - 15);
    }

    [Test]
    public async Task ApplyOutcome_ShouldModifyHunger_WhenHungerChangeIsNonZero()
    {
        using var state = new GameSession();
        state.Player.Nutrition.ModifySatiety(-30);
        state.Player.Stats.SetHunger(state.Player.Nutrition.Satiety);
        var initialHunger = state.Player.Nutrition.Satiety;
        var outcome = new NarrativeOutcome { HungerChange = 20 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Nutrition.Satiety).IsEqualTo(initialHunger + 20);
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(initialHunger + 20);
    }

    [Test]
    public async Task ApplyOutcome_ShouldModifyStress_WhenStressChangeIsNonZero()
    {
        using var state = new GameSession();
        var initialStress = state.Player.Stats.Stress;
        var outcome = new NarrativeOutcome { StressChange = 25 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Stress).IsEqualTo(initialStress + 25);
    }

    [Test]
    public async Task ApplyOutcome_ShouldModifyMotherHealth_WhenMotherHealthChangeIsNonZero()
    {
        using var state = new GameSession();
        var initialMotherHealth = state.Player.Household.MotherHealth;
        var outcome = new NarrativeOutcome { MotherHealthChange = -10 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Household.MotherHealth).IsEqualTo(initialMotherHealth - 10);
    }

    [Test]
    public async Task ApplyOutcome_ShouldAddFood_WhenFoodChangeIsPositive()
    {
        using var state = new GameSession();
        var initialFood = state.Player.Household.FoodStockpile;
        var outcome = new NarrativeOutcome { FoodChange = 5 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(initialFood + 5);
    }

    [Test]
    public async Task ApplyOutcome_ShouldRemoveFood_WhenFoodChangeIsNegative()
    {
        using var state = new GameSession();
        var initialFood = state.Player.Household.FoodStockpile;
        var outcome = new NarrativeOutcome { FoodChange = -2 };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(initialFood - 2);
    }

    [Test]
    public async Task ApplyOutcome_ShouldDoNothing_WhenAllChangesAreZero()
    {
        using var state = new GameSession();
        var initialMoney = state.Player.Stats.Money;
        var initialHealth = state.Player.Stats.Health;
        var initialEnergy = state.Player.Stats.Energy;
        var outcome = new NarrativeOutcome();

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Money).IsEqualTo(initialMoney);
        await Assert.That(state.Player.Stats.Health).IsEqualTo(initialHealth);
        await Assert.That(state.Player.Stats.Energy).IsEqualTo(initialEnergy);
    }

    [Test]
    public async Task ApplyOutcome_ShouldHandleMultipleChanges()
    {
        using var state = new GameSession();
        var initialMoney = state.Player.Stats.Money;
        var initialHealth = state.Player.Stats.Health;
        var initialStress = state.Player.Stats.Stress;

        var outcome = new NarrativeOutcome
        {
            MoneyChange = 25,
            HealthChange = -10,
            StressChange = 15
        };

        state.ApplyOutcome(outcome);

        await Assert.That(state.Player.Stats.Money).IsEqualTo(initialMoney + 25);
        await Assert.That(state.Player.Stats.Health).IsEqualTo(initialHealth - 10);
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(initialStress + 15);
    }

    [Test]
    public async Task ApplyOutcome_ShouldThrow_WhenStateIsNull()
    {
        var outcome = new NarrativeOutcome { MoneyChange = 10 };

        var act = () => ((INarrativeOutcomeTarget)null!).ApplyOutcome(outcome);

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ApplyOutcome_ShouldThrow_WhenOutcomeIsNull()
    {
        using var state = new GameSession();

        var act = () => state.ApplyOutcome(null!);

        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public void ApplyOutcome_ShouldClampHealthToValidRange()
    {
        using var state = new GameSession();
        var outcome = new NarrativeOutcome { HealthChange = -200 };

        state.ApplyOutcome(outcome);

        state.Player.Stats.Health.Should().BeGreaterOrEqualTo(0);
    }

    [Test]
    public void ApplyOutcome_ShouldClampEnergyToValidRange()
    {
        using var state = new GameSession();
        var outcome = new NarrativeOutcome { EnergyChange = 200 };

        state.ApplyOutcome(outcome);

        state.Player.Stats.Energy.Should().BeLessOrEqualTo(100);
    }

    [Test]
    public void ApplyOutcome_ShouldClampStressToValidRange()
    {
        using var state = new GameSession();
        var outcome = new NarrativeOutcome { StressChange = 200 };

        state.ApplyOutcome(outcome);

        state.Player.Stats.Stress.Should().BeLessOrEqualTo(100);
    }
}
