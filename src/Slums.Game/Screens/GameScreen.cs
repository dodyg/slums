using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Clock;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Game.Screens;

internal sealed class GameScreen : ScreenSurface
{
    private const int ActionListX = 2;
    private const int ActionListStartY = 14;
    private const int MaxEventLogEntries = 8;
    private static readonly TimeSpan RealTimePerGameMinute = TimeSpan.FromSeconds(1);
    private readonly GameState _gameState;
    private readonly AutomaticTimeAdvancer _automaticTimeAdvancer;
    private readonly List<string> _eventLog = new(10);
    private int _selectedAction;
    private bool _hasLoggedGameOver;

    private static readonly string[] Actions = ["Rest", "Work", "Shop", "Travel", "End Day"];

    public GameScreen(int width, int height) : base(width, height)
    {
        _gameState = new GameState();
        _gameState.GameEvent += OnGameEvent;
        _automaticTimeAdvancer = new AutomaticTimeAdvancer(RealTimePerGameMinute);
        _selectedAction = 0;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    private void OnGameEvent(object? sender, GameEventArgs e)
    {
        AddEventLogEntry(e.Message);
    }

    public override void Update(TimeSpan delta)
    {
        base.Update(delta);

        if (_gameState.IsGameOver)
        {
            AppendGameOverMessagesIfNeeded();
            return;
        }

        var elapsedMinutes = _automaticTimeAdvancer.CollectElapsedMinutes(delta);
        if (elapsedMinutes <= 0)
        {
            return;
        }

        _gameState.AdvanceTime(elapsedMinutes);
        AppendGameOverMessagesIfNeeded();
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
        Surface.Print(0, 12, "--- Actions ---", Color.Cyan);
        Surface.Print(0, 13, "(Time flows automatically. Arrow keys to select, Enter to confirm)", Color.DarkGray);

        for (var i = 0; i < Actions.Length; i++)
        {
            var prefix = i == _selectedAction ? "> " : "  ";
            var color = i == _selectedAction ? Color.Cyan : Color.White;
            Surface.Print(ActionListX, ActionListStartY + i, prefix + Actions[i], color);
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

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        var handled = base.ProcessMouse(state);
        if (_gameState.IsGameOver || !state.IsOnScreenObject || !state.Mouse.LeftClicked)
        {
            return handled;
        }

        var cellPosition = state.SurfaceCellPosition;
        for (var i = 0; i < Actions.Length; i++)
        {
            var endX = ActionListX + Actions[i].Length + 2;
            if (cellPosition.Y == ActionListStartY + i && cellPosition.X >= ActionListX && cellPosition.X < endX)
            {
                _selectedAction = i;
                ExecuteAction();
                return true;
            }
        }

        return handled;
    }

    private void ExecuteAction()
    {
        switch (_selectedAction)
        {
            case 0:
                _gameState.RestAtHome();
                break;
            case 1:
                ShowWorkMenu();
                break;
            case 2:
                ShowShopMenu();
                break;
            case 3:
                ShowTravelMenu();
                break;
            case 4:
                _gameState.EndDay();
                break;
        }

        AppendGameOverMessagesIfNeeded();
    }

    private void ShowWorkMenu()
    {
        var location = _gameState.World.GetCurrentLocation();
        if (location is null)
        {
            AddEventLogEntry("You are nowhere.");
            return;
        }

        var jobs = _gameState.Jobs.GetAvailableJobs(location).ToList();
        if (jobs.Count == 0)
        {
            AddEventLogEntry("No work available here.");
            return;
        }

        GameHost.Instance.Screen = new WorkScreen(80, 25, _gameState, jobs, this);
    }

    private void ShowShopMenu()
    {
        GameHost.Instance.Screen = new ShopScreen(80, 25, _gameState, this);
    }

    private void ShowTravelMenu()
    {
        var locations = _gameState.World.GetTravelableLocations().ToList();
        if (locations.Count == 0)
        {
            AddEventLogEntry("No travel destinations available");
            return;
        }

        GameHost.Instance.Screen = new TravelScreen(80, 25, _gameState, locations, this);
    }

    private void AddEventLogEntry(string message)
    {
        _eventLog.Add(message);
        while (_eventLog.Count > MaxEventLogEntries)
        {
            _eventLog.RemoveAt(0);
        }
    }

    private void AppendGameOverMessagesIfNeeded()
    {
        if (!_gameState.IsGameOver || _hasLoggedGameOver)
        {
            return;
        }

        _hasLoggedGameOver = true;
        AddEventLogEntry("GAME OVER");
        AddEventLogEntry(_gameState.GameOverReason ?? "Unknown cause");
    }
}
