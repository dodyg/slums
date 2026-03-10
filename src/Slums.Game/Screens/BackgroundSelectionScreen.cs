using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Characters;

namespace Slums.Game.Screens;

internal sealed class BackgroundSelectionScreen : ScreenSurface
{
    private static readonly Background[] Backgrounds = BackgroundRegistry.AllBackgrounds.ToArray();
    private int _selectedIndex;

    public BackgroundSelectionScreen(int width, int height) : base(width, height)
    {
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
        var y = 1;

        Surface.Print(centerX - 11, y, "=== CHOOSE YOUR PAST ===", Color.Yellow);
        y += 2;

        Surface.Print(2, y, "Your background shapes your starting conditions:", Color.Gray);
        y += 2;

        for (var i = 0; i < Backgrounds.Length; i++)
        {
            var bg = Backgrounds[i];
            var isSelected = i == _selectedIndex;
            var color = isSelected ? Color.Cyan : Color.White;
            var prefix = isSelected ? "> " : "  ";

            Surface.Print(2, y, prefix + bg.Name, color);
            y++;

            var wrappedDesc = WrapText(bg.Description, Surface.Width - 6);
            foreach (var line in wrappedDesc)
            {
                Surface.Print(4, y, line, isSelected ? Color.LightGray : Color.DarkGray);
                y++;
            }

            y++;
        }

        RenderStatsPreview(centerX, y);

        var instructions = "Arrow keys to navigate, Enter to select, ESC to go back";
        Surface.Print(centerX - instructions.Length / 2, Surface.Height - 2, instructions, Color.DarkGray);
    }

    private void RenderStatsPreview(int centerX, int startY)
    {
        var y = startY;
        var selected = Backgrounds[_selectedIndex];

        Surface.Print(centerX - 10, y, "--- Starting Stats ---", Color.Cyan);
        y++;

        Surface.Print(centerX - 15, y, $"Money: {selected.StartingMoney} EGP", Color.Gold);
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

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + Backgrounds.Length) % Backgrounds.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % Backgrounds.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            ConfirmSelection();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            GameHost.Instance.Screen = new MainMenuScreen(80, 25);
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

        for (var i = 0; i < Backgrounds.Length; i++)
        {
            var bg = Backgrounds[i];
            var lines = WrapText(bg.Description, Surface.Width - 6);
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
        var selectedBackground = Backgrounds[_selectedIndex];
        GameHost.Instance.Screen = new GameScreen(80, 25, selectedBackground);
    }
}
