using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.Community;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class CommunityEventScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 5;
    private const int ListRowHeight = 2;
    private const int DetailX = 42;
    private readonly CommunityEventMenuContext _context;
    private readonly GameSession _gameState;
    private readonly List<CommunityEventMenuStatus> _events;
    private readonly GameScreen _parentScreen;
    private readonly AttendCommunityEventCommand _command = new();
    private int _selectedIndex;

    public CommunityEventScreen(int width, int height, GameSession gameState, CommunityEventMenuContext context, List<CommunityEventMenuStatus> events, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _events = events;
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

        Surface.Print(ListX, 2, "=== Community Events ===", Color.Cyan);
        Surface.Print(ListX, 3, $"Attended: {_context.Attendance.TotalAttended} | Skips: {_context.Attendance.ConsecutiveSkips}", Color.Gray);
        Surface.Print(DetailX, 2, "=== Event Details ===", Color.Cyan);

        for (var i = 0; i < _events.Count; i++)
        {
            var status = _events[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = ListY + (i * ListRowHeight);
            var color = status.CanAttend
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;

            Surface.Print(ListX, rowY, TrimToFit($"{prefix}{status.Event.Name}", DetailX - ListX - 2), color);
            Surface.Print(ListX + 2, rowY + 1, TrimToFit(GetStatusLine(status), DetailX - ListX - 4), status.CanAttend ? Color.Green : Color.Orange);
        }

        RenderSelectedEventDetails();

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to attend, Escape to cancel", Color.DarkGray);
        Surface.Print(2, Surface.Height - 2, $"Money: {_context.PlayerMoney} LE | Stress: {_gameState.Player.Stats.Stress}%",
            _gameState.Player.Stats.Stress > 70 ? Color.Red : Color.Green);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _events.Count) % _events.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _events.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            AttendSelectedEvent();
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
        for (var i = 0; i < _events.Count; i++)
        {
            var blockStartY = ListY + i * ListRowHeight;
            if (cellPosition.Y >= blockStartY &&
                cellPosition.Y < blockStartY + ListRowHeight &&
                cellPosition.X >= ListX &&
                cellPosition.X < DetailX - 1)
            {
                _selectedIndex = i;
                AttendSelectedEvent();
                return true;
            }
        }

        return handled;
    }

    private void AttendSelectedEvent()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _events.Count)
        {
            return;
        }

        var status = _events[_selectedIndex];
        if (!status.CanAttend)
        {
            return;
        }

        _command.Execute(_gameState, status.Event.Id);
        ReturnToParentScreen();
    }

    private static string GetStatusLine(CommunityEventMenuStatus status)
    {
        return status.CanAttend
            ? $"Cost: {status.Event.MoneyCost} LE | Time: {FormatDuration(status.Event.TimeCostMinutes)} | Stress {status.Event.StressChange}"
            : status.UnavailabilityReason ?? "Not available";
    }

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }

    private void RenderSelectedEventDetails()
    {
        if (_events.Count == 0)
        {
            return;
        }

        var selected = _events[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Event.Name, Color.White);
        foreach (var line in WrapText(selected.Event.Description, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        y++;
        Surface.Print(DetailX, y++, $"Cost: {selected.Event.MoneyCost} LE", Color.Yellow);
        Surface.Print(DetailX, y++, $"Duration: {FormatDuration(selected.Event.TimeCostMinutes)}", Color.Gray);
        Surface.Print(DetailX, y++, $"Stress: {selected.Event.StressChange}", selected.Event.StressChange < 0 ? Color.Green : Color.Orange);
        Surface.Print(DetailX, y++, $"Trust: +{selected.Event.TrustGainAmount} with {selected.Event.TrustGainCount} NPCs", Color.Cyan);

        if (selected.Event.ProvidesFoodAccess)
        {
            Surface.Print(DetailX, y++, "Provides food for the day", Color.Green);
        }

        if (selected.Event.ProvidesInformationTips)
        {
            Surface.Print(DetailX, y++, "Information network tips", Color.Cyan);
        }

        if (selected.Event.HasPickpocketRisk)
        {
            Surface.Print(DetailX, y++, "Risk of pickpockets!", Color.Red);
        }

        if (!selected.CanAttend)
        {
            y++;
            foreach (var line in WrapText(selected.UnavailabilityReason ?? "Cannot attend this event.", detailWidth))
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
