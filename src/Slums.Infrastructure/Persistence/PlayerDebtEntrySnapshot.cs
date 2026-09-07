namespace Slums.Infrastructure.Persistence;

public sealed class PlayerDebtEntrySnapshot
{
    public string Source { get; init; } = "NeighborLoan";
    public int AmountOwed { get; init; }
    public int InterestWeeklyBasisPoints { get; init; }
    public int DueDay { get; init; }
    public string CollectionState { get; init; } = "Current";
    public int OriginDay { get; init; }
    public int? CreditorNpcId { get; init; }
}
