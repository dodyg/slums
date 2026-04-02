using FluentAssertions;
using Slums.Application.Activities;
using Slums.Core.State;
using Slums.Core.World;
using TUnit.Core;

namespace Slums.Application.Tests.Activities;

internal sealed class GameActionCommandTests
{
    [Test]
    public void Execute_ShouldThrow_WhenGameSessionIsNull()
    {
        var command = new GameActionCommand();
        var act = () => command.Execute(null!, GameActionId.Rest);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Execute_ShouldThrow_WhenActionIdRequiresDedicatedUI()
    {
        var command = new GameActionCommand();
        using var session = new GameSession();

        var act = () => command.Execute(session, GameActionId.Work);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Execute_Rest_CallsRestAtHome()
    {
        var command = new GameActionCommand();
        using var session = new GameSession();
        session.World.TravelTo(LocationId.Home);
        session.Player.Stats.SetEnergy(50);

        var result = command.Execute(session, GameActionId.Rest);

        result.Should().BeTrue();
        session.Player.Stats.Energy.Should().BeGreaterThan(50);
    }

    [Test]
    public void Execute_CheckOnMother_ReturnsTrue()
    {
        var command = new GameActionCommand();
        using var session = new GameSession();

        var result = command.Execute(session, GameActionId.CheckOnMother);

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_EndDay_ReturnsTrue()
    {
        var command = new GameActionCommand();
        using var session = new GameSession();

        var result = command.Execute(session, GameActionId.EndDay, new Random(42));

        result.Should().BeTrue();
    }
}
