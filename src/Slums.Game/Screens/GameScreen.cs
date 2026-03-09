using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal sealed class GameScreen : ScreenSurface
{
    private readonly GameState _gameState;
    private readonly List<string> _eventLog = new(10);
    private int _selectedAction;

    private static readonly string[] Actions = ["Rest", "Travel", "End Day"];

    public GameScreen(int width, int height) : base(width, height)
    {
        _gameState = new GameState();
        _gameState.GameEvent += OnGameEvent;
        _selectedAction = 0;
        IsFocused = true;
    }

    private void OnGameEvent(object? sender, GameEventArgs e)
    {
        _eventLog.Add(e.Message);
        if (_eventLog.Count > 8)
        {
            _eventLog.RemoveAt(0);
        }
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        RenderHud();
        RenderActions();
        RenderEventLog();
        RenderLocationInfo();
    }

    private void RenderHud()
    {
        var y = 0;
        Surface.Print(0, y++, "=== SLUMS - Cairo Survival ===", Color.Yellow);

        y++;
        Surface.Print(0, y++, $"Day {_gameState.Clock.Day} - {_gameState.Clock.TimeOfDay}", Color.White);
        Surface.Print(0, y++, $"Time: {_gameState.Clock.Hour:D2}:{_gameState.Clock.Minute:D2}", Color.Gray);

        y++;
        RenderStat("Money", _gameState.Player.Stats.Money, 100, Color.Gold);
        RenderStat("Hunger", _gameState.Player.Stats.Hunger, 100, GetStatColor(_gameState.Player.Stats.Hunger));
        RenderStat("Energy", _gameState.Player.Stats.Energy, 100, GetStatColor(_gameState.Player.Stats.Energy));
        RenderStat("Health", _gameState.Player.Stats.Health, 100, GetStatColor(_gameState.Player.Stats.Health));
        RenderStat("Stress", _gameState.Player.Stats.Stress, 100, GetStressColor(_gameState.Player.Stats.Stress));

        y++;
        Surface.Print(0, y++, $"Food Stock: {_gameState.Player.Household.FoodStockpile}", Color.White);
        Surface.Print(0, y++, $"Mother's Health: {_gameState.Player.Household.MotherHealth}%", 
            _gameState.Player.Household.MotherNeedsCare ? Color.Red : Color.Green);
    }

    private void RenderStat(string name, int value, int max, Color color)
    {
        var barWidth = 10;
        var filled = (int)((double)value / max * barWidth);
        var bar = new string('#', filled) + new string('-', barWidth - filled);
        Surface.Print(0, Surface.Height - 20 + GetStatLine(name), $"{name}: [{bar}] {value}", color);
    }

    private static int GetStatLine(string name) => name switch
    {
        "Money" => 0,
        "Hunger" => 1,
        "Energy" => 2,
        "Health" => 3,
        "Stress" => 4,
        _ => 0
    };

    private static Color GetStatColor(int value) => value switch
    {
        < 20 => Color.Red,
        < 50 => Color.Orange,
        _ => Color.Green
    };

    private static Color GetStressColor(int value) => value switch
    {
        > 80 => Color.Red,
        > 50 => Color.Orange,
        _ => Color.Green
    };

    private void RenderActions()
    {
        var y = 12;
        Surface.Print(0, y++, "--- Actions ---", Color.Cyan);
        Surface.Print(0, y++, "(Arrow keys to select, Enter to confirm)", Color.DarkGray);

        for (var i = 0; i < Actions.Length; i++)
        {
            var prefix = i == _selectedAction ? "> " : "  ";
            var color = i == _selectedAction ? Color.Cyan : Color.White;
            Surface.Print(2, y + i, prefix + Actions[i], color);
        }
    }

    private void RenderEventLog()
    {
        var y = 18;
        Surface.Print(45, y++, "--- Event Log ---", Color.Cyan);

        for (var i = 0; i < _eventLog.Count; i++)
        {
            var text = _eventLog[i];
            if (text.Length > 32)
            {
                text = text[..32] + "...";
            }
            Surface.Print(45, y + i, text, Color.Gray);
        }
    }

    private void RenderLocationInfo()
    {
        var y = 0;
        var x = 45;
        Surface.Print(x, y++, "--- Location ---", Color.Cyan);

        var location = _gameState.World.GetCurrentLocation();
        if (location is not null)
        {
            Surface.Print(x, y++, location.Name, Color.White);
            var desc = location.Description;
            if (desc.Length > 30)
            {
                Surface.Print(x, y++, desc[..30], Color.Gray);
                Surface.Print(x, y++, desc[30..], Color.Gray);
            }
            else
            {
                Surface.Print(x, y++, desc, Color.Gray);
            }
        }

        y++;
        Surface.Print(x, y++, $"District: {DistrictInfo.GetName(_gameState.World.CurrentDistrict)}", Color.Yellow);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (_gameState.IsGameOver)
        {
            if (keyboard.IsKeyPressed(Keys.Enter))
            {
                GameHost.Instance.Screen = new MainMenuScreen(80, 25);
            }
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedAction = (_selectedAction - 1 + Actions.Length) % Actions.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedAction = (_selectedAction + 1) % Actions.Length;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            ExecuteAction();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.T))
        {
            ShowTravelMenu();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            GameHost.Instance.Screen = new MainMenuScreen(80, 25);
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void ExecuteAction()
    {
        switch (_selectedAction)
        {
            case 0:
                _gameState.RestAtHome();
                break;
            case 1:
                ShowTravelMenu();
                break;
            case 2:
                _gameState.EndDay();
                break;
        }

        if (_gameState.IsGameOver)
        {
            _eventLog.Add("GAME OVER");
            _eventLog.Add(_gameState.GameOverReason ?? "Unknown cause");
        }
    }

    private void ShowTravelMenu()
    {
        var locations = _gameState.World.GetTravelableLocations().ToList();
        if (locations.Count == 0)
        {
            _eventLog.Add("No travel destinations available");
            return;
        }

        GameHost.Instance.Screen = new TravelScreen(80, 25, _gameState, locations, this);
    }
}
