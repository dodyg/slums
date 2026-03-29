namespace Slums.Core.Clock;

public sealed record DayScheduleModifiers(
    GameDayOfWeek Day,
    string DayName,
    bool MarketsClosed,
    int FoodCostModifier,
    int JobPayModifier,
    int CrimeDetectionModifier,
    bool PrayerGatheringAvailable,
    bool ClinicDiscount,
    int ClinicDiscountAmount,
    int InvestmentRevenueModifier,
    IReadOnlyList<string> BlockedJobTypes,
    IReadOnlyDictionary<string, int> JobPayOverrides);
