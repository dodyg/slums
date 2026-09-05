using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Technology;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class TechnicalRepairScreen : ScreenSurface
{
    private readonly GameSession _gameState;
    private readonly TechnicalRepairMenuContext _context;
    private readonly List<TechnicalRepairMenuStatus> _actions;
    private readonly GameScreen _parentScreen;
    private readonly TechnicalRepairCommand _command = new();
    private int _selectedIndex;

    public TechnicalRepairScreen(int width, int height, GameSession gameState, TechnicalRepairMenuContext context, List<TechnicalRepairMenuStatus> actions, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _actions = actions;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();
        Surface.Print(2, 2, "=== Technical Repair ===", Color.Cyan);
        Surface.Print(2, 3, $"Technical Repair: {_context.TechnicalRepairSkillLevel} | Parts: {_context.SpareParts} | Handset: {_context.HandsetCondition}% | Solar storage: {_context.SolarStorageCondition}%", Color.Gray);

        for (var i = 0; i < _actions.Count; i++)
        {
            var status = _actions[i];
            var color = i == _selectedIndex ? Color.Cyan : status.CanPerform ? Color.White : Color.Gray;
            var line = status.CanPerform
                ? $"{status.Preview.Action.PartsRequired} parts | {status.Preview.Action.MoneyCost} LE | {status.Preview.Action.TimeCostMinutes}m | +{status.Preview.ConditionGain}% / +{status.Preview.Income} LE"
                : status.UnavailabilityReason ?? "Unavailable";
            Surface.Print(2, 5 + i * 2, $"{(i == _selectedIndex ? "> " : "  ")}{status.Preview.Action.Name}", color);
            Surface.Print(4, 6 + i * 2, line, status.CanPerform ? Color.Green : Color.Orange);
        }

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to repair, Escape to cancel", Color.DarkGray);
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
