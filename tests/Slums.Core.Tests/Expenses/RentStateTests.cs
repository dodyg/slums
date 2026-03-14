using Slums.Core.Expenses;
using TUnit.Core;

namespace Slums.Core.Tests.Expenses;

internal sealed class RentStateTests
{
    [Test]
    public async Task ProcessDay_ShouldPayRentAndResetCounters_WhenPlayerHasEnoughMoney()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 2, accumulatedDebt: 40, firstWarningGiven: false, finalWarningGiven: false);

        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 25);

        await Assert.That(result.Paid).IsTrue();
        await Assert.That(result.AmountPaid).IsEqualTo(20);
        await Assert.That(result.WarningType).IsEqualTo(RentWarningType.None);
        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(0);
        await Assert.That(rentState.FirstWarningGiven).IsFalse();
        await Assert.That(rentState.FinalWarningGiven).IsFalse();
    }

    [Test]
    public async Task ProcessDay_ShouldNotAccumulateDebt_WhenPlayerPaysRent()
    {
        var rentState = new RentState();

        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 50);

        await Assert.That(result.Paid).IsTrue();
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessDay_ShouldAccumulateDebt_WhenPlayerCannotAffordRent()
    {
        var rentState = new RentState();

        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);

        await Assert.That(result.Paid).IsFalse();
        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(1);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(20);
        await Assert.That(result.WarningType).IsEqualTo(RentWarningType.None);
    }

    [Test]
    public async Task ProcessDay_ShouldGiveFirstWarning_OnThirdUnpaidDay()
    {
        var rentState = new RentState();

        rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
        rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);

        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(3);
        await Assert.That(result.WarningType).IsEqualTo(RentWarningType.First);
        await Assert.That(rentState.FirstWarningGiven).IsTrue();
    }

    [Test]
    public async Task ProcessDay_ShouldGiveFinalWarning_OnFifthUnpaidDay()
    {
        var rentState = new RentState();

        for (var day = 1; day <= 5; day++)
        {
            var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
            if (day == 5)
            {
                await Assert.That(result.WarningType).IsEqualTo(RentWarningType.Final);
            }
        }

        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(5);
        await Assert.That(rentState.FinalWarningGiven).IsTrue();
    }

    [Test]
    public async Task ProcessDay_ShouldTriggerEviction_OnSeventhUnpaidDay()
    {
        var rentState = new RentState();

        for (var day = 1; day <= 7; day++)
        {
            var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
            if (day == 7)
            {
                await Assert.That(result.WarningType).IsEqualTo(RentWarningType.Eviction);
            }
        }

        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(7);
    }

    [Test]
    public async Task ProcessDay_ShouldNotRepeatWarnings_WhenAlreadyGiven()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 2, accumulatedDebt: 40, firstWarningGiven: true, finalWarningGiven: false);

        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);

        await Assert.That(result.WarningType).IsEqualTo(RentWarningType.None);
        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(3);
    }

    [Test]
    public async Task ProcessDay_ShouldNotRepeatFinalWarning_WhenAlreadyGiven()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 4, accumulatedDebt: 80, firstWarningGiven: true, finalWarningGiven: true);

        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);

        await Assert.That(result.WarningType).IsEqualTo(RentWarningType.None);
        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(5);
    }

    [Test]
    public async Task ResetWarnings_ShouldClearAllCounters()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 5, accumulatedDebt: 100, firstWarningGiven: true, finalWarningGiven: true);

        rentState.ResetWarnings();

        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(0);
        await Assert.That(rentState.FirstWarningGiven).IsFalse();
        await Assert.That(rentState.FinalWarningGiven).IsFalse();
    }

    [Test]
    public async Task PayPartialDebt_ShouldReduceAccumulatedDebt()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 3, accumulatedDebt: 60, firstWarningGiven: true, finalWarningGiven: false);

        rentState.PayPartialDebt(25);

        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(35);
    }

    [Test]
    public async Task PayPartialDebt_ShouldNotGoNegative()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 1, accumulatedDebt: 20, firstWarningGiven: false, finalWarningGiven: false);

        rentState.PayPartialDebt(100);

        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(0);
    }

    [Test]
    public async Task Restore_ShouldSetAllProperties()
    {
        var rentState = new RentState();

        rentState.Restore(unpaidRentDays: 4, accumulatedDebt: 80, firstWarningGiven: true, finalWarningGiven: false);

        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(4);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(80);
        await Assert.That(rentState.FirstWarningGiven).IsTrue();
        await Assert.That(rentState.FinalWarningGiven).IsFalse();
    }

    [Test]
    public async Task Restore_ShouldClampNegativeValuesToZero()
    {
        var rentState = new RentState();

        rentState.Restore(unpaidRentDays: -5, accumulatedDebt: -100, firstWarningGiven: false, finalWarningGiven: false);

        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(0);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(0);
    }

    [Test]
    public async Task AccumulatedDebt_ShouldIncreaseEachUnpaidDay()
    {
        var rentState = new RentState();

        rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(20);

        rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(40);

        rentState.ProcessDay(dailyRentCost: 20, playerMoney: 0);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(60);
    }

    [Test]
    public async Task PayingRent_ShouldClearUnpaidDaysButKeepAccumulatedDebt()
    {
        var rentState = new RentState();
        rentState.Restore(unpaidRentDays: 4, accumulatedDebt: 80, firstWarningGiven: true, finalWarningGiven: false);

        var result = rentState.ProcessDay(dailyRentCost: 20, playerMoney: 100);

        await Assert.That(result.Paid).IsTrue();
        await Assert.That(rentState.UnpaidRentDays).IsEqualTo(0);
        await Assert.That(rentState.AccumulatedRentDebt).IsEqualTo(80);
        await Assert.That(rentState.FirstWarningGiven).IsFalse();
        await Assert.That(rentState.FinalWarningGiven).IsFalse();
    }
}
