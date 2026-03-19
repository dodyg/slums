namespace Slums.Game.Screens;

internal static class TravelScreenLayout
{
    internal const int DestinationStartX = 4;
    internal const int DestinationStartY = 6;
    internal const int ScrollBarXOffset = 1;

    internal static int GetDetailStartY(int screenHeight)
    {
        return screenHeight - 4;
    }

    internal static int GetMaxVisibleDestinations(int screenHeight)
    {
        return Math.Max(1, GetDetailStartY(screenHeight) - DestinationStartY - 1);
    }

    internal static int GetFirstVisibleIndex(int selectedIndex, int visibleCount, int totalCount)
    {
        if (totalCount <= 0)
        {
            return 0;
        }

        var safeSelectedIndex = Math.Clamp(selectedIndex, 0, totalCount - 1);
        var safeVisibleCount = Math.Max(1, visibleCount);
        return Math.Clamp(safeSelectedIndex - safeVisibleCount + 1, 0, Math.Max(0, totalCount - safeVisibleCount));
    }

    internal static int GetScrollThumbSize(int visibleCount, int totalCount)
    {
        if (totalCount <= visibleCount)
        {
            return 0;
        }

        var safeVisibleCount = Math.Max(1, visibleCount);
        return Math.Max(1, (int)Math.Round((double)(safeVisibleCount * safeVisibleCount) / totalCount, MidpointRounding.AwayFromZero));
    }

    internal static int GetScrollThumbOffset(int firstVisibleIndex, int visibleCount, int totalCount, int thumbSize)
    {
        if (totalCount <= visibleCount || thumbSize <= 0)
        {
            return 0;
        }

        var maxFirstVisibleIndex = totalCount - visibleCount;
        if (maxFirstVisibleIndex <= 0)
        {
            return 0;
        }

        var maxThumbOffset = visibleCount - thumbSize;
        return (int)Math.Round((double)firstVisibleIndex / maxFirstVisibleIndex * maxThumbOffset, MidpointRounding.AwayFromZero);
    }
}
