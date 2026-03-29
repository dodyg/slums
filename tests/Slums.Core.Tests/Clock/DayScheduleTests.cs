using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Jobs;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Core.Tests.Clock;

internal sealed class DayScheduleTests
{
    [Test]
    [Arguments(1, GameDayOfWeek.Saturday)]
    [Arguments(2, GameDayOfWeek.Sunday)]
    [Arguments(3, GameDayOfWeek.Monday)]
    [Arguments(4, GameDayOfWeek.Tuesday)]
    [Arguments(5, GameDayOfWeek.Wednesday)]
    [Arguments(6, GameDayOfWeek.Thursday)]
    [Arguments(7, GameDayOfWeek.Friday)]
    [Arguments(8, GameDayOfWeek.Saturday)]
    [Arguments(14, GameDayOfWeek.Friday)]
    public async Task DayOfWeek_ShouldReturnCorrectDay(int day, GameDayOfWeek expected)
    {
        var clock = new GameClock();
        clock.SetTime(day, 8, 0);

        await Assert.That(clock.DayOfWeek).IsEqualTo(expected);
    }

    [Test]
    public async Task Friday_ShouldBlockBakeryCallCenterAndCafe()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 8, 0);

        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.BlockedJobTypes).Contains(nameof(JobType.BakeryWork));
        await Assert.That(schedule.BlockedJobTypes).Contains(nameof(JobType.CallCenterWork));
        await Assert.That(schedule.BlockedJobTypes).Contains(nameof(JobType.CafeService));
    }

    [Test]
    public async Task Friday_BlockedJobsShouldNotAppearInAvailableJobs()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 8, 0);
        state.World.TravelTo(LocationId.Bakery);
        var jobs = state.GetAvailableJobs();
        await Assert.That(jobs.Any(j => j.Type == JobType.BakeryWork)).IsFalse();
    }

    [Test]
    public async Task NonFriday_BakeryShouldBeAvailable()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        state.World.TravelTo(LocationId.Bakery);
        var jobs = state.GetAvailableJobs();
        await Assert.That(jobs.Any(j => j.Type == JobType.BakeryWork)).IsTrue();
    }

    [Test]
    public async Task Saturday_ShouldReduceFoodCostBy2()
    {
        using var state = new GameSession();
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.FoodCostModifier).IsEqualTo(-2);
        await Assert.That(state.GetFoodCost()).IsEqualTo(13);
    }

    [Test]
    public async Task Sunday_ShouldHaveNoFoodCostModifier()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.FoodCostModifier).IsEqualTo(0);
    }

    [Test]
    public async Task Monday_ClinicCostShouldBeLowerThanSunday()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Clinic);

        state.Clock.SetTime(2, 8, 0);
        var sundayCost = state.GetCurrentLocationClinicStatus().VisitCost;

        state.Clock.SetTime(3, 8, 0);
        var mondayCost = state.GetCurrentLocationClinicStatus().VisitCost;
        var schedule = DayScheduleRegistry.GetModifiers(GameDayOfWeek.Monday);
        await Assert.That(schedule.ClinicDiscount).IsTrue();
        var drop = sundayCost - mondayCost;
        var expectedDrop = schedule.ClinicDiscountAmount;
        await Assert.That(drop).IsGreaterThanOrEqualTo(expectedDrop);
    }

    [Test]
    public async Task Monday_ClinicDiscountShouldBeDoubledForMedicalDropout()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        state.World.TravelTo(LocationId.Clinic);

        state.Clock.SetTime(2, 8, 0);
        var sundayCost = state.GetCurrentLocationClinicStatus().VisitCost;
        state.Clock.SetTime(3, 8, 0);
        var mondayCost = state.GetCurrentLocationClinicStatus().VisitCost;
        await Assert.That(mondayCost).IsLessThan(sundayCost);
        await Assert.That(sundayCost - mondayCost).IsGreaterThanOrEqualTo(10);
    }

    [Test]
    public async Task NonMonday_ShouldNotApplyClinicDiscount()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.ClinicDiscount).IsFalse();
    }

    [Test]
    public async Task Friday_ShouldReduceCrimeDetection()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.CrimeDetectionModifier).IsEqualTo(-10);
    }

    [Test]
    public async Task Friday_PrayerGatheringShouldBeAvailable()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.PrayerGatheringAvailable).IsTrue();
    }

    [Test]
    public async Task NonFriday_PrayerGatheringShouldNotBeAvailable()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.PrayerGatheringAvailable).IsFalse();
    }

    [Test]
    public async Task Wednesday_ShouldHaveInvestmentRevenueModifier()
    {
        using var state = new GameSession();
        state.Clock.SetTime(5, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.InvestmentRevenueModifier).IsEqualTo(1);
    }

    [Test]
    public async Task SudaneseRefugee_ShouldGetExtraFoodDiscountOnSaturday()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        await Assert.That(state.GetFoodCost()).IsEqualTo(12);
    }

    [Test]
    public async Task NonSaturday_SudaneseRefugeeShouldGetNoExtraFoodDiscount()
    {
        using var state = new GameSession();
        state.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        state.Clock.SetTime(2, 8, 0);
        await Assert.That(state.GetFoodCost()).IsEqualTo(15);
    }

    [Test]
    public async Task Saturday_StreetFoodCostShouldAlsoBeReduced()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.CallCenter);
        await Assert.That(state.GetStreetFoodCost()).IsEqualTo(8);
    }

    [Test]
    public async Task Friday_ShouldMarketsClosed()
    {
        using var state = new GameSession();
        state.Clock.SetTime(7, 8, 0);
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.MarketsClosed).IsTrue();
    }

    [Test]
    public async Task DayScheduleRegistry_ShouldHaveAllSevenDays()
    {
        await Assert.That(DayScheduleRegistry.AllModifiers.Count).IsEqualTo(7);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Saturday);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Sunday);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Monday);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Tuesday);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Wednesday);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Thursday);
        await Assert.That(DayScheduleRegistry.AllModifiers.Keys).Contains(GameDayOfWeek.Friday);
    }

    [Test]
    public async Task GameDayOfWeekExtensions_ToSystemDayOfWeek_ShouldMapCorrectly()
    {
        await Assert.That(GameDayOfWeek.Saturday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Saturday);
        await Assert.That(GameDayOfWeek.Sunday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Sunday);
        await Assert.That(GameDayOfWeek.Monday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Monday);
        await Assert.That(GameDayOfWeek.Tuesday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Tuesday);
        await Assert.That(GameDayOfWeek.Wednesday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Wednesday);
        await Assert.That(GameDayOfWeek.Thursday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Thursday);
        await Assert.That(GameDayOfWeek.Friday.ToSystemDayOfWeek()).IsEqualTo(System.DayOfWeek.Friday);
    }

    [Test]
    public async Task GetCurrentSchedule_ShouldReturnScheduleForCurrentDay()
    {
        using var state = new GameSession();
        var schedule = state.GetCurrentSchedule();
        await Assert.That(schedule.Day).IsEqualTo(GameDayOfWeek.Saturday);
        await Assert.That(schedule.DayName).IsEqualTo("Saturday");
    }

    [Test]
    public async Task ClinicOpenCheck_ShouldUseSystemDayOfWeek()
    {
        using var state = new GameSession();
        state.Clock.SetTime(3, 8, 0);
        state.World.TravelTo(LocationId.Clinic);
        var clinicStatus = state.GetCurrentLocationClinicStatus();
        var location = WorldState.AllLocations.First(l => l.Id == LocationId.Clinic);
        var isOpen = location.ClinicOpenDays.Contains(System.DayOfWeek.Monday);
        await Assert.That(clinicStatus.IsOpenToday).IsEqualTo(isOpen);
    }

    [Test]
    public async Task Saturday_LaundryPressingShouldHaveExtraPay()
    {
        using var state = new GameSession();
        var schedule = DayScheduleRegistry.GetModifiers(GameDayOfWeek.Saturday);
        await Assert.That(schedule.JobPayOverrides.ContainsKey(nameof(JobType.LaundryPressing))).IsTrue();
        await Assert.That(schedule.JobPayOverrides[nameof(JobType.LaundryPressing)]).IsEqualTo(2);
    }

    [Test]
    public async Task Saturday_LaundryPressingJobPreviewShouldShowHigherPay()
    {
        using var state = new GameSession();
        state.World.TravelTo(LocationId.Laundry);
        var preview = state.PreviewJob(JobType.LaundryPressing);
        await Assert.That(preview.ActiveModifiers.Any(m => m.Contains("Saturday", StringComparison.Ordinal) && m.Contains("LaundryPressing", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task NonSaturday_LaundryPressingShouldHaveNormalPay()
    {
        using var state = new GameSession();
        state.Clock.SetTime(2, 8, 0);
        state.World.TravelTo(LocationId.Laundry);
        var preview = state.PreviewJob(JobType.LaundryPressing);
        await Assert.That(preview.ActiveModifiers.Any(m => m.Contains("Saturday", StringComparison.Ordinal))).IsFalse();
    }
}
