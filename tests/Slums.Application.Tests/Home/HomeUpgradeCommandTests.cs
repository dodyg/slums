using FluentAssertions;
using Slums.Application.Home;
using Slums.Core.Home;
using Slums.Core.State;
using Slums.Core.World;
using TUnit;

namespace Slums.Application.Tests.Home;

internal sealed class HomeUpgradeCommandTests
{
    [Test]
    public void Execute_PurchasesUpgrade_WhenAtHomeWithMoney()
    {
        var command = new HomeUpgradeCommand();
        var session = new GameSession();
        session.World.TravelTo(LocationId.Home);
        session.Player.Stats.SetMoney(100);

        var result = command.Execute(session, HomeUpgrade.Curtain);

        result.Should().BeTrue();
        session.HomeUpgrades.PurchasedUpgrades.Should().Contain(HomeUpgrade.Curtain);
        session.Player.Stats.Money.Should().Be(85);
    }

    [Test]
    public void Execute_ReturnsFalse_WhenNotAtHome()
    {
        var command = new HomeUpgradeCommand();
        var session = new GameSession();
        session.World.TravelTo(LocationId.Market);
        session.Player.Stats.SetMoney(100);

        var result = command.Execute(session, HomeUpgrade.Curtain);

        result.Should().BeFalse();
        session.HomeUpgrades.PurchasedUpgrades.Should().NotContain(HomeUpgrade.Curtain);
    }

    [Test]
    public void Execute_ReturnsFalse_WhenUnaffordable()
    {
        var command = new HomeUpgradeCommand();
        var session = new GameSession();
        session.World.TravelTo(LocationId.Home);
        session.Player.Stats.SetMoney(5);

        var result = command.Execute(session, HomeUpgrade.Curtain);

        result.Should().BeFalse();
        session.HomeUpgrades.PurchasedUpgrades.Should().NotContain(HomeUpgrade.Curtain);
    }
}
