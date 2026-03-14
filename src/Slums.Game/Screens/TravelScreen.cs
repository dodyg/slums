using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal sealed class TravelScreen : ScreenSurface
{
    private const int DestinationStartX = 4;
    private const int DestinationStartY = 6;
    private const int ListRowHeight = 2;
    private readonly GameSession _gameState;
    private readonly IReadOnlyList<Location> _locations;
    private readonly GameScreen _parentScreen;
    private readonly TravelCommand _travelCommand = new();
    private int _selectedIndex;

    public TravelScreen(int width, int height, GameSession gameState, IReadOnlyList<Location> locations, GameScreen parentScreen) 
        : base(width, height)
    {
        _gameState = gameState;
        _locations = locations;
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
        Surface.Print(centerX - 5, 2, "=== Travel ===", Color.Cyan);
        Surface.Print(2, 4, "Select destination (Enter=travel, W=walk, Esc=cancel):", Color.Gray);

        for (var i = 0; i < _locations.Count; i++)
        {
            var loc = _locations[i];
            var isCurrentLocation = loc.Id == _gameState.World.CurrentLocationId;
            var canAffordTravel = _gameState.CanAffordTravel(loc.Id);
            var prefix = i == _selectedIndex ? "> " : "  ";
            var nameColor = i == _selectedIndex ? Color.Cyan : isCurrentLocation ? Color.DarkGray : canAffordTravel ? Color.White : Color.Orange;

            var currentLocationSuffix = isCurrentLocation ? " [Current]" : string.Empty;
            var displayName = $"{loc.Name} ({DistrictInfo.GetName(loc.District)}){currentLocationSuffix}";
            var walkTime = loc.TravelTimeMinutes * 3;
            var travelInfo = canAffordTravel 
                ? $"[{loc.TravelTimeMinutes} min]" 
                : $"[Walk: {walkTime} min]";
            
            var rowY = DestinationStartY + i * ListRowHeight;
            Surface.Print(DestinationStartX, rowY, $"{prefix}{displayName}", nameColor);
            Surface.Print(Surface.Width - travelInfo.Length - 2, rowY, travelInfo, canAffordTravel ? Color.Yellow : Color.Orange);
            Surface.Print(6, rowY + 1, $"{loc.Description[..Math.Min(50, loc.Description.Length)]}", Color.DarkGray);
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _locations.Count) % _locations.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _locations.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            TravelToSelected();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.W))
        {
            WalkToSelected();
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
        for (var i = 0; i < _locations.Count; i++)
        {
            var rowY = DestinationStartY + i * ListRowHeight;
            if (cellPosition.Y >= rowY &&
                cellPosition.Y < rowY + ListRowHeight &&
                cellPosition.X >= DestinationStartX &&
                cellPosition.X < Surface.Width - 2)
            {
                _selectedIndex = i;
                TravelToSelected();
                return true;
            }
        }

        return handled;
    }

    private void TravelToSelected()
    {
        var location = _locations[_selectedIndex];
        var success = _travelCommand.Execute(_gameState, location.Id, TravelMode.Transport);
        
        if (!success)
        {
            _parentScreen.IsFocused = true;
        }

        ReturnToParentScreen();
    }

    private void WalkToSelected()
    {
        var location = _locations[_selectedIndex];
        var success = _travelCommand.Execute(_gameState, location.Id, TravelMode.Walk);
        
        if (!success)
        {
            _parentScreen.IsFocused = true;
        }

        ReturnToParentScreen();
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
