using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class ClinicTravelScreen : ScreenSurface
{
    private const int OptionsStartX = 4;
    private const int OptionsStartY = 8;
    private const int ListRowHeight = 3;
    private readonly GameSession _gameState;
    private readonly ClinicTravelMenuContext _context;
    private readonly IReadOnlyList<ClinicTravelMenuStatus> _statuses;
    private readonly GameScreen _parentScreen;
    private readonly ClinicTravelCommand _clinicTravelCommand = new();
    private int _selectedIndex;

    public ClinicTravelScreen(
        int width,
        int height,
        GameSession gameState,
        ClinicTravelMenuContext context,
        IReadOnlyList<ClinicTravelMenuStatus> statuses,
        GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _statuses = statuses;
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

        var centerX = Surface.Width / 2;
        Surface.Print(centerX - 12, 2, "=== Take Mother to Clinic ===", Color.Cyan);
        Surface.Print(2, 4, $"Your Money: {_context.PlayerMoney} LE | Mother's Health: {_context.MotherHealth}%", Color.White);
        Surface.Print(2, 6, "Select a clinic (Enter=go, Esc=cancel):", Color.Gray);

        for (var i = 0; i < _statuses.Count; i++)
        {
            var status = _statuses[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var nameColor = i == _selectedIndex ? Color.Cyan : Color.White;
            var canSelect = status.IsOpenToday && status.CanAfford;

            if (!canSelect)
            {
                nameColor = Color.DarkGray;
            }

            var rowY = OptionsStartY + i * ListRowHeight;
            var displayName = $"{status.LocationName} ({status.DistrictName})";

            Surface.Print(OptionsStartX, rowY, $"{prefix}{displayName}", nameColor);

            var costInfo = status.IsOpenToday
                ? $"{status.TotalCost} LE ({status.TravelCost} travel + {status.ClinicCost} clinic)"
                : "CLOSED";
            var costColor = status.IsOpenToday && status.CanAfford ? Color.Green : Color.Orange;
            Surface.Print(OptionsStartX + 2, rowY + 1, $"{costInfo} | {status.TravelTimeMinutes} min travel", costColor);

            if (status.UnavailableReason is not null)
            {
                Surface.Print(OptionsStartX + 2, rowY + 2, status.UnavailableReason, Color.Orange);
            }
            else if (status.IsOpenToday)
            {
                Surface.Print(OptionsStartX + 2, rowY + 2, "Open today", Color.DarkGray);
            }
            else
            {
                Surface.Print(OptionsStartX + 2, rowY + 2, $"Opens: {status.OpenDaysSummary}", Color.DarkGray);
            }
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _statuses.Count) % _statuses.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _statuses.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            TravelToClinic();
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
        for (var i = 0; i < _statuses.Count; i++)
        {
            var rowY = OptionsStartY + i * ListRowHeight;
            if (cellPosition.Y >= rowY &&
                cellPosition.Y < rowY + ListRowHeight &&
                cellPosition.X >= OptionsStartX &&
                cellPosition.X < Surface.Width - 2)
            {
                _selectedIndex = i;
                TravelToClinic();
                return true;
            }
        }

        return handled;
    }

    private void TravelToClinic()
    {
        var status = _statuses[_selectedIndex];
        if (!status.IsOpenToday || !status.CanAfford)
        {
            return;
        }

        _clinicTravelCommand.Execute(_gameState, status.LocationId);
        ReturnToParentScreen();
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
