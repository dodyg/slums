using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class GenderSelectionScreen : ScreenSurface
{
    private readonly GameRuntime _runtime;
    private readonly GameSession _gameState;
    private int _selectedIndex;

    private static readonly (Gender Gender, string Label, string Description)[] Options =
    [
        (Gender.Female, "Amira  —  a young woman",
            "The streets of Cairo have their own rules for women. Some doors are harder to open, others you slip through unseen. Your neighbors trust you faster. The physical trades wear you differently."),
        (Gender.Male, "Karim  —  a young man",
            "The workshop boss takes you seriously. The depot dispatcher nods you through. But suspicion follows men more readily in the wrong corridors, and some community bonds take longer to build.")
    ];

    public GenderSelectionScreen(int width, int height, GameRuntime runtime, GameSession gameState) : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
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
        var y = 2;

        Surface.Print(centerX - 12, y, "=== WHO ARE YOU? ===", Color.Yellow);
        y += 2;

        Surface.Print(2, y, "Choose your character's gender. This affects job opportunities, social dynamics, and how Cairo treats you.", Color.Gray);
        y += 2;

        for (var i = 0; i < Options.Length; i++)
        {
            var option = Options[i];
            var isSelected = i == _selectedIndex;
            var color = isSelected ? Color.Cyan : Color.White;
            var prefix = isSelected ? "> " : "  ";

            Surface.Print(2, y, prefix + option.Label, color);
            y++;

            var wrapped = WrapText(option.Description, Surface.Width - 6);
            foreach (var line in wrapped)
            {
                Surface.Print(4, y, line, isSelected ? Color.LightGray : Color.DarkGray);
                y++;
            }

            y++;
        }

        const string instructions = "Arrow keys to navigate, Enter to select, ESC to go back";
        Surface.Print(centerX - instructions.Length / 2, Surface.Height - 2, instructions, Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + Options.Length) % Options.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % Options.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            ConfirmSelection();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
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

        for (var i = 0; i < Options.Length; i++)
        {
            var lines = WrapText(Options[i].Description, Surface.Width - 6);
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
        _gameState.Player.ApplyGender(Options[_selectedIndex].Gender);
        _gameState.ApplyGenderRelationshipModifiers();
        ScreenTransition.FadeTo(new BackgroundSelectionScreen(
            GameRuntime.ScreenWidth,
            GameRuntime.ScreenHeight,
            _runtime,
            _gameState));
    }

    private static string[] WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            if (testLine.Length > maxWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine);
        }

        return [.. lines];
    }
}
