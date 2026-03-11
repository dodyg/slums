using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal sealed class TravelScreen : ScreenSurface
{
    private const int DestinationStartX = 4;
    private const int DestinationStartY = 5;
    private const int DestinationRowHeight = 2;
    private readonly GameState _gameState;
    private readonly IReadOnlyList<Location> _locations;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public TravelScreen(int width, int height, GameState gameState, IReadOnlyList<Location> locations, GameScreen parentScreen) 
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
        Surface.Print(2, 4, "Select destination (T to quick travel, Esc to cancel):", Color.Gray);

        for (var i = 0; i < _locations.Count; i++)
        {
            var loc = _locations[i];
            var isCurrentLocation = loc.Id == _gameState.World.CurrentLocationId;
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : isCurrentLocation ? Color.DarkGray : Color.White;

            var currentLocationSuffix = isCurrentLocation ? " [Current]" : string.Empty;
            var displayName = $"{loc.Name} ({DistrictInfo.GetName(loc.District)}){currentLocationSuffix}";
            var travelInfo = $"[{loc.TravelTimeMinutes} min]";
            
            var rowY = DestinationStartY + i * DestinationRowHeight;
            Surface.Print(DestinationStartX, rowY, $"{prefix}{displayName}", color);
            Surface.Print(Surface.Width - travelInfo.Length - 2, rowY, travelInfo, Color.Yellow);
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
            var rowY = DestinationStartY + i * DestinationRowHeight;
            if (cellPosition.Y >= rowY &&
                cellPosition.Y < rowY + DestinationRowHeight &&
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
        var success = _gameState.TryTravelTo(location.Id);
        
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
