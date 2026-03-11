using FluentAssertions;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace Slums.Core.Tests.State;

internal sealed class GameStateTests
{
    [Test]
    public async Task Constructor_ShouldInitializeWithDefaultValues()
    {
        var state = new GameState();

        await Assert.That(state.RunId).IsNotEqualTo(Guid.Empty);
        await Assert.That(state.Clock.Day).IsEqualTo(1);
        await Assert.That(state.Player).IsNotNull();
        await Assert.That(state.World).IsNotNull();
        await Assert.That(state.IsGameOver).IsFalse();
    }

    [Test]
    public async Task EndDay_ShouldDeductRentFromMoney()
    {
        var state = new GameState();

        state.EndDay();

        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task EatAtHome_ShouldFeedPlayerAndMother()
    {
        var state = new GameState();

        var result = state.EatAtHome();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.FoodStockpile).IsEqualTo(2);
        await Assert.That(state.Player.Household.FedMotherToday).IsTrue();
        await Assert.That(state.Player.Nutrition.AteToday).IsTrue();
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(97);
    }

    [Test]
    public async Task EndDay_ShouldAdvanceToNextDay()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(10);

        state.EndDay();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
    }

    [Test]
    public async Task EndDay_ShouldReturnPlayerHome()
    {
        var state = new GameState();
        state.World.TravelTo(LocationId.Market);

        state.EndDay(new Random(1));

        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task EatAtHome_WithoutStaples_ShouldFail()
    {
        var state = new GameState();
        state.Player.Household.SetFoodStockpile(0);

        var result = state.EatAtHome();

        await Assert.That(result).IsFalse();
        await Assert.That(state.Player.Nutrition.AteToday).IsFalse();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
    }

    [Test]
    public async Task EatStreetFood_ShouldCostMoneyAndFeedOnlyPlayer()
    {
        var state = new GameState();

        var result = state.EatStreetFood();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Stats.Money).IsEqualTo(92);
        await Assert.That(state.Player.Nutrition.AteToday).IsTrue();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
    }

    [Test]
    public async Task BuyMedicine_ShouldIncreaseMedicineStock()
    {
        var state = new GameState();

        var result = state.BuyMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.MedicineStock).IsEqualTo(2);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(50);
    }

    [Test]
    public async Task GiveMotherMedicine_ShouldConsumeMedicineStock()
    {
        var state = new GameState();
        state.Player.Household.SetMedicineStock(2);

        var result = state.GiveMotherMedicine();

        await Assert.That(result).IsTrue();
        await Assert.That(state.Player.Household.MedicineStock).IsEqualTo(1);
        await Assert.That(state.Player.Household.MedicationGivenToday).IsTrue();
    }

    [Test]
    public async Task RestAtHome_ShouldRestoreEnergyAndAdvanceTime()
    {
        var state = new GameState();

        state.RestAtHome();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(100);
        await Assert.That(state.Clock.Hour).IsEqualTo(14);
    }

    [Test]
    public async Task RestAtHome_ShouldTriggerEndDayWhenRestPassesCurfew()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(12);

        state.RestAtHome();

        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task TryTravelTo_ShouldSucceedWithEnoughMoney()
    {
        var state = new GameState();

        var result = state.TryTravelTo(LocationId.Market);

        await Assert.That(result).IsTrue();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Market);
        await Assert.That(state.Player.Stats.Money).IsEqualTo(98);
    }

    [Test]
    public async Task TryTravelTo_ShouldFailWithInsufficientMoney()
    {
        var state = new GameState();
        state.Player.Stats.ModifyMoney(-99);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsFalse();
        await Assert.That(state.World.CurrentLocationId).IsEqualTo(LocationId.Home);
    }

    [Test]
    public async Task TryTravelTo_ShouldAdvanceTimeByTravelDuration()
    {
        var state = new GameState();

        state.TryTravelTo(LocationId.Market);

        await Assert.That(state.Clock.Minute).IsEqualTo(15);
    }

    [Test]
    public async Task TryTravelTo_ShouldTriggerEndDayWhenTravelPassesCurfew()
    {
        var state = new GameState();
        state.Clock.AdvanceHours(15);
        state.Clock.AdvanceMinutes(50);

        var result = state.TryTravelTo(LocationId.CallCenter);

        await Assert.That(result).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(6);
        await Assert.That(state.Clock.Minute).IsEqualTo(35);
    }

    [Test]
    public async Task WorkJob_ShouldTriggerEndDayWhenShiftPassesCurfew()
    {
        var state = new GameState();
        state.World.TravelTo(LocationId.Bakery);
        state.Clock.AdvanceHours(14);

        var result = state.WorkJob(Slums.Core.Jobs.JobRegistry.BakeryWork);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(state.Clock.Day).IsEqualTo(2);
        await Assert.That(state.Clock.Hour).IsEqualTo(10);
        await Assert.That(state.Clock.Minute).IsEqualTo(0);
    }

    [Test]
    public async Task IsGameOver_ShouldBeTrueWhenHealthIsZero()
    {
        var state = new GameState();
        state.Player.Stats.ModifyHealth(-100);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("health");
    }

    [Test]
    public async Task GameEvent_ShouldBeRaisedForActions()
    {
        var state = new GameState();
        var events = new List<string>();
        state.GameEvent += (_, e) => events.Add(e.Message);

        state.TryTravelTo(LocationId.Market);

        events.Should().ContainMatch("*Traveled*");
    }

    [Test]
    public async Task CommitCrime_ShouldApplyMoneyEnergyAndPressureChanges()
    {
        var state = new GameState();
        var initialMoney = state.Player.Stats.Money;
        var initialEnergy = state.Player.Stats.Energy;

        var result = state.CommitCrime(new Slums.Core.Crimes.CrimeAttempt(
            Slums.Core.Crimes.CrimeType.PettyTheft,
            25,
            20,
            10,
            0,
            10), new Random(5));

        await Assert.That(state.Player.Stats.Energy).IsLessThan(initialEnergy);
        await Assert.That(state.PolicePressure).IsGreaterThan(0);
        state.Player.Stats.Money.Should().BeGreaterOrEqualTo(initialMoney);
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task EndDay_ShouldDecayPolicePressure_OnCleanDay()
    {
        var state = new GameState();
        state.SetPolicePressure(25);

        state.EndDay(new Random(2));

        await Assert.That(state.PolicePressure).IsEqualTo(20);
    }

    [Test]
    public async Task EndDay_PlayerWithoutMeal_ShouldLoseEnergyAndGainStress()
    {
        var state = new GameState();

        state.EndDay();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(58);
        await Assert.That(state.Player.Stats.Stress).IsEqualTo(31);
        await Assert.That(state.Player.Stats.Hunger).IsEqualTo(57);
    }

    [Test]
    public async Task EndDay_PlayerUnderfedForTwoDays_ShouldLoseHealth()
    {
        var state = new GameState();
        state.Player.Nutrition.SetDaysUndereating(1);

        state.EndDay();

        await Assert.That(state.Player.Stats.Health).IsEqualTo(95);
    }

    [Test]
    public async Task EndDay_MotherFragileWithoutMedicine_ShouldLoseHealth()
    {
        var state = new GameState();
        state.Player.Household.SetMotherHealth(45);
        state.Player.Household.FeedMother();

        state.EndDay();

        await Assert.That(state.Player.Household.MotherHealth).IsEqualTo(39);
    }

    [Test]
    public async Task EndDay_MotherInCrisisWithoutCheck_ShouldIncreasePlayerStress()
    {
        var state = new GameState();
        state.Player.Household.SetMotherHealth(20);
        state.Player.Household.FeedMother();
        state.Player.Household.SetMedicineStock(1);
        state.Player.Household.GiveMedicine();

        state.EndDay();

        await Assert.That(state.Player.Stats.Stress).IsEqualTo(36);
    }

    [Test]
    public async Task EndDay_ShouldResetNutritionAndCareFlagsForNextDay()
    {
        var state = new GameState();
        state.EatAtHome();
        state.Player.Household.SetMedicineStock(1);
        state.GiveMotherMedicine();
        state.CheckOnMother();

        state.EndDay();

        await Assert.That(state.Player.Nutrition.AteToday).IsFalse();
        await Assert.That(state.Player.Household.FedMotherToday).IsFalse();
        await Assert.That(state.Player.Household.MedicationGivenToday).IsFalse();
        await Assert.That(state.Player.Household.CheckedOnMotherToday).IsFalse();
    }

    [Test]
    public async Task EndDay_ShouldTriggerGameOver_WhenMotherHealthFallsToZero()
    {
        var state = new GameState();
        state.Player.Household.SetMotherHealth(4);

        state.EndDay();

        await Assert.That(state.IsGameOver).IsTrue();
        await Assert.That(state.GameOverReason).Contains("mother");
    }

    [Test]
    public async Task GetStatusSummary_ShouldReturnCurrentStatus()
    {
        var state = new GameState();

        var summary = state.GetStatusSummary();

        summary.Should().HaveCount(8);
        summary[0].Should().Contain("Day 1");
        summary[2].Should().Contain("Money");
    }
}
