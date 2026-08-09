using FluentAssertions;
using Slums.Application.Phone;
using Slums.Core.Information;
using Slums.Core.Phone;
using Slums.Core.State;
using TUnit;

namespace Slums.Application.Tests.Phone;

internal sealed class PhoneActionCommandTests
{
    [Test]
    public void Execute_AcknowledgesTip()
    {
        var command = new PhoneActionCommand();
        using var session = new GameSession();
        var tip = new Tip
        {
            Type = TipType.CrimeWarning,
            Content = "A tip about a route.",
            DayGenerated = 1,
            ExpiresAfterDay = 5
        };
        session.Tips.AddTip(tip);

        var (success, _) = command.Execute(session, tip.Id, isTip: true);

        success.Should().BeTrue();
        session.Tips.GetTip(tip.Id)!.Acknowledged.Should().BeTrue();
    }

    [Test]
    public void Execute_RespondsToMessage()
    {
        var command = new PhoneActionCommand();
        using var session = new GameSession();
        var message = new PhoneMessage
        {
            Type = PhoneMessageType.Warning,
            Sender = "Neighbor Mona",
            Content = "Can you help?",
            DayReceived = 1,
            RequiresResponse = true
        };
        session.PhoneMessages.AddMessage(message);

        var (success, _) = command.Execute(session, message.Id, isTip: false);

        success.Should().BeTrue();
        session.PhoneMessages.GetMessage(message.Id)!.Responded.Should().BeTrue();
    }

    [Test]
    public void Execute_ReturnsFailure_ForUnknownEntry()
    {
        var command = new PhoneActionCommand();
        using var session = new GameSession();

        var (success, message) = command.Execute(session, "missing-id", isTip: false);

        success.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_Throws_WhenSessionIsNull()
    {
        var command = new PhoneActionCommand();

        var act = () => command.Execute(null!, "some-id", isTip: true);

        act.Should().Throw<ArgumentNullException>();
    }
}
