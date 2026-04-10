using FluentAssertions;
using Slums.Game.Screens;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class GameScreenHudRendererTests
{
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
        var result = GameScreenHudRenderer.WrapText("hello world", 50).ToList();
        result.Should().ContainSingle().Which.Should().Be("hello world");
    }

    [Test]
    public void WrapText_LongLine_WrapsToMultipleLines()
    {
        var result = GameScreenHudRenderer.WrapText("one two three four five", 8).ToList();
        result.Should().Equal("one two", "three", "four", "five");
    }

    [Test]
    public void WrapText_EmptyString_ReturnsNothing()
    {
        var result = GameScreenHudRenderer.WrapText("", 50).ToList();
        result.Should().BeEmpty();
    }

    [Test]
    public void WrapText_SingleWord_FitsAnyWidth()
    {
        var result = GameScreenHudRenderer.WrapText("hello", 3).ToList();
        result.Should().ContainSingle().Which.Should().Be("hello");
    }
}
