using static Slums.Core.Jobs.JobType;

namespace Slums.Core.Clock;

public static class DayScheduleRegistry
{
    private static readonly IReadOnlyDictionary<string, int> SaturdayPayOverrides =
        new Dictionary<string, int>
        {
            [nameof(LaundryPressing)] = 2
        };

    private static readonly IReadOnlyDictionary<string, int> EmptyPayOverrides =
        new Dictionary<string, int>();

    public static IReadOnlyDictionary<GameDayOfWeek, DayScheduleModifiers> AllModifiers { get; } =
        new Dictionary<GameDayOfWeek, DayScheduleModifiers>
        {
            [GameDayOfWeek.Saturday] = new(
                GameDayOfWeek.Saturday,
                "Saturday",
                MarketsClosed: false,
                FoodCostModifier: -2,
                JobPayModifier: 0,
                CrimeDetectionModifier: 0,
                PrayerGatheringAvailable: false,
                ClinicDiscount: false,
                ClinicDiscountAmount: 0,
                InvestmentRevenueModifier: 0,
                BlockedJobTypes: [],
                JobPayOverrides: SaturdayPayOverrides),

            [GameDayOfWeek.Sunday] = new(
                GameDayOfWeek.Sunday,
                "Sunday",
                MarketsClosed: false,
                FoodCostModifier: 0,
                JobPayModifier: 0,
                CrimeDetectionModifier: 0,
                PrayerGatheringAvailable: false,
                ClinicDiscount: false,
                ClinicDiscountAmount: 0,
                InvestmentRevenueModifier: 0,
                BlockedJobTypes: [],
                JobPayOverrides: EmptyPayOverrides),

            [GameDayOfWeek.Monday] = new(
                GameDayOfWeek.Monday,
                "Monday",
                MarketsClosed: false,
                FoodCostModifier: 0,
                JobPayModifier: 0,
                CrimeDetectionModifier: 0,
                PrayerGatheringAvailable: false,
                ClinicDiscount: true,
                ClinicDiscountAmount: 5,
                InvestmentRevenueModifier: 0,
                BlockedJobTypes: [],
                JobPayOverrides: EmptyPayOverrides),

            [GameDayOfWeek.Tuesday] = new(
                GameDayOfWeek.Tuesday,
                "Tuesday",
                MarketsClosed: false,
                FoodCostModifier: 0,
                JobPayModifier: 0,
                CrimeDetectionModifier: 0,
                PrayerGatheringAvailable: false,
                ClinicDiscount: false,
                ClinicDiscountAmount: 0,
                InvestmentRevenueModifier: 0,
                BlockedJobTypes: [],
                JobPayOverrides: EmptyPayOverrides),

            [GameDayOfWeek.Wednesday] = new(
                GameDayOfWeek.Wednesday,
                "Wednesday",
                MarketsClosed: false,
                FoodCostModifier: 0,
                JobPayModifier: 0,
                CrimeDetectionModifier: 0,
                PrayerGatheringAvailable: false,
                ClinicDiscount: false,
                ClinicDiscountAmount: 0,
                InvestmentRevenueModifier: 1,
                BlockedJobTypes: [],
                JobPayOverrides: EmptyPayOverrides),

            [GameDayOfWeek.Thursday] = new(
                GameDayOfWeek.Thursday,
                "Thursday",
                MarketsClosed: false,
                FoodCostModifier: 0,
                JobPayModifier: 0,
                CrimeDetectionModifier: 0,
                PrayerGatheringAvailable: false,
                ClinicDiscount: false,
                ClinicDiscountAmount: 0,
                InvestmentRevenueModifier: 0,
                BlockedJobTypes: [],
                JobPayOverrides: EmptyPayOverrides),

            [GameDayOfWeek.Friday] = new(
                GameDayOfWeek.Friday,
                "Friday",
                MarketsClosed: true,
                FoodCostModifier: 0,
                JobPayModifier: 0,
                CrimeDetectionModifier: -10,
                PrayerGatheringAvailable: true,
                ClinicDiscount: false,
                ClinicDiscountAmount: 0,
                InvestmentRevenueModifier: 0,
                BlockedJobTypes: [nameof(BakeryWork), nameof(CallCenterWork), nameof(CafeService)],
                JobPayOverrides: EmptyPayOverrides),
        };

    public static DayScheduleModifiers GetModifiers(GameDayOfWeek day)
    {
        return AllModifiers.TryGetValue(day, out var modifiers)
            ? modifiers
            : AllModifiers[GameDayOfWeek.Saturday];
    }
}
