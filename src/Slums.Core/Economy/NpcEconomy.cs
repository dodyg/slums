using Slums.Core.Relationships;

namespace Slums.Core.Economy;

public sealed record NpcEconomy
{
    public NpcId Npc { get; init; }
    public NpcWealthLevel WealthLevel { get; init; }
    public int Generosity { get; init; }
    public Dictionary<DebtorId, int> MoneyOwedTo { get; init; } = [];
    public Dictionary<DebtorId, int> MoneyOwedBy { get; init; } = [];
    public int LastHardshipDay { get; init; }
    public int LastWindfallDay { get; init; }
    public int GenerousUntilDay { get; init; }

    public NpcEconomy WithHardship(int day)
    {
        return this with
        {
            WealthLevel = (NpcWealthLevel)Math.Max(0, (int)WealthLevel - 1),
            LastHardshipDay = day
        };
    }

    public NpcEconomy WithWindfall(int day, int generousUntil)
    {
        return this with
        {
            WealthLevel = (NpcWealthLevel)Math.Min(3, (int)WealthLevel + 1),
            LastWindfallDay = day,
            GenerousUntilDay = generousUntil
        };
    }
}
