using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal sealed class TravelScreen : ScreenSurface
{
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
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        var centerX = Surface.Width / 2;
        var startY = 5;

        Surface.Print(centerX - 5, 2, "=== Travel ===", Color.Cyan);
        Surface.Print(2, 4, "Select destination (T to quick travel, Esc to cancel):", Color.Gray);

        for (var i = 0; i < _locations.Count; i++)
        {
            var loc = _locations[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;

            var displayName = $"{loc.Name} ({DistrictInfo.GetName(loc.District)})";
            var travelInfo = $"[{loc.TravelTimeMinutes} min]";
            
            Surface.Print(4, startY + i * 2, $"{prefix}{displayName}", color);
            Surface.Print(6, startY + i * 2 + 1, $"{loc.Description[..Math.Min(50, loc.Description.Length)]}", Color.DarkGray);
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
            GameHost.Instance.Screen = _parentScreen;
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void TravelToSelected()
    {
        var location = _locations[_selectedIndex];
        var success = _gameState.TryTravelTo(location.Id);
        
        if (!success)
        {
            _parentScreen.IsFocused = true;
        }
        
        GameHost.Instance.Screen = _parentScreen;
    }
}
