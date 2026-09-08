using FluentAssertions;
using Slums.Application.Activities;
using Slums.Game.Rendering;
using Slums.Game.Screens;
using Slums.TestSupport;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class GameScreenHudRendererTests
{
    [Test]
    public void BuildDayOverviewText_WithoutActiveHoliday_OmitsHolidaySegment()
    {
        var statusContext = GameStatusContext.Create(TestSessions.Create());

        var text = GameScreenHudRenderer.BuildDayOverviewText(statusContext);

        statusContext.HolidayName.Should().BeNull();
        text.Should().StartWith($"Day {statusContext.Clock.Day} (");
        text.Should().Contain(statusContext.SeasonName);
        text.Should().EndWith(statusContext.WeatherName);
    }

    [Test]
    public void BuildDayOverviewText_WithActiveHoliday_AppendsHolidayName()
    {
        var session = TestSessions.Create();
        session.Clock.SetTime(day: 151, hour: 10, minute: 0);

        var statusContext = GameStatusContext.Create(session);
        var text = GameScreenHudRenderer.BuildDayOverviewText(statusContext);

        statusContext.HolidayName.Should().Be("Ramadan");
        text.Should().Contain($"| {statusContext.SeasonName} | {statusContext.WeatherName} | Ramadan");
    }

    [Test]
    public void TrimToWidth_ShortString_ReturnsAsIs()
    {
        GameScreenHudRenderer.TrimToWidth("hello", 10).Should().Be("hello");
    }

    [Test]
    public void TrimToWidth_LongString_TruncatesWithEllipsis()
    {
        var result = GameScreenHudRenderer.TrimToWidth("hello world this is long", 10);
        result.Should().Be("hello w...");
        result.Length.Should().Be(10);
    }

    [Test]
    public void TrimToWidth_ExactLength_ReturnsAsIs()
    {
        GameScreenHudRenderer.TrimToWidth("hello", 5).Should().Be("hello");
    }

    [Test]
    public void WrapText_ShortLine_ReturnsSingleLine()
    {
        var result = TextWrap.WrapText("hello world", 50).ToList();
        result.Should().ContainSingle().Which.Should().Be("hello world");
    }

    [Test]
    public void WrapText_LongLine_WrapsToMultipleLines()
    {
        var result = TextWrap.WrapText("one two three four five", 8).ToList();
        result.Should().Equal("one two", "three", "four", "five");
    }

    [Test]
    public void WrapText_EmptyString_ReturnsNothing()
    {
        var result = TextWrap.WrapText("", 50).ToList();
        result.Should().BeEmpty();
    }

    [Test]
    public void WrapText_SingleWord_FitsAnyWidth()
    {
        var result = TextWrap.WrapText("hello", 3).ToList();
        result.Should().ContainSingle().Which.Should().Be("hello");
    }
}
