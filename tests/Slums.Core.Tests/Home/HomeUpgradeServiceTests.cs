using Slums.Core.Home;
using Slums.Core.State;
using Slums.Core.World;
using TUnit;

namespace Slums.Core.Tests.Home;

internal sealed class HomeUpgradeServiceTests
{
    [Test]
    public async Task Purchase_ShouldApplyUpgradeAndKeepSessionDiagnostics()
    {
        var session = new GameSession();
        var moneyBefore = session.Player.Stats.Money;

        var result = HomeUpgradeService.Purchase(session, HomeUpgrade.CleanBedding);

        await Assert.That(result).IsTrue();
        await Assert.That(session.HomeUpgrades.HasUpgrade(HomeUpgrade.CleanBedding)).IsTrue();
        await Assert.That(session.Player.Stats.Money).IsEqualTo(moneyBefore - HomeUpgradeDefinitions.GetCost(HomeUpgrade.CleanBedding));
        await Assert.That(session.Mutations[^1].Action).IsEqualTo("TryPurchaseHomeUpgrade");
    }

    [Test]
    public async Task RestAtHome_ShouldRejectWhenSessionIsAwayFromHome()
    {
        var session = new GameSession();
        session.World.TravelTo(LocationId.Market);

        var result = HomeUpgradeService.RestAtHome(session);

        await Assert.That(result).IsFalse();
        await Assert.That(session.Mutations[^1].Category).IsEqualTo("GuardRejected");
        await Assert.That(session.EventJournal.Entries[^1].Message).IsEqualTo("You need to go home to rest.");
    }
}
