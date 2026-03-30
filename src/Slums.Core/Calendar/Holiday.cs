namespace Slums.Core.Calendar;

public sealed record Holiday
{
    public HolidayId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public int DurationDays { get; init; }
    public string? Description { get; init; }
    
    public int? FoodCostModifier { get; init; }
    public int? StressModifier { get; init; }
    public int? TrustModifierWithNeighbors { get; init; }
    public int? MotherHealthModifier { get; init; }
    public int? JobPayModifier { get; init; }
    public bool? CommunityEventAvailable { get; init; }
    
    public DateOnly EndDate => StartDate.AddDays(DurationDays - 1);
    
    public bool IsActiveOn(DateOnly date) => date >= StartDate && date <= EndDate;
}
