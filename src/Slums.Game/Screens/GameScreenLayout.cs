namespace Slums.Game.Screens;

internal static class GameScreenLayout
{
    private const int HudTopOffsetFromBottom = 20;
    private const int StressStatOffset = 5;

    internal const int ActionListX = 2;
    internal const int EventLogX = 45;
    internal const int EventLogY = 18;
    internal const int StatusPageX = 45;
    internal const int StatusPageY = 0;
    internal const int MaxEventLogEntries = 8;

    internal static int GetStatRowY(int screenHeight, int statOffset)
    {
        return screenHeight - HudTopOffsetFromBottom + statOffset;
    }

    internal static int GetActionHeaderY(int screenHeight)
    {
        return GetStressStatRowY(screenHeight) + 1;
    }

    internal static int GetActionListStartY(int screenHeight)
    {
        return GetActionHeaderY(screenHeight) + 2;
    }

    internal static int GetStressStatRowY(int screenHeight)
    {
        return GetStatRowY(screenHeight, StressStatOffset);
    }
}
