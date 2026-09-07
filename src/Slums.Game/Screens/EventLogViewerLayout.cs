namespace Slums.Game.Screens;

/// <summary>
/// Pure layout math for the event log viewer: visible window, scroll clamping, page
/// indicator, and single-line trimming. Extracted so the rules can be tested without
/// constructing SadConsole surfaces.
/// </summary>
internal static class EventLogViewerLayout
{
    private const int HeaderAndFooterRows = 4;
    private const int FirstEntryRow = 3;

    public static int GetVisibleLineCount(int surfaceHeight)
    {
        return surfaceHeight - HeaderAndFooterRows;
    }

    public static int GetFirstEntryRow()
    {
        return FirstEntryRow;
    }

    public static int ClampScrollOffset(int scrollOffset, int entryCount, int visibleLineCount)
    {
        var maxOffset = Math.Max(0, entryCount - visibleLineCount);
        return Math.Clamp(scrollOffset, 0, maxOffset);
    }

    public static string GetPageText(int scrollOffset, int entryCount, int visibleLineCount)
    {
        if (entryCount == 0)
        {
            return "0";
        }

        var first = scrollOffset + 1;
        var last = Math.Min(scrollOffset + visibleLineCount, entryCount);
        return $"{first}-{last}/{entryCount}";
    }

    public static string TrimToWidth(string text, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }
}
