using FluentAssertions;
using Slums.Core.Clock;
using TUnit.Core.Interfaces;
using TUnit.Core;

namespace Slums.Core.Tests;

public class GameClockTests
{
    [Test]
    public async Task Constructor_ShouldInitializeToDay1Morning()
    {
        var clock = new GameClock();

        await Assert.That(clock.Day).IsEqualTo(1);
        await Assert.That(clock.Hour).IsEqualTo(6);
        await Assert.That(clock.Minute).IsEqualTo(0);
        await Assert.That(clock.TimeOfDay).IsEqualTo(TimeOfDay.Morning);
    }

    [Test]
    public async Task AdvanceMinutes_ShouldIncrementMinutesCorrectly()
    {
        var clock = new GameClock();

        clock.AdvanceMinutes(30);

        await Assert.That(clock.Minute).IsEqualTo(30);
        await Assert.That(clock.Hour).IsEqualTo(6);
    }

    [Test]
    public async Task AdvanceMinutes_ShouldRollOverToNextHour()
    {
        var clock = new GameClock();

        clock.AdvanceMinutes(90);

        await Assert.That(clock.Minute).IsEqualTo(30);
        await Assert.That(clock.Hour).IsEqualTo(7);
    }

    [Test]
    public async Task AdvanceHours_ShouldIncrementHoursCorrectly()
    {
        var clock = new GameClock();

        clock.AdvanceHours(5);

        await Assert.That(clock.Hour).IsEqualTo(11);
        await Assert.That(clock.Day).IsEqualTo(1);
    }

    [Test]
    public async Task AdvanceHours_ShouldRollOverToNextDay()
    {
        var clock = new GameClock();

        clock.AdvanceHours(20);

        await Assert.That(clock.Hour).IsEqualTo(2);
        await Assert.That(clock.Day).IsEqualTo(2);
    }

    [Test]
    public async Task AdvanceToNextDay_ShouldResetTimeAndIncrementDay()
    {
        var clock = new GameClock();
        clock.AdvanceHours(10);

        clock.AdvanceToNextDay();

        await Assert.That(clock.Day).IsEqualTo(2);
        await Assert.That(clock.Hour).IsEqualTo(6);
        await Assert.That(clock.Minute).IsEqualTo(0);
    }

    [Test]
    [Arguments(5, TimeOfDay.Morning)]
    [Arguments(11, TimeOfDay.Morning)]
    [Arguments(12, TimeOfDay.Afternoon)]
    [Arguments(16, TimeOfDay.Afternoon)]
    [Arguments(17, TimeOfDay.Evening)]
    [Arguments(20, TimeOfDay.Evening)]
    [Arguments(21, TimeOfDay.Night)]
    [Arguments(4, TimeOfDay.Night)]
    public async Task TimeOfDay_ShouldReturnCorrectTimeOfDay(int hour, TimeOfDay expected)
    {
        var clock = new GameClock();
        clock.AdvanceHours(hour - 6);

        await Assert.That(clock.TimeOfDay).IsEqualTo(expected);
    }

    [Test]
    public async Task IsEndOfDay_ShouldReturnTrueWhenHourIs22OrLater()
    {
        var clock = new GameClock();
        clock.AdvanceHours(16);

        await Assert.That(clock.IsEndOfDay).IsTrue();
    }
}
