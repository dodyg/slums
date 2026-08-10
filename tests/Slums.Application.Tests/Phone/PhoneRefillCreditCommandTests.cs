using FluentAssertions;
using Slums.Application.Phone;
using Slums.Core.State;
using TUnit;

namespace Slums.Application.Tests.Phone;

internal sealed class PhoneRefillCreditCommandTests
{
    [Test]
    public void Execute_RefillsCredit_WhenOutOfCredit()
    {
        var command = new PhoneRefillCreditCommand();
        var session = new GameSession();
        session.RestorePhoneState(hasPhone: true, creditRemaining: 0, daysSinceCreditRefill: 7, phoneLost: false, phoneLostDay: null, phoneRecovered: false);
        session.Player.Stats.SetMoney(10);

        var (success, _) = command.Execute(session);

        success.Should().BeTrue();
        session.Phone.IsOperational().Should().BeTrue();
    }

    [Test]
    public void Execute_FailsWhenPhoneLost()
    {
        var command = new PhoneRefillCreditCommand();
        var session = new GameSession();
        session.Phone.LosePhone(1);
        session.Player.Stats.SetMoney(100);

        var (success, _) = command.Execute(session);

        success.Should().BeFalse();
    }

    [Test]
    public void Execute_FailsWhenNotEnoughMoney()
    {
        var command = new PhoneRefillCreditCommand();
        var session = new GameSession();
        session.RestorePhoneState(hasPhone: true, creditRemaining: 0, daysSinceCreditRefill: 7, phoneLost: false, phoneLostDay: null, phoneRecovered: false);
        session.Player.Stats.SetMoney(1);

        var (success, _) = command.Execute(session);

        success.Should().BeFalse();
    }

    [Test]
    public void Execute_Throws_WhenSessionIsNull()
    {
        var command = new PhoneRefillCreditCommand();

        var act = () => command.Execute(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
