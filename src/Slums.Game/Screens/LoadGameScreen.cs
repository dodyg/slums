using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Persistence;

namespace Slums.Game.Screens;

internal sealed class LoadGameScreen : ScreenSurface
{
    private readonly GameRuntime _runtime;
    private IReadOnlyList<SaveSlotMetadata> _slots = [];
    private string? _statusMessage;
    private int _selectedIndex;

    public LoadGameScreen(int width, int height, GameRuntime runtime)
        : base(width, height)
    {
        _runtime = runtime;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        RefreshSlots();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(2, 2, "=== Load Game ===", Color.Cyan);
        if (_slots.Count == 0)
        {
            Surface.Print(2, 5, "No save slots found.", Color.Orange);
            Surface.Print(2, Surface.Height - 2, "Press Escape to return", Color.DarkGray);
            return;
        }

        var y = 5;
        for (var i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            Surface.Print(2, y++, $"{prefix}{slot.Slot}", color);
            Surface.Print(4, y++, $"{slot.CheckpointName} | {slot.LastPlayedUtc.LocalDateTime:g}", Color.Gray);
        }

        if (!string.IsNullOrWhiteSpace(_statusMessage))
        {
            Surface.Print(2, Surface.Height - 4, _statusMessage, Color.Yellow);
        }

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to load, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (_slots.Count == 0)
        {
            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                ReturnToMainMenu();
                return true;
            }

            return base.ProcessKeyboard(keyboard);
        }

        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _slots.Count) % _slots.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _slots.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            var slot = _slots[_selectedIndex];
            var loadedGame = _runtime.LoadGameUseCase.ExecuteAsync(slot.Slot).GetAwaiter().GetResult();
            if (loadedGame is null)
            {
                _statusMessage = "Failed to load save.";
                RefreshSlots();
                return true;
            }

            using (loadedGame)
            {
                _runtime.NarrativeService.RestoreProgress(loadedGame.LastKnot);
                var gameSession = loadedGame.TakeGameSession();
                try
                {
                    IsFocused = false;
                    GameHost.Instance.Screen = new GameScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, gameSession);
                }
                catch
                {
                    gameSession.Dispose();
                    throw;
                }
            }

            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            ReturnToMainMenu();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void RefreshSlots()
    {
        _slots = _runtime.SaveGameStore.ListSlotsAsync().GetAwaiter().GetResult();
        _selectedIndex = Math.Clamp(_selectedIndex, 0, Math.Max(0, _slots.Count - 1));
    }

    private void ReturnToMainMenu()
    {
        IsFocused = false;
        GameHost.Instance.Screen = new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime);
    }
}
