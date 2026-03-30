namespace Slums.Core.Calendar;

public sealed record RamadanState
{
    public bool IsActive { get; init; }
    public bool PlayerIsFasting { get; init; }
    public int DaysFasting { get; init; }
    public int DaysRemaining { get; init; }
    
    public static RamadanState Inactive { get; } = new()
    {
        IsActive = false,
        PlayerIsFasting = false,
        DaysFasting = 0,
        DaysRemaining = 0
    };
    
    public RamadanState WithFastingChoice(bool isFasting) => this with { PlayerIsFasting = isFasting };
    
    public RamadanState AdvanceDay()
    {
        if (!IsActive)
        {
            return this;
        }
        
        var newDaysFasting = PlayerIsFasting ? DaysFasting + 1 : DaysFasting;
        var newDaysRemaining = Math.Max(0, DaysRemaining - 1);
        
        return this with
        {
            DaysFasting = newDaysFasting,
            DaysRemaining = newDaysRemaining
        };
    }
    
    public int EnergyModifier => IsActive && PlayerIsFasting ? -5 : 0;
    public int StressModifier => IsActive && PlayerIsFasting ? 3 : (IsActive && !PlayerIsFasting ? 2 : 0);
    public int TrustModifierWithReligiousNpcs => IsActive && PlayerIsFasting ? 1 : 0;
    public int JobPayModifierPercent => IsActive && PlayerIsFasting ? -10 : 0;
}
