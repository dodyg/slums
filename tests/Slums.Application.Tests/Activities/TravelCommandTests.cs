using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class TravelCommandTests
{
    [Test]
    public void Execute_ShouldThrow_WhenGameSessionIsNull()
    {
        var command = new TravelCommand();
        var act = () => command.Execute(null!, LocationId.CallCenter, TravelMode.Transport);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Execute_ShouldThrow_WhenInvalidTravelMode()
    {
        var command = new TravelCommand();
        using var session = new GameSession();

        var act = () => command.Execute(session, LocationId.CallCenter, (TravelMode)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Execute_Transport_CallsTryTravelTo()
    {
        var command = new TravelCommand();
        using var session = new GameSession();
        session.Player.Stats.SetMoney(10);
        session.Player.Stats.SetEnergy(50);

        var result = command.Execute(session, LocationId.CallCenter, TravelMode.Transport);

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_Walk_CallsTryWalkTo()
    {
        var command = new TravelCommand();
        using var session = new GameSession();
        session.Player.Stats.SetEnergy(50);

        var result = command.Execute(session, LocationId.CallCenter, TravelMode.Walk);

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_WalkFails_WhenNotEnoughEnergy()
    {
        var command = new TravelCommand();
        using var session = new GameSession();
        session.Player.Stats.SetEnergy(1);

        var result = command.Execute(session, LocationId.CallCenter, TravelMode.Walk);

        result.Should().BeFalse();
    }
}
