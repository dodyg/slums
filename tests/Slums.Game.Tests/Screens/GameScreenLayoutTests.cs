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

        stressRowY.Should().Be(6);
        actionHeaderY.Should().Be(8);
        actionListStartY.Should().Be(9);
    }

    [Test]
    public void Layout_ShouldKeepOverviewStatusPageAndEventLogInSeparateBands()
    {
        GameScreenLayout.OverviewY.Should().Be(0);
        GameScreenLayout.StatusPageY.Should().Be(11);
        GameScreenLayout.EventLogY.Should().Be(19);
        GameScreenLayout.RightPanelWidth.Should().Be(53);
    }
}
