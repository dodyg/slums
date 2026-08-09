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
    private Task<IReadOnlyList<SaveSlotMetadata>>? _pendingSlots;
    private Task<LoadGameResult>? _pendingLoad;

    public LoadGameScreen(int width, int height, GameRuntime runtime)
        : base(width, height)
    {
        _runtime = runtime;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        RefreshSlotsAsync();
    }

    public override void Update(TimeSpan delta)
    {
        base.Update(delta);

        if (_pendingSlots is { IsCompleted: true })
        {
            var task = _pendingSlots;
            _pendingSlots = null;

            if (task.IsFaulted)
            {
                _statusMessage = "Could not list save slots.";
            }
            else
            {
                _slots = task.GetAwaiter().GetResult();
                _selectedIndex = Math.Clamp(_selectedIndex, 0, Math.Max(0, _slots.Count - 1));
            }
        }

        if (_pendingLoad is { IsCompleted: true })
        {
            var task = _pendingLoad;
            _pendingLoad = null;

            if (task.IsFaulted)
            {
                _statusMessage = "Failed to load save.";
                RefreshSlotsAsync();
            }
            else
            {
                HandleLoadResult(task.GetAwaiter().GetResult());
            }
        }
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
            RenderStatus();
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

        RenderStatus();

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to load, Escape to cancel", Color.DarkGray);
    }

    private void RenderStatus()
    {
        if (!string.IsNullOrWhiteSpace(_statusMessage))
        {
            Surface.Print(2, Surface.Height - 4, _statusMessage, Color.Yellow);
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (_pendingLoad is not null)
        {
            return true;
        }

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
            _statusMessage = "Loading...";
            _pendingLoad = _runtime.LoadGameUseCase.ExecuteAsync(slot.Slot);
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            ReturnToMainMenu();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void HandleLoadResult(LoadGameResult result)
    {
        switch (result.Kind)
        {
            case LoadGameResultKind.Loaded when result.Session is not null:
                using (result.Session)
                {
                    _runtime.NarrativeService.RestoreProgress(result.Session.LastKnot);
                    var gameSession = result.Session.TakeGameSession();
                    _runtime.MutationLogger.Attach(gameSession);
                    ScreenTransition.FadeTo(new GameScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, gameSession));
                }

                break;
            case LoadGameResultKind.Missing:
                _statusMessage = "Save not found.";
                RefreshSlotsAsync();
                break;
            case LoadGameResultKind.Corrupt:
                _statusMessage = "Save is corrupted and could not be loaded.";
                RefreshSlotsAsync();
                break;
            case LoadGameResultKind.Incompatible:
                _statusMessage = result.Detail ?? "Save is from an incompatible version.";
                RefreshSlotsAsync();
                break;
        }
    }

    private void RefreshSlotsAsync()
    {
        _pendingSlots = _runtime.SaveGameStore.ListSlotsAsync();
    }

    private void ReturnToMainMenu()
    {
        ScreenTransition.SwitchTo(new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime));
    }
}
