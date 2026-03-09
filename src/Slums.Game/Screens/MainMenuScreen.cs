using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace Slums.Game.Screens;

internal sealed class MainMenuScreen : ScreenSurface
{
    private static readonly string[] MenuItems = ["New Game", "Load Game", "Quit"];
    private int _selectedIndex;

    public MainMenuScreen(int width, int height) : base(width, height)
    {
        _selectedIndex = 0;
        IsFocused = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        
        var centerX = Surface.Width / 2;
        var startY = Surface.Height / 2 - MenuItems.Length / 2;
        
        Surface.Clear();
        
        var title = "SLUMS";
        Surface.Print(centerX - title.Length / 2, startY - 4, title, Color.Yellow);
        
        var subtitle = "A Cairo Survival Story";
        Surface.Print(centerX - subtitle.Length / 2, startY - 2, subtitle, Color.Gray);
        
        for (var i = 0; i < MenuItems.Length; i++)
        {
            var item = MenuItems[i];
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            var prefix = i == _selectedIndex ? "> " : "  ";
            Surface.Print(centerX - item.Length / 2 - 1, startY + i, prefix + item, color);
        }
        
        var instructions = "Use arrow keys to navigate, Enter to select";
        Surface.Print(centerX - instructions.Length / 2, startY + MenuItems.Length + 2, instructions, Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + MenuItems.Length) % MenuItems.Length;
            return true;
        }
        
        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % MenuItems.Length;
            return true;
        }
        
        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            ExecuteSelection();
            return true;
        }
        
        return base.ProcessKeyboard(keyboard);
    }

    private void ExecuteSelection()
    {
        switch (_selectedIndex)
        {
            case 0:
                GameHost.Instance.Screen = new GameScreen(80, 25);
                break;
            case 1:
                break;
            case 2:
                Environment.Exit(0);
                break;
        }
    }
}
