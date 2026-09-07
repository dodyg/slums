namespace Slums.Infrastructure.Persistence;

public sealed class NpcEconomyEntrySnapshot
{
    public string Npc { get; init; } = string.Empty;
    public string WealthLevel { get; init; } = "Stable";
    public int Generosity { get; init; }
    public IReadOnlyList<DebtorAmountSnapshot> MoneyOwedTo { get; init; } = [];
    public IReadOnlyList<DebtorAmountSnapshot> MoneyOwedBy { get; init; } = [];
    public int LastHardshipDay { get; init; }
    public int LastWindfallDay { get; init; }
    public int GenerousUntilDay { get; init; }
}
