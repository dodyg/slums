using FluentAssertions;
using Slums.Core.Calendar;
using TUnit.Core;

namespace Slums.Core.Tests.Calendar;

internal sealed class HolidayTests
{
    [Test]
    public async Task EndDate_ShouldCalculateFromDuration()
    {
        var holiday = new Holiday
        {
            StartDate = new DateOnly(2025, 3, 30),
            DurationDays = 3
        };

        await Assert.That(holiday.EndDate).IsEqualTo(new DateOnly(2025, 4, 1));
    }

    [Test]
    public async Task IsActiveOn_WhenDateWithinRange_ShouldReturnTrue()
    {
        var holiday = new Holiday
        {
            StartDate = new DateOnly(2025, 2, 28),
            DurationDays = 30
        };

        await Assert.That(holiday.IsActiveOn(new DateOnly(2025, 3, 10))).IsTrue();
    }

    [Test]
    public async Task IsActiveOn_WhenDateBeforeStart_ShouldReturnFalse()
    {
        var holiday = new Holiday
        {
            StartDate = new DateOnly(2025, 2, 28),
            DurationDays = 30
        };

        await Assert.That(holiday.IsActiveOn(new DateOnly(2025, 2, 27))).IsFalse();
    }

    [Test]
    public async Task IsActiveOn_WhenDateAfterEnd_ShouldReturnFalse()
    {
        var holiday = new Holiday
        {
            StartDate = new DateOnly(2025, 2, 28),
            DurationDays = 30
        };

        await Assert.That(holiday.IsActiveOn(new DateOnly(2025, 4, 1))).IsFalse();
    }

    [Test]
    public async Task IsActiveOn_WhenExactStartDate_ShouldReturnTrue()
    {
        var holiday = new Holiday
        {
            StartDate = new DateOnly(2025, 1, 7),
            DurationDays = 1
        };

        await Assert.That(holiday.IsActiveOn(new DateOnly(2025, 1, 7))).IsTrue();
    }

    [Test]
    public async Task IsActiveOn_WhenExactEndDate_ShouldReturnTrue()
    {
        var holiday = new Holiday
        {
            StartDate = new DateOnly(2025, 3, 30),
            DurationDays = 3
        };

        await Assert.That(holiday.IsActiveOn(new DateOnly(2025, 4, 1))).IsTrue();
    }
}
