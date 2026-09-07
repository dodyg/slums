using FluentAssertions;
using Slums.Game.Rendering;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class UiTextTests
{
    [Test]
    public void TrimToFit_ShouldUseAnEllipsisWhenTextExceedsTheLimit()
    {
        UiText.TrimToFit("abcdefgh", 6).Should().Be("abc...");
    }

    [Test]
    public void FormatDuration_ShouldUseHoursAndMinutesWhenNeeded()
    {
        UiText.FormatDuration(45).Should().Be("45m");
        UiText.FormatDuration(120).Should().Be("2h");
        UiText.FormatDuration(95).Should().Be("1h 35m");
    }
}
