using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Application.HouseholdAssets;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class ShopScreen : ScreenSurface
{
    private const int OptionsStartX = 2;
    private const int OptionsStartY = 8;
    private const string CancelOptionLabel = "Cancel";
    private readonly ShopMenuContext _context;
    private readonly GameSession _gameState;
    private readonly GameScreen _parentScreen;
    private readonly ShopCommand _shopCommand = new();
    private readonly ShopMenuStatusQuery _shopMenuStatusQuery = new();
    private readonly HouseholdAssetsMenuQuery _householdAssetsMenuQuery = new();
    private int _selectedIndex;

    public ShopScreen(int width, int height, GameSession gameState, ShopMenuContext context, GameScreen parentScreen) 
        : base(width, height)
    {
        _context = context;
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
        var purchaseOptions = GetPurchaseOptions();
        var optionCount = purchaseOptions.Count + 1;
        if (_selectedIndex >= optionCount)
        {
            _selectedIndex = optionCount - 1;
        }

        var y = 2;
        Surface.Print(OptionsStartX, y++, "=== Shop ===", Color.Cyan);
        y++;

        Surface.Print(OptionsStartX, y++, $"Your Money: {_context.Money} LE", Color.Gold);
        Surface.Print(OptionsStartX, y++, $"Food Stockpile: {_context.FoodStockpile}", Color.White);
        Surface.Print(OptionsStartX, y++, $"Mother's Health: {_context.MotherHealth}%", 
            _context.MotherNeedsCare ? Color.Red : Color.Green);
        if (_context.HasClinicServices)
        {
            var clinicColor = _context.ClinicOpenToday ? Color.Green : Color.Orange;
            var clinicLine = _context.ClinicOpenToday
                ? $"Clinic: Open today ({_context.ClinicVisitCost} LE)"
                : $"Clinic: Closed today ({_context.ClinicOpenDaysSummary})";
            Surface.Print(OptionsStartX, y++, clinicLine, clinicColor);
        }
        y = OptionsStartY;

        for (var i = 0; i < optionCount; i++)
        {
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            string label;
            if (i < purchaseOptions.Count)
            {
                var option = purchaseOptions[i];
                label = FormatOptionLabel(option);
                if (!option.CanAfford)
                {
                    color = Color.DarkGray;
                }
            }
            else
            {
                label = CancelOptionLabel;
            }

            Surface.Print(OptionsStartX, y++, $"{prefix}{label}", color);
        }

        y++;
        var selectedOption = _selectedIndex < purchaseOptions.Count ? purchaseOptions[_selectedIndex] : null;
        if (!string.IsNullOrWhiteSpace(selectedOption?.Note))
        {
            Surface.Print(OptionsStartX, y++, selectedOption.Note, Color.DarkGray);
        }

        Surface.Print(OptionsStartX, y++, "Arrow keys to select, Enter to buy, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        var optionCount = GetOptionCount();
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + optionCount) % optionCount;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % optionCount;
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

        var purchaseOptions = GetPurchaseOptions();
        var cellPosition = state.SurfaceCellPosition;
        for (var i = 0; i < purchaseOptions.Count + 1; i++)
        {
            var label = i < purchaseOptions.Count
                ? FormatOptionLabel(purchaseOptions[i])
                : CancelOptionLabel;
            var endX = OptionsStartX + label.Length + 2;
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
        var purchaseOptions = GetPurchaseOptions();
        if (_selectedIndex >= purchaseOptions.Count)
        {
            ReturnToParentScreen();
            return;
        }

        var selectedOption = purchaseOptions[_selectedIndex];
        if (selectedOption.OptionId == ShopOptionId.OpenHouseholdAssets)
        {
            OpenHouseholdAssetsScreen();
            return;
        }

        _shopCommand.Execute(_gameState, selectedOption.OptionId);
        ReturnToParentScreen();
    }

    private void OpenHouseholdAssetsScreen()
    {
        var householdContext = HouseholdAssetsMenuContext.Create(_gameState);
        var statuses = _householdAssetsMenuQuery.GetStatuses(householdContext).ToList();
        if (statuses.Count == 0)
        {
            ReturnToParentScreen();
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new HouseholdAssetsScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, householdContext, statuses, _parentScreen);
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }

    private IReadOnlyList<ShopMenuStatus> GetPurchaseOptions()
    {
        return _shopMenuStatusQuery.GetStatuses(_context);
    }

    private static string FormatOptionLabel(ShopMenuStatus option)
    {
        return option.Cost > 0
            ? $"{option.Name} ({option.Cost} LE)"
            : option.Name;
    }

    private int GetOptionCount()
    {
        return GetPurchaseOptions().Count + 1;
    }
}
