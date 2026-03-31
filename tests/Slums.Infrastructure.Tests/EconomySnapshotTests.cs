using Slums.Core.Characters;
using Slums.Core.Economy;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Infrastructure.Persistence;
using TUnit.Core;

namespace Slums.Infrastructure.Tests;

internal sealed class EconomySnapshotTests
{
    [Test]
    public async Task EconomySnapshot_CaptureAndRestore_PreservesNpcEconomies()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.NpcEconomies.SetWealthLevel(NpcId.LandlordHajjMahmoud, NpcWealthLevel.Poor);
        session.NpcEconomies.SetWealthLevel(NpcId.NeighborMona, NpcWealthLevel.Comfortable);

        var snapshot = GameSessionEconomySnapshot.Capture(session);

        await Assert.That(snapshot.NpcEconomies).IsNotEmpty();

        using var restored = new GameSession(new Random(42));
        restored.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        snapshot.Restore(restored);

        await Assert.That(restored.NpcEconomies.GetEconomy(NpcId.LandlordHajjMahmoud).WealthLevel).IsEqualTo(NpcWealthLevel.Poor);
        await Assert.That(restored.NpcEconomies.GetEconomy(NpcId.NeighborMona).WealthLevel).IsEqualTo(NpcWealthLevel.Comfortable);
    }

    [Test]
    public async Task EconomySnapshot_CaptureAndRestore_PreservesPlayerDebts()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.PlayerDebts.AddDebt(new PlayerDebt
        {
            Source = DebtSource.LoanShark,
            AmountOwed = 200,
            InterestWeeklyBasisPoints = 2500,
            DueDay = 14,
            CollectionState = DebtCollectionState.Overdue,
            OriginDay = 7
        });

        var snapshot = GameSessionEconomySnapshot.Capture(session);

        using var restored = new GameSession(new Random(42));
        restored.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        snapshot.Restore(restored);

        await Assert.That(restored.PlayerDebts.Debts).Count().IsEqualTo(1);
        await Assert.That(restored.PlayerDebts.Debts[0].Source).IsEqualTo(DebtSource.LoanShark);
        await Assert.That(restored.PlayerDebts.Debts[0].AmountOwed).IsEqualTo(200);
        await Assert.That(restored.PlayerDebts.Debts[0].CollectionState).IsEqualTo(DebtCollectionState.Overdue);
    }

    [Test]
    public async Task EconomySnapshot_CaptureAndRestore_PreservesNpcToNpcDebt()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        var from = DebtorId.FromNpc(NpcId.NeighborMona);
        var to = DebtorId.FromNpc(NpcId.LandlordHajjMahmoud);
        session.NpcEconomies.AddDebt(from, to, 40);

        var snapshot = GameSessionEconomySnapshot.Capture(session);

        using var restored = new GameSession(new Random(42));
        restored.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        snapshot.Restore(restored);

        var monaEcon = restored.NpcEconomies.GetEconomy(NpcId.NeighborMona);
        await Assert.That(monaEcon.MoneyOwedTo.Values.Sum()).IsEqualTo(40);
    }

    [Test]
    public async Task EconomySnapshot_CaptureAndRestore_FullRoundTrip()
    {
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));
        session.Relationships.SetNpcRelationship(NpcId.NeighborMona, 15, 0);
        session.TryBorrowFromNpc(NpcId.NeighborMona, 30);
        session.NpcEconomies.SetWealthLevel(NpcId.FixerUmmKarim, NpcWealthLevel.Comfortable);

        var fullSnapshot = GameSessionSnapshot.Capture(session);

        using var restored = fullSnapshot.Restore();

        await Assert.That(restored.NpcEconomies.GetEconomy(NpcId.FixerUmmKarim).WealthLevel).IsEqualTo(NpcWealthLevel.Comfortable);
        await Assert.That(restored.PlayerDebts.Debts).Count().IsEqualTo(1);
        await Assert.That(restored.PlayerDebts.Debts[0].AmountOwed).IsGreaterThan(0);
    }

    [Test]
    public async Task EconomySnapshot_EmptySnapshot_RestoresGracefully()
    {
        var snapshot = new GameSessionEconomySnapshot();
        using var session = new GameSession(new Random(42));
        session.Player.ApplyBackground(BackgroundRegistry.GetByType(BackgroundType.SudaneseRefugee));

        snapshot.Restore(session);

        await Assert.That(session.PlayerDebts.Debts).IsEmpty();
    }
}
