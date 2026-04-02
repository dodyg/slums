using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.State;

internal sealed class DebtSubsystemTests
{
    private static GameSession CreateSession(int money = 200)
    {
        var session = new GameSession();
        session.Player.Stats.SetMoney(money);
        session.NpcEconomies.Initialize();
        return session;
    }

    [Test]
    public async Task TryBorrowFromNpc_RejectsZeroAmount()
    {
        using var session = CreateSession();
        var (success, _, message) = session.TryBorrowFromNpc(NpcId.NeighborMona, 0);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromNpc_RejectsNegativeAmount()
    {
        using var session = CreateSession();
        var (success, _, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, -10);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromNpc_RejectsLowTrust()
    {
        using var session = CreateSession();
        var (success, _, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromNpc_SucceedsWithTrust()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);
        var (success, amount, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        await Assert.That(success).IsTrue();
        await Assert.That(amount).IsGreaterThan(0);
    }

    [Test]
    public async Task TryBorrowFromNpc_CapsAtMaxForGenerousNpc()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        var (_, amount, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, 100);
        await Assert.That(amount).IsEqualTo(50);
    }

    [Test]
    public async Task TryBorrowFromNpc_LandlordCapsHigher()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 20, 0);
        var (_, amount, _) = session.TryBorrowFromNpc(NpcId.LandlordHajjMahmoud, 100);
        await Assert.That(amount).IsEqualTo(100);
    }

    [Test]
    public async Task TryBorrowFromNpc_RejectsExistingDebt()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        var (success, _, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromNpc_RejectsStrugglingNpc()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        session.NpcEconomies.SetWealthLevel(NpcId.NeighborMona, NpcWealthLevel.Struggling);
        var (success, _, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromNpc_RefugeeUsesCommunityMutualAid()
    {
        using var session = CreateSession();
        session.Player.ApplyBackground(BackgroundRegistry.SudaneseRefugee);
        session.Relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);
        var (success, _, _) = session.TryBorrowFromNpc(NpcId.NurseSalma, 30);
        await Assert.That(success).IsTrue();
        var debt = session.PlayerDebts.Debts.FirstOrDefault(d => d.Source == DebtSource.CommunityMutualAid);
        await Assert.That(debt).IsNotNull();
    }

    [Test]
    public async Task TryBorrowFromNpc_MedicalDropoutGetsLongerDueDay()
    {
        using var session = CreateSession();
        session.Player.ApplyBackground(BackgroundRegistry.MedicalSchoolDropout);
        session.Relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);
        var (success, _, _) = session.TryBorrowFromNpc(NpcId.NurseSalma, 30);
        await Assert.That(success).IsTrue();
        var debt = session.PlayerDebts.Debts[0];
        await Assert.That(debt.DueDay).IsEqualTo(session.Clock.Day + 21);
    }

    [Test]
    public async Task TryBorrowFromNpc_AddsMoney()
    {
        using var session = CreateSession(money: 50);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        var (_, amount, _) = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        await Assert.That(session.Player.Stats.Money).IsEqualTo(50 + amount);
    }

    [Test]
    public async Task TryBorrowFromLandlord_RejectsLowTrust()
    {
        using var session = CreateSession();
        var (success, _, _) = session.TryBorrowFromLandlord(80);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromLandlord_SucceedsWithMinTrust()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 5, 0);
        var (success, amount, _) = session.TryBorrowFromLandlord(80);
        await Assert.That(success).IsTrue();
        await Assert.That(amount).IsEqualTo(80);
    }

    [Test]
    public async Task TryBorrowFromLandlord_ClampsAmountToRange()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 10, 0);
        var (_, amount, _) = session.TryBorrowFromLandlord(200);
        await Assert.That(amount).IsEqualTo(100);
    }

    [Test]
    public async Task TryBorrowFromLandlord_ClampsMinAmount()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 10, 0);
        var (_, amount, _) = session.TryBorrowFromLandlord(20);
        await Assert.That(amount).IsEqualTo(50);
    }

    [Test]
    public async Task TryBorrowFromLandlord_RejectsExistingDebt()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 10, 0);
        session.TryBorrowFromLandlord(60);
        var (success, _, _) = session.TryBorrowFromLandlord(60);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromLoanShark_Succeeds()
    {
        using var session = CreateSession();
        var (success, amount, _) = session.TryBorrowFromLoanShark(200);
        await Assert.That(success).IsTrue();
        await Assert.That(amount).IsEqualTo(200);
    }

    [Test]
    public async Task TryBorrowFromLoanShark_ClampsToMax()
    {
        using var session = CreateSession();
        var (_, amount, _) = session.TryBorrowFromLoanShark(500);
        await Assert.That(amount).IsEqualTo(300);
    }

    [Test]
    public async Task TryBorrowFromLoanShark_PrisonerMaxIs200()
    {
        using var session = CreateSession();
        session.Player.ApplyBackground(BackgroundRegistry.ReleasedPoliticalPrisoner);
        var (_, amount, _) = session.TryBorrowFromLoanShark(500);
        await Assert.That(amount).IsEqualTo(200);
    }

    [Test]
    public async Task TryBorrowFromLoanShark_ClampsMinTo100()
    {
        using var session = CreateSession();
        var (_, amount, _) = session.TryBorrowFromLoanShark(50);
        await Assert.That(amount).IsEqualTo(100);
    }

    [Test]
    public async Task TryBorrowFromLoanShark_RejectsExistingDebt()
    {
        using var session = CreateSession();
        session.TryBorrowFromLoanShark(100);
        var (success, _, _) = session.TryBorrowFromLoanShark(100);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryBorrowFromLoanShark_AddsDistrictHeat()
    {
        using var session = CreateSession();
        var heatBefore = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        session.TryBorrowFromLoanShark(100);
        var heatAfter = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        await Assert.That(heatAfter).IsEqualTo(heatBefore + 5);
    }

    [Test]
    public async Task TryLendToNpc_SucceedsWhenAffordable()
    {
        using var session = CreateSession(money: 100);
        var (success, _) = session.TryLendToNpc(NpcId.NeighborMona, 20);
        await Assert.That(success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(80);
    }

    [Test]
    public async Task TryLendToNpc_RejectsWhenNotEnoughMoney()
    {
        using var session = CreateSession(money: 10);
        var (success, _) = session.TryLendToNpc(NpcId.NeighborMona, 20);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryLendToNpc_GrantsTrustAndFavor()
    {
        using var session = CreateSession(money: 100);
        session.TryLendToNpc(NpcId.NeighborMona, 20);
        var rel = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.Trust).IsGreaterThan(0);
        await Assert.That(rel.WasHelped).IsTrue();
    }

    [Test]
    public async Task TryLendToNpc_RejectsZeroAmount()
    {
        using var session = CreateSession();
        var (success, _) = session.TryLendToNpc(NpcId.NeighborMona, 0);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RefuseNpcLoan_ReducesTrust()
    {
        using var session = CreateSession();
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 30, 0);
        var (success, _) = session.RefuseNpcLoan(NpcId.NeighborMona);
        await Assert.That(success).IsTrue();
        var rel = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.Trust).IsLessThan(30);
    }

    [Test]
    public async Task RepayDebt_FullRepaymentClearsDebtAndRestoresTrust()
    {
        using var session = CreateSession(money: 0);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        var borrowed = session.Player.Stats.Money;

        var (success, remaining, _) = session.RepayDebt(DebtSource.NeighborLoan, borrowed);
        await Assert.That(success).IsTrue();
        await Assert.That(remaining).IsEqualTo(0);
        await Assert.That(session.Relationships.GetNpcRelationship(NpcId.NeighborMona).HasUnpaidDebt).IsFalse();
    }

    [Test]
    public async Task RepayDebt_PartialRepaymentLeavesRemaining()
    {
        using var session = CreateSession(money: 0);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 50);
        var borrowed = session.Player.Stats.Money;
        session.Player.Stats.SetMoney(borrowed / 2);

        var (success, remaining, _) = session.RepayDebt(DebtSource.NeighborLoan, borrowed / 2);
        await Assert.That(success).IsTrue();
        await Assert.That(remaining).IsGreaterThan(0);
    }

    [Test]
    public async Task RepayDebt_RejectsInvalidAmount()
    {
        using var session = CreateSession();
        var (success, _, _) = session.RepayDebt(DebtSource.NeighborLoan, 0);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RepayDebt_RejectsWhenNoDebtExists()
    {
        using var session = CreateSession();
        var (success, _, _) = session.RepayDebt(DebtSource.NeighborLoan, 50);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RepayDebt_RejectsWhenNotEnoughMoney()
    {
        using var session = CreateSession(money: 200);
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 50);
        session.Player.Stats.SetMoney(0);

        var (success, _, _) = session.RepayDebt(DebtSource.NeighborLoan, 50);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task RepayDebt_LoanSharkFullRepaymentReducesHeat()
    {
        using var session = CreateSession(money: 300);
        session.TryBorrowFromLoanShark(100);
        var heatBefore = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);

        session.RepayDebt(DebtSource.LoanShark, 100);
        var heatAfter = session.DistrictHeat.GetHeat(session.World.CurrentDistrict);
        await Assert.That(heatAfter).IsEqualTo(heatBefore - 3);
    }
}
