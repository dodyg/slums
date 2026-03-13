using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class EntertainmentScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 5;
    private const int ListRowHeight = 2;
    private const int DetailX = 40;
    private readonly EntertainmentMenuContext _context;
    private readonly GameSession _gameState;
    private readonly List<EntertainmentMenuStatus> _activities;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public EntertainmentScreen(int width, int height, GameSession gameState, EntertainmentMenuContext context, List<EntertainmentMenuStatus> activities, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _activities = activities;
        _parentScreen = parentScreen;
        _selectedIndex = 0;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Entertainment ===", Color.Cyan);
        Surface.Print(ListX, 3, $"Location: {_context.LocationName}", Color.Gray);
        Surface.Print(DetailX, 2, "=== Activity Details ===", Color.Cyan);

        for (var i = 0; i < _activities.Count; i++)
        {
            var status = _activities[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = ListY + (i * ListRowHeight);
            var color = status.CanPerform
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;

            Surface.Print(ListX, rowY, TrimToFit($"{prefix}{status.Activity.Name}", DetailX - ListX - 2), color);
            Surface.Print(ListX + 2, rowY + 1, TrimToFit(GetStatusLine(status), DetailX - ListX - 4), status.CanPerform ? Color.Green : Color.Orange);
        }

        RenderSelectedActivityDetails();

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to perform, Escape to cancel", Color.DarkGray);
        Surface.Print(2, Surface.Height - 2, $"Money: {_context.Player.Stats.Money} LE | Stress: {_context.Player.Stats.Stress}%",
            _context.Player.Stats.Stress > 70 ? Color.Red : Color.Green);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _activities.Count) % _activities.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _activities.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            PerformSelectedActivity();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            ReturnToParentScreen();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        var handled = base.ProcessMouse(state);
        if (!state.IsOnScreenObject || !state.Mouse.LeftClicked)
        {
            return handled;
        }

        var cellPosition = state.SurfaceCellPosition;
        for (var i = 0; i < _activities.Count; i++)
        {
            var blockStartY = ListY + i * ListRowHeight;
            if (cellPosition.Y >= blockStartY &&
                cellPosition.Y < blockStartY + ListRowHeight &&
                cellPosition.X >= ListX &&
                cellPosition.X < DetailX - 1)
            {
                _selectedIndex = i;
                PerformSelectedActivity();
                return true;
            }
        }

        return handled;
    }

    private void PerformSelectedActivity()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _activities.Count)
        {
            return;
        }

        var status = _activities[_selectedIndex];
        if (!status.CanPerform)
        {
            return;
        }

        _gameState.TryPerformEntertainment(status.Activity);
        ReturnToParentScreen();
    }

    private static string GetStatusLine(EntertainmentMenuStatus status)
    {
        return status.CanPerform
            ? $"Cost: {status.Activity.BaseCost} LE | Stress -{status.Activity.StressReduction}"
            : status.UnavailabilityReason ?? "Not available";
    }

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }

    private void RenderSelectedActivityDetails()
    {
        if (_activities.Count == 0)
        {
            return;
        }

        var selected = _activities[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Activity.Name, Color.White);
        foreach (var line in WrapText(selected.Activity.Description, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        y++;
        Surface.Print(DetailX, y++, $"Cost: {selected.Activity.BaseCost} LE", Color.Yellow);
        Surface.Print(DetailX, y++, $"Duration: {FormatDuration(selected.Activity.DurationMinutes)}", Color.Gray);
        Surface.Print(DetailX, y++, $"Stress Reduction: -{selected.Activity.StressReduction}", Color.Green);
        if (selected.Activity.EnergyCost > 0)
        {
            Surface.Print(DetailX, y++, $"Energy Cost: -{selected.Activity.EnergyCost}", Color.Orange);
        }

        if (!selected.CanPerform)
        {
            y++;
            foreach (var line in WrapText(selected.UnavailabilityReason ?? "Cannot perform this activity.", detailWidth))
            {
                Surface.Print(DetailX, y++, line, Color.Red);
            }
        }
    }

    private static string FormatDuration(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes}m";
        }
        var hours = minutes / 60;
        var mins = minutes % 60;
        return mins > 0 ? $"{hours}h {mins}m" : $"{hours}h";
    }

    private static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(current) ? word : $"{current} {word}";
            if (candidate.Length > maxWidth && current.Length > 0)
            {
                yield return current;
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
        {
            yield return current;
        }
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
