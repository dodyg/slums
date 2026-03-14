using FluentAssertions;
using Slums.Game.Screens;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class GameScreenLayoutTests
{
    [Test]
    public void ActionsHeader_ShouldRenderBelowStressStatRow()
    {
        var stressRowY = GameScreenLayout.GetStressStatRowY(GameRuntime.ScreenHeight);
        var actionHeaderY = GameScreenLayout.GetActionHeaderY(GameRuntime.ScreenHeight);
        var actionListStartY = GameScreenLayout.GetActionListStartY(GameRuntime.ScreenHeight);

        actionHeaderY.Should().Be(stressRowY + 1);
        actionListStartY.Should().Be(actionHeaderY + 2);
    }
}
