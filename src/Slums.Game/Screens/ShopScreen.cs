using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Expenses;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class ShopScreen : ScreenSurface
{
    private const int OptionsStartX = 2;
    private const int OptionsStartY = 8;
    private readonly GameState _gameState;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    private static readonly string[] Options = ["Buy Food (15 LE)", "Buy Medicine (50 LE)", "Cancel"];

    public ShopScreen(int width, int height, GameState gameState, GameScreen parentScreen) 
        : base(width, height)
    {
        _gameState = gameState;
        _parentScreen = parentScreen;
        _selectedIndex = 0;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        var y = 2;
        Surface.Print(OptionsStartX, y++, "=== Shop ===", Color.Cyan);
        y++;

        Surface.Print(OptionsStartX, y++, $"Your Money: {_gameState.Player.Stats.Money} LE", Color.Gold);
        Surface.Print(OptionsStartX, y++, $"Food Stockpile: {_gameState.Player.Household.FoodStockpile}", Color.White);
        Surface.Print(OptionsStartX, y++, $"Mother's Health: {_gameState.Player.Household.MotherHealth}%", 
            _gameState.Player.Household.MotherNeedsCare ? Color.Red : Color.Green);
        y = OptionsStartY;

        for (var i = 0; i < Options.Length; i++)
        {
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;

            var canAfford = i switch
            {
                0 => _gameState.Player.Stats.Money >= RecurringExpenses.CheapFoodStockpile,
                1 => _gameState.Player.Stats.Money >= RecurringExpenses.MedicineCost,
                _ => true
            };

            if (!canAfford && i < 2)
            {
                color = Color.DarkGray;
            }

            Surface.Print(OptionsStartX, y++, $"{prefix}{Options[i]}", color);
        }

        y++;
        Surface.Print(OptionsStartX, y++, "Arrow keys to select, Enter to buy, Escape to cancel", Color.DarkGray);
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
            ExecuteSelection();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            ReturnToParentScreen();
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
        for (var i = 0; i < Options.Length; i++)
        {
            var endX = OptionsStartX + Options[i].Length + 2;
            if (cellPosition.Y == OptionsStartY + i && cellPosition.X >= OptionsStartX && cellPosition.X < endX)
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
                _gameState.BuyFood();
                break;
            case 1:
                _gameState.BuyMedicine();
                break;
            case 2:
                ReturnToParentScreen();
                return;
        }

        ReturnToParentScreen();
    }

    private void ReturnToParentScreen()
    {
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
