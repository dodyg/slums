namespace Slums.Core.Economy;

public sealed class PlayerDebtState
{
    private readonly List<PlayerDebt> _debts = [];

    public IReadOnlyList<PlayerDebt> Debts => _debts;

    public void AddDebt(PlayerDebt debt)
    {
        ArgumentNullException.ThrowIfNull(debt);
        _debts.Add(debt);
    }

    public void RepayPartial(DebtSource source, int amount)
    {
        for (var i = 0; i < _debts.Count; i++)
        {
            if (_debts[i].Source == source)
            {
                _debts[i] = _debts[i].WithRepayment(amount);
                if (_debts[i].AmountOwed <= 0)
                {
                    _debts.RemoveAt(i);
                }
                return;
            }
        }
    }

    public void RepayDebtAt(int index, int amount)
    {
        if (index < 0 || index >= _debts.Count)
        {
            return;
        }

        _debts[index] = _debts[index].WithRepayment(amount);
        if (_debts[index].AmountOwed <= 0)
        {
            _debts.RemoveAt(index);
        }
    }

    public IReadOnlyList<PlayerDebt> GetOverdueDebts(int currentDay)
    {
        return _debts.Where(d => d.IsOverdue(currentDay)).ToArray();
    }

    public void ProcessInterest(int currentDay)
    {
        for (var i = 0; i < _debts.Count; i++)
        {
            if (_debts[i].Source == DebtSource.LoanShark && _debts[i].InterestWeeklyBasisPoints > 0)
            {
                var interest = _debts[i].AmountOwed * _debts[i].InterestWeeklyBasisPoints / 10000;
                _debts[i] = _debts[i].WithInterest(Math.Max(1, interest));
            }
        }
    }

    public void UpdateCollectionStates(int currentDay)
    {
        for (var i = 0; i < _debts.Count; i++)
        {
            if (_debts[i].Source != DebtSource.LoanShark)
            {
                continue;
            }

            var daysOverdue = _debts[i].DaysOverdue(currentDay);
            var newState = daysOverdue switch
            {
                >= 14 => DebtCollectionState.Critical,
                >= 8 => DebtCollectionState.Escalating,
                > 0 => DebtCollectionState.Overdue,
                _ => DebtCollectionState.Current
            };

            if (newState != _debts[i].CollectionState)
            {
                _debts[i] = _debts[i].WithEscalation(newState);
            }
        }
    }

    public void RestoreDebts(IEnumerable<PlayerDebt> debts)
    {
        _debts.Clear();
        _debts.AddRange(debts);
    }
}
