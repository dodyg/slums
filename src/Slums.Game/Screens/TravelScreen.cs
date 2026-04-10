using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.Clock;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal sealed class TravelScreen : ScreenSurface
{
    private readonly GameSession _gameState;
    private readonly IReadOnlyList<Location> _locations;
    private readonly GameScreen _parentScreen;
    private readonly TravelCommand _travelCommand = new();
    private readonly TipContextQuery _tipContextQuery = new();
    private int _selectedIndex;
    private int _mouseSelectedIndex = -1;

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
        Surface.Print(2, 4, "Select destination (Enter=transport, W=walk, Esc=cancel):", Color.Gray);
        var visibleCount = TravelScreenLayout.GetMaxVisibleDestinations(Surface.Height);
        var firstVisibleIndex = TravelScreenLayout.GetFirstVisibleIndex(_selectedIndex, visibleCount, _locations.Count);
        var detailStartY = TravelScreenLayout.GetDetailStartY(Surface.Height);

        for (var rowIndex = 0; rowIndex < visibleCount && firstVisibleIndex + rowIndex < _locations.Count; rowIndex++)
        {
            var i = firstVisibleIndex + rowIndex;
            var loc = _locations[i];
            var travelCost = _gameState.GetTravelCost(loc.Id);
            var travelMinutes = _gameState.GetTravelTimeMinutes(loc.Id);
            var walkMinutes = _gameState.GetWalkTimeMinutes(loc.Id);
            var canAffordTravel = _gameState.Player.Stats.Money >= travelCost;
            var prefix = i == _selectedIndex ? "> " : "  ";
            var nameColor = i == _selectedIndex ? Color.Cyan : canAffordTravel ? Color.White : Color.Orange;

            var displayName = $"{loc.Name} ({DistrictInfo.GetName(loc.District)})";
            var travelInfo = canAffordTravel 
                ? $"[{travelCost} LE | {travelMinutes} min]" 
                : $"[Walk: {walkMinutes} min]";

            var rowY = TravelScreenLayout.DestinationStartY + rowIndex;
            Surface.Print(TravelScreenLayout.DestinationStartX, rowY, TrimToFit($"{prefix}{displayName}", Surface.Width - travelInfo.Length - 8), nameColor);
            Surface.Print(Surface.Width - travelInfo.Length - 2, rowY, travelInfo, canAffordTravel ? Color.Yellow : Color.Orange);
        }

        if (_locations.Count > visibleCount)
        {
            var pagingHint = firstVisibleIndex + visibleCount < _locations.Count
                ? "More destinations below..."
                : "More destinations above...";
            Surface.Print(2, detailStartY - 1, pagingHint, Color.DarkGray);
            RenderScrollBar(firstVisibleIndex, visibleCount);
        }

        RenderSelectedLocationDetails(detailStartY);
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
        var visibleCount = TravelScreenLayout.GetMaxVisibleDestinations(Surface.Height);
        var firstVisibleIndex = TravelScreenLayout.GetFirstVisibleIndex(_selectedIndex, visibleCount, _locations.Count);
        for (var rowIndex = 0; rowIndex < visibleCount && firstVisibleIndex + rowIndex < _locations.Count; rowIndex++)
        {
            var rowY = TravelScreenLayout.DestinationStartY + rowIndex;
            if (cellPosition.Y == rowY &&
                cellPosition.X >= TravelScreenLayout.DestinationStartX &&
                cellPosition.X < Surface.Width - 2)
            {
                var clickedIndex = firstVisibleIndex + rowIndex;
                if (_selectedIndex == clickedIndex)
                {
                    TravelToSelected();
                }
                else
                {
                    _selectedIndex = clickedIndex;
                    _mouseSelectedIndex = clickedIndex;
                }

                return true;
            }
        }

        var detailStartY = TravelScreenLayout.GetDetailStartY(Surface.Height);
        if (cellPosition.Y == detailStartY + 2)
        {
            var transportLabel = "Transport:";
            if (cellPosition.X >= 2 && cellPosition.X < 2 + transportLabel.Length + 10)
            {
                TravelToSelected();
                return true;
            }

            var walkPrefix = $"Transport: {_gameState.GetTravelCost(_locations[_selectedIndex].Id)} LE / {_gameState.GetTravelTimeMinutes(_locations[_selectedIndex].Id)} min | ";
            if (cellPosition.X >= 2 + walkPrefix.Length && cellPosition.X < 2 + walkPrefix.Length + 10)
            {
                WalkToSelected();
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
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }

    private void RenderSelectedLocationDetails(int detailStartY)
    {
        if (_locations.Count == 0)
        {
            return;
        }

        var selectedLocation = _locations[_selectedIndex];
        var travelCost = _gameState.GetTravelCost(selectedLocation.Id);
        var travelMinutes = _gameState.GetTravelTimeMinutes(selectedLocation.Id);
        var walkMinutes = _gameState.GetWalkTimeMinutes(selectedLocation.Id);
        var travelSummary = _gameState.GetTravelConditionSummary(selectedLocation.Id) ?? selectedLocation.Description;

        Surface.Print(2, detailStartY, $"Selected: {selectedLocation.Name}", Color.White);

        var travelHints = _tipContextQuery.GetTravelHints(_gameState);
        if (travelHints.Count > 0)
        {
            var hintY = detailStartY + 1;
            for (var t = 0; t < Math.Min(travelHints.Count, 2); t++)
            {
                var hint = travelHints[t];
                var hintColor = hint.IsEmergency ? Color.Red : Color.Orange;
                Surface.Print(2, hintY + t, TrimToFit($"! {hint.Content}", Surface.Width - 4), hintColor);
            }

            Surface.Print(2, detailStartY + 1 + Math.Min(travelHints.Count, 2), TrimToFit(travelSummary, Surface.Width - 4), Color.DarkGray);
        }
        else
        {
            Surface.Print(2, detailStartY + 1, TrimToFit(travelSummary, Surface.Width - 4), Color.DarkGray);
        }

        Surface.Print(2, detailStartY + 2, $"[Transport]: {travelCost} LE / {travelMinutes} min | [Walk]: {walkMinutes} min (free)", Color.Yellow);

        var currentMinutes = _gameState.Clock.Hour * 60 + _gameState.Clock.Minute;
        var arrivalHour = (currentMinutes + travelMinutes) / 60;
        var arrivalMinute = (currentMinutes + travelMinutes) % 60;
        if (arrivalHour < 24)
        {
            Surface.Print(2, detailStartY + 3, $"Arrive by {arrivalHour:D2}:{arrivalMinute:D2} via transport", Color.Gray);
        }

        var walkArrivalHour = (currentMinutes + walkMinutes) / 60;
        var walkArrivalMinute = (currentMinutes + walkMinutes) % 60;
        if (walkArrivalHour < 24)
        {
            Surface.Print(2, detailStartY + 4, $"Arrive by {walkArrivalHour:D2}:{walkArrivalMinute:D2} via walk", Color.Gray);
        }
    }

    private void RenderScrollBar(int firstVisibleIndex, int visibleCount)
    {
        var scrollBarX = Surface.Width - TravelScreenLayout.ScrollBarXOffset;
        var thumbSize = TravelScreenLayout.GetScrollThumbSize(visibleCount, _locations.Count);
        var thumbOffset = TravelScreenLayout.GetScrollThumbOffset(firstVisibleIndex, visibleCount, _locations.Count, thumbSize);

        for (var rowIndex = 0; rowIndex < visibleCount; rowIndex++)
        {
            var rowY = TravelScreenLayout.DestinationStartY + rowIndex;
            var isThumbRow = rowIndex >= thumbOffset && rowIndex < thumbOffset + thumbSize;
            Surface.Print(scrollBarX, rowY, isThumbRow ? "#" : "|", isThumbRow ? Color.Cyan : Color.DarkGray);
        }
    }

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }
}
