using FluentAssertions;
using Slums.Core.World;
using Slums.Game.Screens;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class TravelScreenLayoutTests
{
    [Test]
    public void TravelMenuLayout_ShouldFitAllTravelableLocations_OnDefaultScreen()
    {
        var maxVisible = TravelScreenLayout.GetMaxVisibleDestinations(GameRuntime.ScreenHeight);

        maxVisible.Should().BeGreaterOrEqualTo(WorldState.AllLocations.Count - 1);
    }

    [Test]
    public void GetFirstVisibleIndex_ShouldKeepSelectedDestinationInView_WhenScrollingIsNeeded()
    {
        var firstVisibleIndex = TravelScreenLayout.GetFirstVisibleIndex(selectedIndex: 11, visibleCount: 5, totalCount: 12);

        firstVisibleIndex.Should().Be(7);
    }

    [Test]
    public void ScrollThumbMetrics_ShouldMatchVisibleWindow()
    {
        var thumbSize = TravelScreenLayout.GetScrollThumbSize(visibleCount: 5, totalCount: 12);
        var thumbOffset = TravelScreenLayout.GetScrollThumbOffset(firstVisibleIndex: 7, visibleCount: 5, totalCount: 12, thumbSize);

        thumbSize.Should().Be(2);
        thumbOffset.Should().Be(3);
    }
}
