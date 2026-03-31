using Slums.Core.Economy;
using Slums.Core.Relationships;
using TUnit.Core;

namespace Slums.Core.Tests.Economy;

internal sealed class PlayerDebtTests
{
    [Test]
    public async Task PlayerDebt_DaysOverdue_ReturnsCorrectDays()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 10,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        };

        await Assert.That(debt.DaysOverdue(12)).IsEqualTo(2);
        await Assert.That(debt.DaysOverdue(10)).IsEqualTo(0);
        await Assert.That(debt.DaysOverdue(8)).IsEqualTo(0);
    }

    [Test]
    public async Task PlayerDebt_IsOverdue_ReturnsTrueWhenPastDue()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 10,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        };

        await Assert.That(debt.IsOverdue(11)).IsTrue();
        await Assert.That(debt.IsOverdue(10)).IsFalse();
    }

    [Test]
    public async Task PlayerDebt_WithRepayment_ReducesAmount()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        };

        var result = debt.WithRepayment(50);
        await Assert.That(result.AmountOwed).IsEqualTo(150);
    }

    [Test]
    public async Task PlayerDebt_WithRepayment_ClampsToZero()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 20,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        };

        var result = debt.WithRepayment(100);
        await Assert.That(result.AmountOwed).IsEqualTo(0);
    }

    [Test]
    public async Task PlayerDebt_WithInterest_AddsToAmount()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        };

        var result = debt.WithInterest(50);
        await Assert.That(result.AmountOwed).IsEqualTo(250);
    }

    [Test]
    public async Task PlayerDebt_WithEscalation_UpdatesCollectionState()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        };

        var result = debt.WithEscalation(DebtCollectionState.Escalating);
        await Assert.That(result.CollectionState).IsEqualTo(DebtCollectionState.Escalating);
    }

    [Test]
    public async Task PlayerDebtState_AddDebt_TracksDebt()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1,
            CreditorNpcId = (int)NpcId.NeighborMona
        });

        await Assert.That(state.Debts.Count).IsEqualTo(1);
        await Assert.That(state.Debts[0].Source).IsEqualTo(DebtSource.NeighborLoan);
    }

    [Test]
    public async Task PlayerDebtState_RepayPartial_ReducesDebt()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 50,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });

        state.RepayPartial(DebtSource.NeighborLoan, 20);

        await Assert.That(state.Debts[0].AmountOwed).IsEqualTo(30);
    }

    [Test]
    public async Task PlayerDebtState_RepayPartial_RemovesDebtWhenPaid()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 50,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });

        state.RepayPartial(DebtSource.NeighborLoan, 50);

        await Assert.That(state.Debts.Count).IsEqualTo(0);
    }

    [Test]
    public async Task PlayerDebtState_GetOverdueDebts_ReturnsOnlyOverdue()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 5,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 20,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 13
        });

        var overdue = state.GetOverdueDebts(10);
        await Assert.That(overdue.Count).IsEqualTo(1);
        await Assert.That(overdue[0].Source).IsEqualTo(DebtSource.NeighborLoan);
    }

    [Test]
    public async Task PlayerDebtState_ProcessInterest_AppliesLoanSharkInterest()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        });
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });

        state.ProcessInterest(14);

        await Assert.That(state.Debts[0].AmountOwed).IsGreaterThan(200);
        await Assert.That(state.Debts[1].AmountOwed).IsEqualTo(30);
    }

    [Test]
    public async Task PlayerDebtState_UpdateCollectionStates_EscalatesOverdue()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        });

        state.UpdateCollectionStates(16);
        await Assert.That(state.Debts[0].CollectionState).IsEqualTo(DebtCollectionState.Overdue);

        state.UpdateCollectionStates(22);
        await Assert.That(state.Debts[0].CollectionState).IsEqualTo(DebtCollectionState.Escalating);

        state.UpdateCollectionStates(28);
        await Assert.That(state.Debts[0].CollectionState).IsEqualTo(DebtCollectionState.Critical);
    }

    [Test]
    public async Task PlayerDebtState_RestoreDebts_ReplacesAllDebts()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });

        var newDebts = new[]
        {
            new PlayerDebt
            {
                Source = DebtSource.LoanShark,
                AmountOwed = 100,
                InterestWeeklyBasisPoints = 2000,
                DueDay = 21,
                CollectionState = DebtCollectionState.Overdue,
                OriginDay = 14
            }
        };

        state.RestoreDebts(newDebts);
        await Assert.That(state.Debts.Count).IsEqualTo(1);
        await Assert.That(state.Debts[0].Source).IsEqualTo(DebtSource.LoanShark);
    }

    [Test]
    public async Task LoanSharkEscalation_ApplyDailyPenalty_Phase1()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Overdue,
            OriginDay = 7
        };

        var result = LoanSharkEscalation.ApplyDailyPenalty(debt, 16);
        await Assert.That(result.Stress).IsEqualTo(5);
        await Assert.That(result.Health).IsEqualTo(0);
    }

    [Test]
    public async Task LoanSharkEscalation_ApplyDailyPenalty_Phase2()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Escalating,
            OriginDay = 7
        };

        var result = LoanSharkEscalation.ApplyDailyPenalty(debt, 22);
        await Assert.That(result.Stress).IsEqualTo(8);
        await Assert.That(result.Health).IsEqualTo(-5);
    }

    [Test]
    public async Task LoanSharkEscalation_ShouldTriggerViolence_AfterFourteenDaysOverdue()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Critical,
            OriginDay = 7
        };

        await Assert.That(LoanSharkEscalation.ShouldTriggerViolence(debt, 28)).IsTrue();
        await Assert.That(LoanSharkEscalation.ShouldTriggerViolence(debt, 27)).IsFalse();
    }

    [Test]
    public async Task LoanSharkEscalation_ApplyDailyPenalty_NotOverdue()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        };

        var result = LoanSharkEscalation.ApplyDailyPenalty(debt, 13);
        await Assert.That(result.Stress).IsEqualTo(0);
    }

    [Test]
    public async Task LoanSharkEscalation_NonLoanShark_ReturnsZero()
    {
        var debt = new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 10,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        };

        var result = LoanSharkEscalation.ApplyDailyPenalty(debt, 15);
        await Assert.That(result.Stress).IsEqualTo(0);
    }

    [Test]
    public async Task PlayerDebtState_RepayDebtAt_RepaySpecificDebt()
    {
        var state = new PlayerDebtState();
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.NeighborLoan,
            AmountOwed = 30,
            InterestWeeklyBasisPoints = 0,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 1
        });
        state.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Current,
            OriginDay = 7
        });

        state.RepayDebtAt(0, 10);
        await Assert.That(state.Debts[0].AmountOwed).IsEqualTo(20);
        await Assert.That(state.Debts[1].AmountOwed).IsEqualTo(200);
    }

    [Test]
    public async Task PlayerDebtState_AddDebt_ThrowsOnNull()
    {
        var state = new PlayerDebtState();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => state.AddDebt(null!));
    }
}
