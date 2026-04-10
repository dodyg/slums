using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace Slums.Game.Screens;

internal sealed class EventLogViewerScreen : ScreenSurface
{
    private readonly GameScreen _parentScreen;
    private readonly IReadOnlyList<string> _entries;
    private int _scrollOffset;

    public EventLogViewerScreen(int width, int height, GameScreen parentScreen, IReadOnlyList<string> entries)
        : base(width, height)
    {
        _parentScreen = parentScreen;
        _entries = entries;
        _scrollOffset = Math.Max(0, entries.Count - GetVisibleLineCount());
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    private int GetVisibleLineCount()
    {
        return Surface.Height - 4;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        var centerX = Surface.Width / 2;
        Surface.Print(centerX - 6, 1, "=== Event Log ===", Color.Cyan);

        var visible = GetVisibleLineCount();
        var maxOffset = Math.Max(0, _entries.Count - visible);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, maxOffset);

        var y = 3;
        for (var i = 0; i < visible && _scrollOffset + i < _entries.Count; i++)
        {
            var entryIndex = _scrollOffset + i;
            var text = TrimToWidth(_entries[entryIndex], Surface.Width - 4);
            var color = entryIndex >= _entries.Count - GameScreenLayout.MaxEventLogEntries
                ? Color.Gray
                : Color.DarkGray;
            Surface.Print(2, y + i, text, color);
        }

        if (_entries.Count > 0)
        {
            var page = _entries.Count > 0 ? $"{_scrollOffset + 1}-{Math.Min(_scrollOffset + visible, _entries.Count)}/{_entries.Count}" : "0";
            Surface.Print(2, Surface.Height - 2, $"{page} | Up/Down=scroll PgUp/PgDn=page Home/End=jump Esc=close", Color.DarkGray);
        }
        else
        {
            Surface.Print(2, Surface.Height - 2, "No events recorded. Esc=close", Color.DarkGray);
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        var visible = GetVisibleLineCount();
        var maxOffset = Math.Max(0, _entries.Count - visible);

        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _scrollOffset = Math.Max(0, _scrollOffset - 1);
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _scrollOffset = Math.Min(maxOffset, _scrollOffset + 1);
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.PageUp))
        {
            _scrollOffset = Math.Max(0, _scrollOffset - visible);
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.PageDown))
        {
            _scrollOffset = Math.Min(maxOffset, _scrollOffset + visible);
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Home))
        {
            _scrollOffset = 0;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.End))
        {
            _scrollOffset = maxOffset;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            ReturnToParentScreen();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }

    private static string TrimToWidth(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }
}
