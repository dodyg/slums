using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Characters;
using Slums.Application.Narrative;
using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Game.Rendering;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class BackgroundSelectionScreen : ScreenSurface
{
    private readonly GameRuntime _runtime;
    private readonly GameSession _gameState;
    private int _selectedIndex;
    private readonly ScreenActionKeyGate _actionKeyGate = new();

    public BackgroundSelectionScreen(int width, int height, GameRuntime runtime, GameSession gameState) : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
        _selectedIndex = 0;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();
        var backgrounds = _gameState.ContentCatalog.Backgrounds;

        var centerX = Surface.Width / 2;
        var y = 1;

        Surface.Print(centerX - 11, y, "=== CHOOSE YOUR PAST ===", Color.Yellow);
        y += 2;

        Surface.Print(2, y, "Your background shapes your starting conditions:", Color.Gray);
        y += 2;

        for (var i = 0; i < backgrounds.Count; i++)
        {
            var bg = backgrounds[i];
            var isSelected = i == _selectedIndex;
            var color = isSelected ? Color.Cyan : Color.White;
            var prefix = isSelected ? "> " : "  ";

            Surface.Print(2, y, prefix + bg.Name, color);
            y++;

            var wrappedDesc = TextWrap.WrapText(bg.Description, Surface.Width - 6);
            foreach (var line in wrappedDesc)
            {
                Surface.Print(4, y, line, isSelected ? Color.LightGray : Color.DarkGray);
                y++;
            }

            y++;
        }

        RenderStatsPreview(centerX, y);

        const string instructions = "Arrow keys to navigate, Enter to select, ESC to go back";
        Surface.Print(centerX - instructions.Length / 2, Surface.Height - 2, instructions, Color.DarkGray);
    }

    private void RenderStatsPreview(int centerX, int startY)
    {
        var y = startY;
        var selected = _gameState.ContentCatalog.Backgrounds[_selectedIndex];

        Surface.Print(centerX - 10, y, "--- Starting Stats ---", Color.Cyan);
        y++;

        Surface.Print(centerX - 15, y, $"Money: {selected.StartingMoney} LE", Color.Gold);
        y++;
        Surface.Print(centerX - 15, y, $"Health: {selected.StartingHealth}%", GetStatColor(selected.StartingHealth));
        y++;
        Surface.Print(centerX - 15, y, $"Energy: {selected.StartingEnergy}%", GetStatColor(selected.StartingEnergy));
        y++;
        Surface.Print(centerX - 15, y, $"Mother's Health: {selected.MotherStartingHealth}%", GetStatColor(selected.MotherStartingHealth));
        y++;
        Surface.Print(centerX - 15, y, $"Food Stock: {selected.FoodStockpile}", Color.White);
    }

    private static Color GetStatColor(int value) => value switch
    {
        < 30 => Color.Red,
        < 60 => Color.Orange,
        _ => Color.Green
    };

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _gameState.ContentCatalog.Backgrounds.Count) % _gameState.ContentCatalog.Backgrounds.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _gameState.ContentCatalog.Backgrounds.Count;
            return true;
        }

        var numberIndex = NumberKeyMapper.GetPressedNumberIndex(keyboard, _gameState.ContentCatalog.Backgrounds.Count);
        if (numberIndex is int selectedIndex)
        {
            _selectedIndex = selectedIndex;
            return true;
        }

        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            ConfirmSelection();
            return true;
        }

        if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
        {
            ScreenTransition.SwitchTo(new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime));
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
        var y = 5;

        var backgrounds = _gameState.ContentCatalog.Backgrounds;
        for (var i = 0; i < backgrounds.Count; i++)
        {
            var bg = backgrounds[i];
            var lines = TextWrap.WrapText(bg.Description, Surface.Width - 6).ToArray();
            var totalHeight = 2 + lines.Length + 1;

            if (cellPosition.Y >= y && cellPosition.Y < y + totalHeight)
            {
                _selectedIndex = i;
                if (cellPosition.Y == y || cellPosition.Y == y + 1)
                {
                    ConfirmSelection();
                }
                return true;
            }

            y += totalHeight;
        }

        return handled;
    }

    private void ConfirmSelection()
    {
        var selectedBackground = _gameState.ContentCatalog.Backgrounds[_selectedIndex];
        new SelectBackgroundCommand().Execute(_gameState, selectedBackground);
        var nextScreen = new GameScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState);
        _runtime.NarrativeService.StartScene(selectedBackground.InkIntroKnot, NarrativeSceneState.Create(_gameState));
        ScreenTransition.FadeTo(new NarrativeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime.NarrativeService, _gameState, nextScreen));
    }
}
