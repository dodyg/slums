namespace Slums.Game.Screens;

internal static class NarrativeScreenLayout
{
    internal const int TextPanelTop = 3;
    internal const int ReservedBottomRows = 10;

    internal static int GetTextPanelHeight(int screenHeight)
    {
        return Math.Max(1, screenHeight - ReservedBottomRows);
    }

    internal static int GetMaxScrollOffset(int wrappedLineCount, int visibleLineCount)
    {
        return Math.Max(0, wrappedLineCount - visibleLineCount);
    }

    internal static int ClampScrollOffset(int scrollOffset, int wrappedLineCount, int visibleLineCount)
    {
        return Math.Clamp(scrollOffset, 0, GetMaxScrollOffset(wrappedLineCount, visibleLineCount));
    }

    internal static int GetScrollPositionCount(int wrappedLineCount, int visibleLineCount)
    {
        return Math.Max(1, GetMaxScrollOffset(wrappedLineCount, visibleLineCount) + 1);
    }
}
