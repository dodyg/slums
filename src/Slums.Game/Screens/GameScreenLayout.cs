namespace Slums.Game.Screens;

internal static class GameScreenLayout
{
    private const int StatTopY = 2;
    private const int StressStatOffset = 4;
    private const int ActionHeaderY = 8;

    internal const int ActionListX = 2;
    internal const int OverviewX = 45;
    internal const int OverviewY = 0;
    internal const int RightPanelWidth = 53;
    internal const int EventLogX = 45;
    internal const int EventLogY = 19;
    internal const int StatusPageX = 45;
    internal const int StatusPageY = 11;
    internal const int MaxEventLogEntries = 200;

    internal static int GetStatRowY(int screenHeight, int statOffset)
    {
        _ = screenHeight;
        return StatTopY + statOffset;
    }

    internal static int GetActionHeaderY(int screenHeight)
    {
        _ = screenHeight;
        return ActionHeaderY;
    }

    internal static int GetActionListStartY(int screenHeight)
    {
        _ = screenHeight;
        return ActionHeaderY + 1;
    }

    internal static int GetStressStatRowY(int screenHeight)
    {
        _ = screenHeight;
        return GetStatRowY(screenHeight, StressStatOffset);
    }
}
