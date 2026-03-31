namespace Slums.Core.Economy;

public sealed record PlayerDebt
{
    public DebtSource Source { get; init; }
    public int AmountOwed { get; init; }
    public int InterestWeeklyBasisPoints { get; init; }
    public int DueDay { get; init; }
    public DebtCollectionState CollectionState { get; init; }
    public int OriginDay { get; init; }
    public int? CreditorNpcId { get; init; }

    public int DaysOverdue(int currentDay) => Math.Max(0, currentDay - DueDay);

    public bool IsOverdue(int currentDay) => currentDay > DueDay;

    public PlayerDebt WithRepayment(int amount)
    {
        var remaining = Math.Max(0, AmountOwed - amount);
        return this with { AmountOwed = remaining };
    }

    public PlayerDebt WithInterest(int weeklyInterest)
    {
        return this with { AmountOwed = AmountOwed + weeklyInterest };
    }

    public PlayerDebt WithEscalation(DebtCollectionState newState)
    {
        return this with { CollectionState = newState };
    }
}
