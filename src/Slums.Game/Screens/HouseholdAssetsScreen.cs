using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.HouseholdAssets;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class HouseholdAssetsScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 5;
    private const int DetailX = 40;
    private readonly HouseholdAssetsMenuContext _context;
    private readonly HouseholdAssetsCommand _command = new();
    private readonly GameSession _gameState;
    private readonly List<HouseholdAssetsMenuStatus> _statuses;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public HouseholdAssetsScreen(int width, int height, GameSession gameState, HouseholdAssetsMenuContext context, List<HouseholdAssetsMenuStatus> statuses, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _statuses = statuses;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Household Assets ===", Color.Cyan);
        Surface.Print(ListX, 3, $"Location: {_context.LocationName ?? "Unknown"} | Week {_context.CurrentWeek}", Color.Gray);
        Surface.Print(DetailX, 2, "=== Details ===", Color.Cyan);

        for (var i = 0; i < _statuses.Count; i++)
        {
            var status = _statuses[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = status.CanExecute
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;
            Surface.Print(ListX, ListY + i, TrimToFit($"{prefix}{status.Title}", DetailX - ListX - 2), color);
        }

        RenderSelectedDetails();
        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to act, Escape to cancel", Color.DarkGray);
        Surface.Print(2, Surface.Height - 2, $"Money: {_gameState.Player.Stats.Money} LE | Pets: {_gameState.Player.HouseholdAssets.Pets.Count} | Plants: {_gameState.Player.HouseholdAssets.Plants.Count}", Color.Gold);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _statuses.Count) % _statuses.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _statuses.Count;
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
        for (var i = 0; i < _statuses.Count; i++)
        {
            var endX = DetailX - 1;
            if (cellPosition.Y == ListY + i && cellPosition.X >= ListX && cellPosition.X < endX)
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
        if (_selectedIndex < 0 || _selectedIndex >= _statuses.Count)
        {
            return;
        }

        var status = _statuses[_selectedIndex];
        if (status.ActionType == HouseholdAssetActionType.ManagePlant && status.PlantId is Guid plantId)
        {
            IsFocused = false;
            GameHost.Instance.Screen = new PlantUpgradeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, PlantUpgradeMenuContext.Create(_gameState, plantId), plantId, this, _parentScreen);
            return;
        }

        if (!status.CanExecute)
        {
            return;
        }

        _command.Execute(_gameState, status.ActionType, status.PetType, status.PlantType);
        ReturnToParentScreen();
    }

    private void RenderSelectedDetails()
    {
        if (_statuses.Count == 0)
        {
            return;
        }

        var selected = _statuses[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;
        Surface.Print(DetailX, y++, selected.Title, Color.White);
        foreach (var line in WrapText(selected.Summary, detailWidth))
        {
            Surface.Print(DetailX, y++, line, selected.CanExecute ? Color.Green : Color.Orange);
        }

        y++;
        foreach (var line in WrapText(selected.Note, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }
    }

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }

    private static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;
        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(current) ? word : $"{current} {word}";
            if (candidate.Length > maxWidth && current.Length > 0)
            {
                yield return current;
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
        {
            yield return current;
        }
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
