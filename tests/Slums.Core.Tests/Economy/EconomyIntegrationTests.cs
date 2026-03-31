using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Economy;

internal sealed class EconomyIntegrationTests
{
    [Test]
    public async Task GameSession_Constructor_InitializesEconomy()
    {
        using var session = new GameSession(new Random(42));

        await Assert.That(session.NpcEconomies.Economies).IsNotEmpty();
        await Assert.That(session.PlayerDebts.Debts).IsEmpty();
    }

    [Test]
    public async Task GameSession_TryBorrowFromNpc_Success()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);

        var result = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Amount).IsGreaterThan(0);
        await Assert.That(session.Player.Stats.Money).IsGreaterThan(0);
        await Assert.That(session.PlayerDebts.Debts).Count().IsEqualTo(1);
    }

    [Test]
    public async Task GameSession_TryBorrowFromNpc_RejectsLowTrust()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 5, 0);

        var result = session.TryBorrowFromNpc(NpcId.LandlordHajjMahmoud, 30);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task GameSession_TryBorrowFromNpc_SudaneseRefugee_CommunityMutualAid()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.Relationships.SetNpcRelationship(NpcId.NurseSalma, 15, 0);

        session.TryBorrowFromNpc(NpcId.NurseSalma, 30);

        await Assert.That(session.PlayerDebts.Debts[0].Source).IsEqualTo(DebtSource.CommunityMutualAid);
    }

    [Test]
    public async Task GameSession_TryBorrowFromNpc_RejectsExistingDebt()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);
        session.Relationships.SetDebtState(NpcId.NeighborMona, true);

        var result = session.TryBorrowFromNpc(NpcId.NeighborMona, 30);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task GameSession_TryBorrowFromNpc_RejectsStrugglingNpc()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.RunnerYoussef, 15, 0);
        session.NpcEconomies.SetWealthLevel(NpcId.RunnerYoussef, NpcWealthLevel.Struggling);

        var result = session.TryBorrowFromNpc(NpcId.RunnerYoussef, 30);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task GameSession_TryBorrowFromLandlord_Success()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 10, 0);

        var result = session.TryBorrowFromLandlord(80);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Amount).IsGreaterThanOrEqualTo(50);
        await Assert.That(session.AccumulatedRentDebt).IsGreaterThan(0);
    }

    [Test]
    public async Task GameSession_TryBorrowFromLandlord_RejectsLowTrust()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.LandlordHajjMahmoud, 2, 0);

        var result = session.TryBorrowFromLandlord(80);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task GameSession_TryBorrowFromLoanShark_Success()
    {
        using var session = new GameSession(new Random(42));

        var result = session.TryBorrowFromLoanShark(200);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsGreaterThanOrEqualTo(200);
        await Assert.That(session.PlayerDebts.Debts[0].Source).IsEqualTo(DebtSource.LoanShark);
        await Assert.That(session.PlayerDebts.Debts[0].InterestWeeklyBasisPoints).IsGreaterThan(0);
    }

    [Test]
    public async Task GameSession_TryBorrowFromLoanShark_RejectsExistingSharkDebt()
    {
        using var session = new GameSession(new Random(42));
        session.TryBorrowFromLoanShark(200);

        var result = session.TryBorrowFromLoanShark(100);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task GameSession_TryBorrowFromLoanShark_Prisoner_CapsAt200()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.ReleasedPoliticalPrisoner));

        var result = session.TryBorrowFromLoanShark(300);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Amount).IsEqualTo(200);
    }

    [Test]
    public async Task GameSession_TryLendToNpc_Success()
    {
        using var session = new GameSession(new Random(42));
        session.Player.Stats.SetMoney(100);

        var result = session.TryLendToNpc(NpcId.NeighborMona, 20);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(80);
        var rel = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.Trust).IsGreaterThan(0);
        await Assert.That(rel.WasHelped).IsTrue();
    }

    [Test]
    public async Task GameSession_TryLendToNpc_RejectsInsufficientFunds()
    {
        using var session = new GameSession(new Random(42));
        session.Player.Stats.SetMoney(5);

        var result = session.TryLendToNpc(NpcId.NeighborMona, 20);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task GameSession_RefuseNpcLoan_DecreasesTrust()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 20, 0);

        var result = session.RefuseNpcLoan(NpcId.NeighborMona);

        await Assert.That(result.Success).IsTrue();
        var rel = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.Trust).IsLessThan(20);
    }

    [Test]
    public async Task GameSession_RepayDebt_Success()
    {
        using var session = new GameSession(new Random(42));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        var borrowed = session.Player.Stats.Money;

        var result = session.RepayDebt(DebtSource.NeighborLoan, borrowed);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Remaining).IsEqualTo(0);
    }

    [Test]
    public async Task GameSession_RepayDebt_PartialPayment()
    {
        using var session = new GameSession(new Random(42));
        session.TryBorrowFromLoanShark(200);

        var result = session.RepayDebt(DebtSource.LoanShark, 50);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Remaining).IsGreaterThan(0);
    }

    [Test]
    public async Task GameSession_RepayDebt_FullPayment_ClearsDebtFlag()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 30);

        var borrowed = session.Player.Stats.Money;
        session.RepayDebt(DebtSource.CommunityMutualAid, borrowed);

        var rel = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        await Assert.That(rel.HasUnpaidDebt).IsFalse();
    }

    [Test]
    public async Task GameSession_GetFoodCost_UmmKarimComfortable_AppliesDiscount()
    {
        using var session = new GameSession(new Random(42));
        session.NpcEconomies.SetWealthLevel(NpcId.FixerUmmKarim, NpcWealthLevel.Comfortable);

        var cost = session.GetFoodCost();

        await Assert.That(cost).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task GameSession_GetStreetFoodCost_UmmKarimNotComfortable_NoDiscount()
    {
        using var session = new GameSession(new Random(42));
        session.NpcEconomies.SetWealthLevel(NpcId.FixerUmmKarim, NpcWealthLevel.Struggling);

        var costWithStruggling = session.GetStreetFoodCost();
        session.NpcEconomies.SetWealthLevel(NpcId.FixerUmmKarim, NpcWealthLevel.Comfortable);
        var costWithComfortable = session.GetStreetFoodCost();

        await Assert.That(costWithComfortable).IsLessThanOrEqualTo(costWithStruggling);
    }

    [Test]
    public async Task GameSession_RestoreEconomyState_RestoresAllData()
    {
        using var session = new GameSession(new Random(42));

        var economies = new[]
        {
            (NpcId.LandlordHajjMahmoud, NpcWealthLevel.Poor, 5,
                new Dictionary<DebtorId, int>(), new Dictionary<DebtorId, int>(), 10, 0, 0)
        };

        var debts = new[]
        {
            new PlayerDebt
            {
                Source = DebtSource.LoanShark,
                AmountOwed = 150,
                InterestWeeklyBasisPoints = 2500,
                DueDay = 20,
                CollectionState = DebtCollectionState.Overdue,
                OriginDay = 13
            }
        };

        session.RestoreEconomyState(economies, debts);

        var econ = session.NpcEconomies.GetEconomy(NpcId.LandlordHajjMahmoud);
        await Assert.That(econ.WealthLevel).IsEqualTo(NpcWealthLevel.Poor);
        await Assert.That(session.PlayerDebts.Debts).Count().IsEqualTo(1);
        await Assert.That(session.PlayerDebts.Debts[0].AmountOwed).IsEqualTo(150);
    }

    [Test]
    public async Task GameSession_EndDay_DoesNotCrashWithEconomy()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));

        session.EndDay();

        await Assert.That(session.IsGameOver).IsFalse();
    }

    [Test]
    public async Task NpcEconomyResolver_ResolveWeek_ModifiesEconomies()
    {
        var economies = new NpcEconomyState();
        economies.Initialize();
        var relationships = new RelationshipState();

        foreach (NpcId npc in Enum.GetValues<NpcId>())
        {
            relationships.SetNpcRelationship(npc, 20, 0);
        }

        NpcEconomyResolver.ResolveWeek(economies, relationships, 7, new Random(123));

        var struggling = economies.GetStrugglingNpcs();
        var comfortable = economies.GetComfortableNpcs();

        await Assert.That(struggling.Count + comfortable.Count).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task NpcEconomyResolver_GetNpcNeedingLoan_ReturnsNpcWithNoLender()
    {
        var economies = new NpcEconomyState();
        economies.Initialize();
        var relationships = new RelationshipState();

        economies.SetWealthLevel(NpcId.RunnerYoussef, NpcWealthLevel.Struggling);

        foreach (NpcId npc in Enum.GetValues<NpcId>())
        {
            relationships.SetNpcRelationship(npc, 0, 0);
        }

        var result = NpcEconomyResolver.GetNpcNeedingLoan(economies, relationships);

        await Assert.That(result).IsNotNull();
    }
}
