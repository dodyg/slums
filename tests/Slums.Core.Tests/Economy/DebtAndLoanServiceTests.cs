using FluentAssertions;
using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Economy;

internal sealed class DebtAndLoanServiceTests
{
    [Test]
    public void ApplyRentPayment_ShouldCapPaymentAtAvailableArrearsAndMoney()
    {
        var session = new GameSession();
        session.RestoreRentState(1, 40, false, false);

        DebtAndLoanService.ApplyRentPayment(session, 100);

        session.AccumulatedRentDebt.Should().Be(0);
        session.Player.Stats.Money.Should().Be(60);
        session.EventJournal.Entries[^1].Message.Should().Contain("Paid 40 LE toward rent arrears.");
    }

    [Test]
    public void BorrowFromNpc_ShouldPreserveDebtAndMutationRecording()
    {
        var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);

        var result = DebtAndLoanService.BorrowFromNpc(session, NpcId.NeighborMona, 30);

        result.Success.Should().BeTrue();
        session.PlayerDebts.Debts.Should().ContainSingle();
        session.Mutations[^1].Action.Should().Be("TryBorrowFromNpc");
    }

    [Test]
    public void LendToNpc_ShouldUpdateRelationshipAndEconomyState()
    {
        var session = new GameSession();
        var moneyBefore = session.Player.Stats.Money;

        var result = DebtAndLoanService.LendToNpc(session, NpcId.NeighborMona, 20);

        result.Success.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(moneyBefore - 20);
        session.Relationships.GetNpcRelationship(NpcId.NeighborMona).WasHelped.Should().BeTrue();
    }
}
