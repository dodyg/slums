using SadConsole;
using SadConsole.Transitions;

namespace Slums.Game.Screens;

internal static class ScreenTransition
{
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(250);

    internal static void FadeTo(ScreenSurface newScreen)
    {
        if (GameHost.Instance.Screen is ScreenSurface current)
        {
            current.IsFocused = false;
        }

        newScreen.IsFocused = true;
        GameHost.Instance.Screen = newScreen;
        newScreen.SadComponents.Add(new FadeIn(newScreen, FadeDuration));
    }

    internal static void SwitchTo(ScreenSurface newScreen)
    {
        if (GameHost.Instance.Screen is ScreenSurface current)
        {
            current.IsFocused = false;
        }

        newScreen.IsFocused = true;
        GameHost.Instance.Screen = newScreen;
    }
}
