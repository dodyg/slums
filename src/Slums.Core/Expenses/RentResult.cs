namespace Slums.Core.Expenses;

public sealed record RentResult
{
    public bool Paid { get; init; }
    public int AmountPaid { get; init; }
    public int CurrentUnpaidDays { get; init; }
    public int AccumulatedDebt { get; init; }
    public RentWarningType WarningType { get; init; }
    public bool GraceApplied { get; init; }
    public int GraceDaysRemaining { get; init; }

    public static RentResult Empty => new();
}
