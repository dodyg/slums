using FluentAssertions;
using Slums.Application.Phone;
using Slums.Core.Phone;
using Slums.Core.State;
using TUnit;

namespace Slums.Application.Tests.Phone;

internal sealed class PhoneReplaceCommandTests
{
    [Test]
    public void Execute_ReplacesLostPhone()
    {
        var command = new PhoneReplaceCommand();
        var session = new GameSession();
        session.Phone.LosePhone(3);
        session.Player.Stats.SetMoney(100);

        var (success, _) = command.Execute(session);

        success.Should().BeTrue();
        session.Phone.PhoneLost.Should().BeFalse();
        session.Phone.IsOperational().Should().BeTrue();
        session.Player.Stats.Money.Should().Be(100 - PhoneState.ReplacementCost);
    }

    [Test]
    public void Execute_FailsWhenPhoneNotLost()
    {
        var command = new PhoneReplaceCommand();
        var session = new GameSession();
        session.Player.Stats.SetMoney(100);

        var (success, _) = command.Execute(session);

        success.Should().BeFalse();
    }

    [Test]
    public void Execute_FailsWhenNotEnoughMoney()
    {
        var command = new PhoneReplaceCommand();
        var session = new GameSession();
        session.Phone.LosePhone(3);
        session.Player.Stats.SetMoney(20);

        var (success, _) = command.Execute(session);

        success.Should().BeFalse();
    }

    [Test]
    public void Execute_Throws_WhenSessionIsNull()
    {
        var command = new PhoneReplaceCommand();

        var act = () => command.Execute(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
