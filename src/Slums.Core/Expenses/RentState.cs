namespace Slums.Core.Expenses;

public sealed class RentState
{
    public const int EvictionThreshold = 7;
    public const int FirstWarningDay = 3;
    public const int FinalWarningDay = 5;

    private int _unpaidRentDays;
    private bool _firstWarningGiven;
    private bool _finalWarningGiven;
    private int _accumulatedDebt;
    private int _graceDaysRemaining;

    public int UnpaidRentDays => _unpaidRentDays;
    public int AccumulatedRentDebt => _accumulatedDebt;
    public bool FirstWarningGiven => _firstWarningGiven;
    public bool FinalWarningGiven => _finalWarningGiven;
    public int GraceDaysRemaining => _graceDaysRemaining;

    public RentResult ProcessDay(int dailyRentCost, int playerMoney)
    {
        var result = new RentResult();

        if (_graceDaysRemaining > 0)
        {
            _graceDaysRemaining--;
            return result with { GraceApplied = true, GraceDaysRemaining = _graceDaysRemaining };
        }

        if (playerMoney >= dailyRentCost)
        {
            result = result with { Paid = true, AmountPaid = dailyRentCost };
            ResetWarnings();
        }
        else
        {
            _unpaidRentDays++;
            _accumulatedDebt += dailyRentCost;
            result = result with
            {
                Paid = false,
                CurrentUnpaidDays = _unpaidRentDays,
                AccumulatedDebt = _accumulatedDebt
            };

            if (_unpaidRentDays == FirstWarningDay && !_firstWarningGiven)
            {
                _firstWarningGiven = true;
                result = result with { WarningType = RentWarningType.First };
            }
            else if (_unpaidRentDays == FinalWarningDay && !_finalWarningGiven)
            {
                _finalWarningGiven = true;
                result = result with { WarningType = RentWarningType.Final };
            }
            else if (_unpaidRentDays >= EvictionThreshold)
            {
                result = result with { WarningType = RentWarningType.Eviction };
            }
        }

        return result;
    }

    public void ResetWarnings()
    {
        _unpaidRentDays = 0;
        _firstWarningGiven = false;
        _finalWarningGiven = false;
    }

    public void PayPartialDebt(int amount)
    {
        _accumulatedDebt = Math.Max(0, _accumulatedDebt - amount);
    }

    public void AddGraceDays(int days)
    {
        if (days <= 0)
        {
            return;
        }

        _graceDaysRemaining = Math.Min(14, _graceDaysRemaining + days);
    }

    public void Restore(int unpaidRentDays, int accumulatedDebt, bool firstWarningGiven, bool finalWarningGiven)
    {
        _unpaidRentDays = Math.Max(0, unpaidRentDays);
        _accumulatedDebt = Math.Max(0, accumulatedDebt);
        _firstWarningGiven = firstWarningGiven;
        _finalWarningGiven = finalWarningGiven;
    }

    public void RestoreGraceDays(int graceDays)
    {
        _graceDaysRemaining = Math.Clamp(graceDays, 0, 14);
    }
}

public enum RentWarningType
{
    None,
    First,
    Final,
    Eviction
}

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
