namespace Slums.Core.Calendar;

public sealed class HolidayRegistry
{
    private static readonly IReadOnlyList<Holiday> Holidays = new List<Holiday>
    {
        new()
        {
            Id = HolidayId.CopticChristmas,
            Name = "Coptic Christmas",
            StartDate = new DateOnly(2025, 1, 7),
            DurationDays = 1,
            Description = "Community gathering event. Bakery demand surge.",
            JobPayModifier = 5,
            StressModifier = -5,
            CommunityEventAvailable = true
        },
        new()
        {
            Id = HolidayId.ShamElNessim,
            Name = "Sham el-Nessim",
            StartDate = new DateOnly(2025, 4, 21),
            DurationDays = 1,
            Description = "Outdoor spring festival. Unique food items available.",
            StressModifier = -8,
            CommunityEventAvailable = true
        },
        new()
        {
            Id = HolidayId.Ramadan,
            Name = "Ramadan",
            StartDate = new DateOnly(2025, 2, 28),
            DurationDays = 30,
            Description = "Month of fasting. Community iftar gatherings available.",
            FoodCostModifier = 0,
            StressModifier = 0,
            CommunityEventAvailable = true
        },
        new()
        {
            Id = HolidayId.EidAlFitr,
            Name = "Eid al-Fitr",
            StartDate = new DateOnly(2025, 3, 30),
            DurationDays = 3,
            Description = "Gift-spending pressure: 20-50 LE expected. Large community events.",
            StressModifier = 5,
            TrustModifierWithNeighbors = -3,
            CommunityEventAvailable = true
        },
        new()
        {
            Id = HolidayId.EidAlAdha,
            Name = "Eid al-Adha",
            StartDate = new DateOnly(2025, 6, 6),
            DurationDays = 4,
            Description = "Meat-sharing event. Market food prices spike then surplus.",
            MotherHealthModifier = 5,
            FoodCostModifier = 5,
            CommunityEventAvailable = true
        }
    }.AsReadOnly();
    
    public static IReadOnlyList<Holiday> AllHolidays => Holidays;
    
    public static ActiveHolidayState GetHolidayState(DateOnly currentDate)
    {
        foreach (var holiday in Holidays)
        {
            if (holiday.IsActiveOn(currentDate))
            {
                var currentDay = currentDate.DayNumber - holiday.StartDate.DayNumber + 1;
                var daysRemaining = holiday.EndDate.DayNumber - currentDate.DayNumber;
                
                return new ActiveHolidayState
                {
                    Id = holiday.Id,
                    Name = holiday.Name,
                    IsActive = true,
                    CurrentDay = currentDay,
                    DaysRemaining = daysRemaining,
                    IsRamadan = holiday.Id == HolidayId.Ramadan,
                    FoodCostModifier = holiday.FoodCostModifier,
                    StressModifier = holiday.StressModifier,
                    TrustModifierWithNeighbors = holiday.TrustModifierWithNeighbors,
                    MotherHealthModifier = holiday.MotherHealthModifier,
                    JobPayModifier = holiday.JobPayModifier,
                    CommunityEventAvailable = holiday.CommunityEventAvailable,
                    Description = holiday.Description
                };
            }
        }
        
        return ActiveHolidayState.None;
    }
    
    public static bool IsRamadanActive(DateOnly currentDate)
    {
        var ramadan = Holidays.FirstOrDefault(h => h.Id == HolidayId.Ramadan);
        return ramadan?.IsActiveOn(currentDate) ?? false;
    }
    
    public static int GetRamadanDay(DateOnly currentDate)
    {
        var ramadan = Holidays.FirstOrDefault(h => h.Id == HolidayId.Ramadan);
        if (ramadan is null || !ramadan.IsActiveOn(currentDate))
        {
            return 0;
        }
        
        return currentDate.DayNumber - ramadan.StartDate.DayNumber + 1;
    }
    
    public static int GetRamadanDaysRemaining(DateOnly currentDate)
    {
        var ramadan = Holidays.FirstOrDefault(h => h.Id == HolidayId.Ramadan);
        if (ramadan is null || !ramadan.IsActiveOn(currentDate))
        {
            return 0;
        }
        
        return ramadan.EndDate.DayNumber - currentDate.DayNumber;
    }
    
    public static Holiday? GetHoliday(HolidayId id) => Holidays.FirstOrDefault(h => h.Id == id);
}
