using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Persistence;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class SaveGameScreen : ScreenSurface
{
    private static readonly string[] Slots = ["slot1", "slot2", "slot3"];
    private readonly GameRuntime _runtime;
    private readonly GameSession _gameState;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public SaveGameScreen(int width, int height, GameRuntime runtime, GameSession gameState, GameScreen parentScreen)
        : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(2, 2, "=== Save Game ===", Color.Cyan);
        Surface.Print(2, 4, "Choose a slot:", Color.Gray);

        var y = 6;
        for (var i = 0; i < Slots.Length; i++)
        {
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            Surface.Print(2, y++, $"{prefix}{Slots[i]}", color);
        }

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to save, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + Slots.Length) % Slots.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % Slots.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            var slot = Slots[_selectedIndex];
            _runtime.SaveGameUseCase.ExecuteAsync(SaveGameRequest.Create(_gameState, _runtime.NarrativeService.LastKnot), slot).GetAwaiter().GetResult();
            _gameState.AddEventMessage($"Saved game to {slot}.");
            ReturnToParentScreen();
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
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
