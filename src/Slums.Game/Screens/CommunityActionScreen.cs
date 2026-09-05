using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class CommunityActionScreen : ScreenSurface
{
    private readonly GameSession _gameState;
    private readonly CommunityActionMenuContext _context;
    private readonly List<CommunityActionMenuStatus> _actions;
    private readonly GameScreen _parentScreen;
    private readonly CommunityActionCommand _command = new();
    private int _selectedIndex;

    public CommunityActionScreen(int width, int height, GameSession gameState, CommunityActionMenuContext context, List<CommunityActionMenuStatus> actions, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _actions = actions;
        _parentScreen = parentScreen;
        IsFocused = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();
        Surface.Print(2, 2, "=== Community Adaptation ===", Color.Cyan);
        Surface.Print(2, 3, $"Organizing: {_context.CommunityOrganizingSkillLevel} | Cooling room: {_context.CoolingRoomDaysRemaining}d | Water reserve: {_context.WaterReserveUnits}", Color.Gray);
        for (var i = 0; i < _actions.Count; i++)
        {
            var status = _actions[i];
            var color = i == _selectedIndex ? Color.Cyan : status.CanPerform ? Color.White : Color.Gray;
            var reason = status.CanPerform ? $"{status.Preview.Action.MoneyCost} LE | {status.Preview.Action.EnergyCost} energy" : status.UnavailabilityReason ?? "Unavailable";
            Surface.Print(2, 5 + (i * 2), $"{(i == _selectedIndex ? "> " : "  ")}{status.Preview.Action.Name}", color);
            Surface.Print(4, 6 + (i * 2), reason, status.CanPerform ? Color.Green : Color.Orange);
        }
        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to coordinate, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _actions.Count) % _actions.Count;
            return true;
        }
        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _actions.Count;
            return true;
        }
        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            var selected = _actions[_selectedIndex];
            if (selected.CanPerform)
            {
                _command.Execute(_gameState, selected.Preview.Action.Type);
                ReturnToParentScreen();
            }
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
        ScreenTransition.ReturnTo(_parentScreen);
    }
}
