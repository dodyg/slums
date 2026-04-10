using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Home;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class HomeUpgradeScreen : ScreenSurface
{
    private const int OptionsStartX = 2;
    private const int OptionsStartY = 10;
    private const string CancelOptionLabel = "Cancel";
    private readonly GameSession _gameState;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public HomeUpgradeScreen(int width, int height, GameSession gameState, GameScreen parentScreen)
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
        var availableUpgrades = _gameState.GetAvailableHomeUpgrades();
        var optionCount = availableUpgrades.Count + 1;
        if (_selectedIndex >= optionCount)
        {
            _selectedIndex = optionCount - 1;
        }

        var y = 2;
        Surface.Print(OptionsStartX, y++, "=== Home Improvement ===", Color.Cyan);
        y++;

        Surface.Print(OptionsStartX, y++, $"Your Money: {_gameState.Player.Stats.Money} LE", Color.Gold);
        Surface.Print(OptionsStartX, y++, $"Owned Upgrades: {FormatOwnedUpgrades()}", Color.White);
        y++;

        var currentBonus = _gameState.HomeUpgrades.GetEnergyRecoveryBonus();
        Surface.Print(OptionsStartX, y++, $"Energy Recovery Bonus: +{currentBonus}", Color.Green);

        if (_gameState.HomeUpgrades.GetStressBonus() > 0)
        {
            Surface.Print(OptionsStartX, y++, $"Stress Reduction: -{_gameState.HomeUpgrades.GetStressBonus()}/day", Color.Green);
        }

        y = OptionsStartY;

        for (var i = 0; i < optionCount; i++)
        {
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            string label;
            if (i < availableUpgrades.Count)
            {
                var upgrade = availableUpgrades[i];
                var cost = HomeUpgradeDefinitions.GetCost(upgrade);
                var canAfford = _gameState.Player.Stats.Money >= cost;
                label = $"{upgrade} ({cost} LE) - {GetShortDescription(upgrade)}";
                if (!canAfford)
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
        if (_selectedIndex < availableUpgrades.Count)
        {
            var selectedUpgrade = availableUpgrades[_selectedIndex];
            Surface.Print(OptionsStartX, y++, HomeUpgradeDefinitions.GetDescription(selectedUpgrade), Color.DarkGray);
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

        var availableUpgrades = _gameState.GetAvailableHomeUpgrades();
        var cellPosition = state.SurfaceCellPosition;
        for (var i = 0; i < availableUpgrades.Count + 1; i++)
        {
            if (cellPosition.Y == OptionsStartY + i && cellPosition.X >= OptionsStartX)
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
        var availableUpgrades = _gameState.GetAvailableHomeUpgrades();
        if (_selectedIndex >= availableUpgrades.Count)
        {
            ReturnToParentScreen();
            return;
        }

        var upgrade = availableUpgrades[_selectedIndex];
        _gameState.TryPurchaseHomeUpgrade(upgrade);
        ReturnToParentScreen();
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }

    private string FormatOwnedUpgrades()
    {
        var owned = _gameState.HomeUpgrades.PurchasedUpgrades;
        return owned.Count == 0 ? "None" : string.Join(", ", owned);
    }

    private static string GetShortDescription(HomeUpgrade upgrade) => upgrade switch
    {
        HomeUpgrade.CleanBedding => "+2 energy recovery",
        HomeUpgrade.Fan => "+3 energy (summer), +1 otherwise",
        HomeUpgrade.WindowScreen => "+1 energy, -2 stress/day",
        HomeUpgrade.Curtain => "+1 energy, +1 privacy",
        _ => ""
    };

    private int GetOptionCount()
    {
        return _gameState.GetAvailableHomeUpgrades().Count + 1;
    }
}
