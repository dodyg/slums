namespace Slums.Core.Calendar;

public sealed record ActiveHolidayState
{
    public HolidayId Id { get; init; }
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public int CurrentDay { get; init; }
    public int DaysRemaining { get; init; }
    public bool IsRamadan { get; init; }
    
    public int? FoodCostModifier { get; init; }
    public int? StressModifier { get; init; }
    public int? TrustModifierWithNeighbors { get; init; }
    public int? MotherHealthModifier { get; init; }
    public int? JobPayModifier { get; init; }
    public bool? CommunityEventAvailable { get; init; }
    public string? Description { get; init; }
    
    public static ActiveHolidayState None { get; } = new()
    {
        Id = HolidayId.None,
        Name = null,
        IsActive = false,
        CurrentDay = 0,
        DaysRemaining = 0,
        IsRamadan = false
    };
}
