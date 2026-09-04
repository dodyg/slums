using FluentAssertions;
using Slums.Core.Phone;
using Slums.Core.State;
using TUnit.Core;

namespace Slums.Core.Tests.Phone;

internal sealed class PhoneServiceTests
{
    [Test]
    public void RefillCredit_ShouldRecordPhoneMutationAndRestoreCredit()
    {
        var session = new GameSession();
        session.Player.Stats.SetMoney(20);
        for (var i = 0; i < 7; i++)
        {
            session.Phone.DailyCreditDrain();
        }

        var result = PhoneService.RefillCredit(session);

        result.Success.Should().BeTrue();
        session.Phone.IsOperational().Should().BeTrue();
        session.Mutations[^1].Action.Should().Be("RefillPhoneCredit");
    }

    [Test]
    public void RestoreState_ShouldHydrateTheExistingPhoneState()
    {
        var session = new GameSession();
        var phone = session.Phone;

        PhoneService.RestoreState(session, true, 3, 4, false, null, false);

        session.Phone.Should().BeSameAs(phone);
        session.Phone.CreditRemaining.Should().Be(3);
    }
}
