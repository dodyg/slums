using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Endings;
using Slums.Core.State;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class EndingChoiceScreen : ScreenSurface
{
    private readonly GameSession _gameState;
    private readonly IReadOnlyList<EndingChoiceOption> _options;
    private readonly GameScreen _parentScreen;
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private int _selectedIndex;

    public EndingChoiceScreen(int width, int height, GameSession gameState, IReadOnlyList<EndingChoiceOption> options, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _options = options;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();
        Surface.Print(2, 2, "=== Long-Term Paths ===", Color.Cyan);
        Surface.Print(2, 4, "Conditions have opened these choices. Choose deliberately.", Color.Gray);

        for (var index = 0; index < _options.Count; index++)
        {
            var option = _options[index];
            var color = index == _selectedIndex ? Color.Cyan : Color.White;
            Surface.Print(2, 7 + (index * 3), index == _selectedIndex ? "> " + option.Label : "  " + option.Label, color);
            Surface.Print(5, 8 + (index * 3), option.Requirements, Color.Gray);
        }

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to commit, Escape to return", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _options.Count) % _options.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _options.Count;
            return true;
        }

        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            if (EndingChoiceCommand.Execute(_gameState, _options[_selectedIndex].Id))
            {
                ScreenTransition.ReturnTo(_parentScreen);
            }

            return true;
        }

        if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
        {
            ScreenTransition.ReturnTo(_parentScreen);
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }
}
