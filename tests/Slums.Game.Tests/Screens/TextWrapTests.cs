using FluentAssertions;
using Slums.Game.Rendering;
using TUnit;

namespace Slums.Game.Tests.Screens;

internal sealed class TextWrapTests
{
    [Test]
    public void WrapText_ExactWidthWord_RemainsOnOneLine()
    {
        TextWrap.WrapText("hello", 5).Should().Equal("hello");
    }

    [Test]
    public void WrapText_WordLongerThanWidth_RemainsIntact()
    {
        TextWrap.WrapText("hello", 3).Should().Equal("hello");
    }

    [Test]
    public void WrapText_MultipleSpaces_CollapsesSeparators()
    {
        TextWrap.WrapText("one   two    three", 50).Should().Equal("one two three");
    }
}
