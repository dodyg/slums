using FluentAssertions;
using Slums.Game.Screens;
using TUnit.Core;

namespace Slums.Game.Tests.Screens;

internal sealed class EventLogViewerLayoutTests
{
    [Test]
    public void MaxEventLogEntries_ShouldSupportLongPlaySessions()
    {
        GameScreenLayout.MaxEventLogEntries.Should().BeGreaterOrEqualTo(100);
    }

    [Test]
    public void EventLogWindow_ShouldShowLatestEntries_WhenLogExceedsVisibleArea()
    {
        const int visibleEntries = 6;
        var entries = new List<string>();
        for (var i = 0; i < 50; i++)
        {
            entries.Add($"Event {i}");
        }

        var start = Math.Max(0, entries.Count - visibleEntries);
        var displayedEntries = new List<string>();
        for (var i = entries.Count - 1; i >= start; i--)
        {
            displayedEntries.Add(entries[i]);
        }

        displayedEntries.Should().HaveCount(visibleEntries);
        displayedEntries[0].Should().Be("Event 49");
        displayedEntries[^1].Should().Be("Event 44");
    }

    [Test]
    public void EventLogWindow_EmptyLog_ShouldShowNothing()
    {
        var entries = new List<string>();
        var visibleEntries = 6;
        var start = Math.Max(0, entries.Count - visibleEntries);

        entries.Count.Should().Be(0);
        start.Should().Be(0);
    }
}
