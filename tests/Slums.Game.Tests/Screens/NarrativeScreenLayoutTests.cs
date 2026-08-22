using FluentAssertions;
using Slums.Game.Screens;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class NarrativeScreenLayoutTests
{
    [Test]
    public void GetTextPanelHeight_ShouldReserveSpaceForChoicesAndHints()
    {
        var height = NarrativeScreenLayout.GetTextPanelHeight(GameRuntime.ScreenHeight);

        height.Should().Be(GameRuntime.ScreenHeight - NarrativeScreenLayout.ReservedBottomRows);
    }

    [Test]
    public void ClampScrollOffset_ShouldKeepOffsetWithinWrappedTextWindow()
    {
        var offset = NarrativeScreenLayout.ClampScrollOffset(
            scrollOffset: 99,
            wrappedLineCount: 32,
            visibleLineCount: 10);

        offset.Should().Be(22);
    }

    [Test]
    public void GetScrollPositionCount_ShouldMatchOneBasedScrollIndicator()
    {
        var positions = NarrativeScreenLayout.GetScrollPositionCount(
            wrappedLineCount: 32,
            visibleLineCount: 10);

        positions.Should().Be(23);
    }
}
