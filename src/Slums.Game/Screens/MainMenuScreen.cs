using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;

namespace Slums.Game.Screens;

internal sealed class MainMenuScreen : ScreenSurface
{
    private static readonly string[] MenuItems = ["New Game", "Load Game", "Quit"];
    private readonly GameRuntime _runtime;
    private int _selectedIndex;

    public MainMenuScreen(int width, int height, GameRuntime runtime) : base(width, height)
    {
        _runtime = runtime;
        _selectedIndex = 0;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);

        var centerX = Surface.Width / 2;
        var startY = Surface.Height / 2 - MenuItems.Length / 2;

        Surface.Clear();

        const string title = "SLUMS";
        Surface.Print(centerX - title.Length / 2, startY - 4, title, Color.Yellow);

        const string subtitle = "A Cairo Survival Story";
        Surface.Print(centerX - subtitle.Length / 2, startY - 2, subtitle, Color.Gray);

        for (var i = 0; i < MenuItems.Length; i++)
        {
            var item = MenuItems[i];
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            var prefix = i == _selectedIndex ? "> " : "  ";
            Surface.Print(centerX - item.Length / 2 - 1, startY + i, prefix + item, color);
        }

        const string instructions = "Use arrow keys to navigate, Enter to select";
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

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        var handled = base.ProcessMouse(state);
        if (!state.IsOnScreenObject || !state.Mouse.LeftClicked)
        {
            return handled;
        }

        var cellPosition = state.SurfaceCellPosition;
        var centerX = Surface.Width / 2;
        var startY = Surface.Height / 2 - MenuItems.Length / 2;

        for (var i = 0; i < MenuItems.Length; i++)
        {
            var startX = centerX - MenuItems[i].Length / 2 - 1;
            var endX = startX + MenuItems[i].Length + 2;
            if (cellPosition.Y == startY + i && cellPosition.X >= startX && cellPosition.X < endX)
            {
                _selectedIndex = i;
                ExecuteSelection();
                return true;
            }
        }

        return handled;
    }

    private void ExecuteSelection()
    {
        switch (_selectedIndex)
        {
            case 0:
                IsFocused = false;
                var newSession = new Slums.Core.State.GameSession(_runtime.RandomSource.SharedRandom);
                _runtime.MutationLogger.Attach(newSession);
                GameHost.Instance.Screen = new BackgroundSelectionScreen(
                    GameRuntime.ScreenWidth,
                    GameRuntime.ScreenHeight,
                    _runtime,
                    newSession);
                break;
            case 1:
                IsFocused = false;
                GameHost.Instance.Screen = new LoadGameScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime);
                break;
            case 2:
                Environment.Exit(0);
                break;
        }
    }
}
