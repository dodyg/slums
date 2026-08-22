using SadConsole.Input;

namespace Slums.Game.Input;

internal static class NumberKeyMapper
{
    public static int? GetPressedNumberIndex(Keyboard keyboard, int maxCount)
    {
        ArgumentNullException.ThrowIfNull(keyboard);

        for (var index = 0; index < Math.Min(9, maxCount); index++)
        {
            var topRowKey = Keys.D1 + index;
            if (keyboard.IsKeyPressed(topRowKey))
            {
                return GetNumberIndex(topRowKey, maxCount);
            }

            var numPadKey = Keys.NumPad1 + index;
            if (keyboard.IsKeyPressed(numPadKey))
            {
                return GetNumberIndex(numPadKey, maxCount);
            }
        }

        return null;
    }

    public static int? GetNumberIndex(Keys key, int maxCount)
    {
        for (var index = 0; index < Math.Min(9, maxCount); index++)
        {
            if (key == Keys.D1 + index || key == Keys.NumPad1 + index)
            {
                return index;
            }
        }

        return null;
    }
}
