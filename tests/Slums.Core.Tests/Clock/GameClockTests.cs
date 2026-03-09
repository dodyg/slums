using FluentAssertions;
using Slums.Core.Clock;
using Xunit;

namespace Slums.Core.Tests;

public class GameClockTests
{
    [Fact]
    public void Constructor_ShouldInitializeToDay1Morning()
    {
        var clock = new GameClock();

        clock.Day.Should().Be(1);
        clock.Hour.Should().Be(6);
        clock.Minute.Should().Be(0);
        clock.TimeOfDay.Should().Be(TimeOfDay.Morning);
    }

    [Fact]
    public void AdvanceMinutes_ShouldIncrementMinutesCorrectly()
    {
        var clock = new GameClock();

        clock.AdvanceMinutes(30);

        clock.Minute.Should().Be(30);
        clock.Hour.Should().Be(6);
    }

    [Fact]
    public void AdvanceMinutes_ShouldRollOverToNextHour()
    {
        var clock = new GameClock();

        clock.AdvanceMinutes(90);

        clock.Minute.Should().Be(30);
        clock.Hour.Should().Be(7);
    }

    [Fact]
    public void AdvanceHours_ShouldIncrementHoursCorrectly()
    {
        var clock = new GameClock();

        clock.AdvanceHours(5);

        clock.Hour.Should().Be(11);
        clock.Day.Should().Be(1);
    }

    [Fact]
    public void AdvanceHours_ShouldRollOverToNextDay()
    {
        var clock = new GameClock();

        clock.AdvanceHours(20);

        clock.Hour.Should().Be(2);
        clock.Day.Should().Be(2);
    }

    [Fact]
    public void AdvanceToNextDay_ShouldResetTimeAndIncrementDay()
    {
        var clock = new GameClock();
        clock.AdvanceHours(10);

        clock.AdvanceToNextDay();

        clock.Day.Should().Be(2);
        clock.Hour.Should().Be(6);
        clock.Minute.Should().Be(0);
    }

    [Theory]
    [InlineData(5, TimeOfDay.Morning)]
    [InlineData(11, TimeOfDay.Morning)]
    [InlineData(12, TimeOfDay.Afternoon)]
    [InlineData(16, TimeOfDay.Afternoon)]
    [InlineData(17, TimeOfDay.Evening)]
    [InlineData(20, TimeOfDay.Evening)]
    [InlineData(21, TimeOfDay.Night)]
    [InlineData(4, TimeOfDay.Night)]
    public void TimeOfDay_ShouldReturnCorrectTimeOfDay(int hour, TimeOfDay expected)
    {
        var clock = new GameClock();
        clock.AdvanceHours(hour - 6);

        clock.TimeOfDay.Should().Be(expected);
    }

    [Fact]
    public void IsEndOfDay_ShouldReturnTrueWhenHourIs22OrLater()
    {
        var clock = new GameClock();
        clock.AdvanceHours(16);

        clock.IsEndOfDay.Should().BeTrue();
    }
}
