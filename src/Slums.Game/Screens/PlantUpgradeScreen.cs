using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.HouseholdAssets;
using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class PlantUpgradeScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 6;
    private const int DetailX = 36;
    private readonly PlantUpgradeCommand _command = new();
    private readonly PlantUpgradeMenuContext _context;
    private readonly GameSession _gameState;
    private readonly GameScreen _rootParentScreen;
    private readonly HouseholdAssetsScreen _parentScreen;
    private readonly Guid _plantId;
    private readonly List<PlantUpgradeMenuStatus> _statuses;
    private int _selectedIndex;

    public PlantUpgradeScreen(int width, int height, GameSession gameState, PlantUpgradeMenuContext context, Guid plantId, HouseholdAssetsScreen parentScreen, GameScreen rootParentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _plantId = plantId;
        _parentScreen = parentScreen;
        _rootParentScreen = rootParentScreen;
        _statuses = new PlantUpgradeMenuQuery().GetStatuses(context).ToList();
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, $"=== {_context.Definition.Name} Upgrades ===", Color.Cyan);
        Surface.Print(ListX, 3, $"Money: {_context.Money} LE | Week {_context.CurrentWeek}", Color.Gray);
        Surface.Print(ListX, 4, $"Care this week: {(_context.Plant.IsBaseCarePaidForWeek(_context.CurrentWeek) ? "covered" : "due")}", Color.Gray);
        Surface.Print(DetailX, 2, "=== Upgrade Details ===", Color.Cyan);

        for (var i = 0; i < _statuses.Count; i++)
        {
            var status = _statuses[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = status.CanExecute
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;
            Surface.Print(ListX, ListY + i, $"{prefix}{status.Name} ({status.Cost} LE)", color);
        }

        RenderSelectedDetails();
        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to buy, Escape to cancel", Color.DarkGray);
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

    private void ExecuteSelection()
    {
        var status = _statuses[_selectedIndex];
        if (!status.CanExecute)
        {
            return;
        }

        _command.Execute(_gameState, _plantId, status.UpgradeType);
        _rootParentScreen.IsFocused = true;
        IsFocused = false;
        _parentScreen.IsFocused = false;
        GameHost.Instance.Screen = _rootParentScreen;
    }

    private void RenderSelectedDetails()
    {
        var selected = _statuses[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Name, Color.White);
        foreach (var line in WrapText(selected.Note, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        y++;
        Surface.Print(DetailX, y++, $"Plant: {_context.Definition.Name}", Color.White);
        Surface.Print(DetailX, y++, $"Category: {_context.Definition.Category}", Color.White);
        Surface.Print(DetailX, y++, $"Cost: {selected.Cost} LE", selected.CanExecute ? Color.Green : Color.Orange);
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
