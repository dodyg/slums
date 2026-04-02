using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class ShopCommandTests
{
    [Test]
    public void Execute_ShouldThrow_WhenGameSessionIsNull()
    {
        var command = new ShopCommand();
        var act = () => command.Execute(null!, ShopOptionId.BuyFood);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Execute_ShouldThrow_WhenInvalidOptionId()
    {
        var command = new ShopCommand();
        using var session = new GameSession();

        var act = () => command.Execute(session, (ShopOptionId)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Execute_OpenHouseholdAssets_ReturnsTrueWithoutMutation()
    {
        var command = new ShopCommand();
        using var session = new GameSession();
        var moneyBefore = session.Player.Stats.Money;

        var result = command.Execute(session, ShopOptionId.OpenHouseholdAssets);

        result.Should().BeTrue();
        session.Player.Stats.Money.Should().Be(moneyBefore);
    }

    [Test]
    public void Execute_BuyFood_CallsBuyFood()
    {
        var command = new ShopCommand();
        using var session = new GameSession();
        session.Player.Stats.SetMoney(50);

        var result = command.Execute(session, ShopOptionId.BuyFood);

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_BuyMedicine_CallsBuyMedicine()
    {
        var command = new ShopCommand();
        using var session = new GameSession();
        session.Player.Stats.SetMoney(100);

        var result = command.Execute(session, ShopOptionId.BuyMedicine);

        result.Should().BeTrue();
    }
}
