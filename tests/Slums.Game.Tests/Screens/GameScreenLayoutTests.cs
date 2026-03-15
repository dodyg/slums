using FluentAssertions;
using Slums.Game.Screens;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class GameScreenLayoutTests
{
    [Test]
    public void Layout_ShouldReserveRoomForMotherHealth_WithoutPushingActionsOffScreen()
    {
        var stressRowY = GameScreenLayout.GetStressStatRowY(GameRuntime.ScreenHeight);
        var actionHeaderY = GameScreenLayout.GetActionHeaderY(GameRuntime.ScreenHeight);
        var actionListStartY = GameScreenLayout.GetActionListStartY(GameRuntime.ScreenHeight);

        stressRowY.Should().Be(11);
        actionHeaderY.Should().Be(12);
        actionListStartY.Should().Be(13);
    }
}
