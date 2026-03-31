using FluentAssertions;
using Slums.Core.Calendar;
using TUnit.Core;

namespace Slums.Core.Tests.Calendar;

internal sealed class HolidayRegistryTests
{
    [Test]
    public async Task AllHolidays_ShouldContainFiveHolidays()
    {
        await Assert.That(HolidayRegistry.AllHolidays.Count).IsEqualTo(5);
    }

    [Test]
    public async Task GetHolidayState_WhenNoHoliday_ShouldReturnNone()
    {
        var date = new DateOnly(2024, 10, 15);

        var state = HolidayRegistry.GetHolidayState(date);

        await Assert.That(state.IsActive).IsFalse();
        await Assert.That(state.Id).IsEqualTo(HolidayId.None);
    }

    [Test]
    public async Task GetHolidayState_WhenCopticChristmas_ShouldReturnActiveState()
    {
        var date = new DateOnly(2025, 1, 7);

        var state = HolidayRegistry.GetHolidayState(date);

        await Assert.That(state.IsActive).IsTrue();
        await Assert.That(state.Id).IsEqualTo(HolidayId.CopticChristmas);
        await Assert.That(state.Name).IsEqualTo("Coptic Christmas");
        await Assert.That(state.CommunityEventAvailable).IsTrue();
    }

    [Test]
    public async Task GetHolidayState_WhenRamadanStart_ShouldReturnRamadanState()
    {
        var date = new DateOnly(2025, 2, 28);

        var state = HolidayRegistry.GetHolidayState(date);

        await Assert.That(state.IsActive).IsTrue();
        await Assert.That(state.IsRamadan).IsTrue();
        await Assert.That(state.CurrentDay).IsEqualTo(1);
        await Assert.That(state.DaysRemaining).IsEqualTo(29);
    }

    [Test]
    public async Task GetHolidayState_WhenRamadanMiddle_ShouldReturnCorrectDay()
    {
        var date = new DateOnly(2025, 3, 10);

        var state = HolidayRegistry.GetHolidayState(date);

        await Assert.That(state.IsActive).IsTrue();
        await Assert.That(state.IsRamadan).IsTrue();
        await Assert.That(state.CurrentDay).IsEqualTo(11);
    }

    [Test]
    public async Task GetHolidayState_WhenEidAlFitr_ShouldReturnActiveState()
    {
        var date = new DateOnly(2025, 3, 30);

        var state = HolidayRegistry.GetHolidayState(date);

        await Assert.That(state.IsActive).IsTrue();
        await Assert.That(state.Id).IsEqualTo(HolidayId.EidAlFitr);
        await Assert.That(state.StressModifier).IsEqualTo(5);
    }

    [Test]
    public async Task IsRamadanActive_WhenRamadanDate_ShouldReturnTrue()
    {
        var date = new DateOnly(2025, 3, 15);

        var isRamadan = HolidayRegistry.IsRamadanActive(date);

        await Assert.That(isRamadan).IsTrue();
    }

    [Test]
    public async Task IsRamadanActive_WhenNotRamadanDate_ShouldReturnFalse()
    {
        var date = new DateOnly(2025, 1, 15);

        var isRamadan = HolidayRegistry.IsRamadanActive(date);

        await Assert.That(isRamadan).IsFalse();
    }

    [Test]
    public async Task GetRamadanDay_WhenRamadanDate_ShouldReturnCorrectDay()
    {
        var date = new DateOnly(2025, 3, 5);

        var day = HolidayRegistry.GetRamadanDay(date);

        await Assert.That(day).IsEqualTo(6);
    }

    [Test]
    public async Task GetRamadanDay_WhenNotRamadanDate_ShouldReturnZero()
    {
        var date = new DateOnly(2025, 1, 15);

        var day = HolidayRegistry.GetRamadanDay(date);

        await Assert.That(day).IsEqualTo(0);
    }

    [Test]
    public async Task GetHoliday_ShouldReturnCorrectHoliday()
    {
        var holiday = HolidayRegistry.GetHoliday(HolidayId.CopticChristmas);

        await Assert.That(holiday).IsNotNull();
        await Assert.That(holiday!.Id).IsEqualTo(HolidayId.CopticChristmas);
        await Assert.That(holiday.DurationDays).IsEqualTo(1);
    }

    [Test]
    public async Task GetHoliday_WhenInvalidId_ShouldReturnNull()
    {
        var holiday = HolidayRegistry.GetHoliday(HolidayId.None);

        await Assert.That(holiday).IsNull();
    }
}
