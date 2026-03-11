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
    private readonly GameRuntime _runtime;
    private readonly GameState _gameState;
    private readonly AutomaticTimeAdvancer _automaticTimeAdvancer;
    private readonly List<string> _eventLog = new(10);
    private int _selectedAction;
    private bool _hasLoggedGameOver;

    public GameScreen(int width, int height, GameRuntime runtime, GameState gameState) : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
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
            if (TryShowEndingScene())
            {
                return;
            }

            AppendGameOverMessagesIfNeeded();
            return;
        }

        var elapsedMinutes = _automaticTimeAdvancer.CollectElapsedMinutes(delta);
        if (elapsedMinutes <= 0)
        {
            return;
        }

        _gameState.AdvanceTime(elapsedMinutes);
        if (TryShowPendingNarrativeScene())
        {
            return;
        }

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
        Surface.Print(0, y++, $"Police Pressure: {_gameState.PolicePressure}", _gameState.PolicePressure >= 80 ? Color.Red : Color.Orange);

        y++;
        // Money is displayed as a plain figure; the bar is only meaningful for 0-100 bounded stats
        Surface.Print(0, Surface.Height - 20 + GetStatLine("Money"), $"Money: {_gameState.Player.Stats.Money} LE", Color.Gold);
        RenderStat("Hunger", _gameState.Player.Stats.Hunger, 100, GetStatColor(_gameState.Player.Stats.Hunger));
        RenderStat("Energy", _gameState.Player.Stats.Energy, 100, GetStatColor(_gameState.Player.Stats.Energy));
        RenderStat("Health", _gameState.Player.Stats.Health, 100, GetStatColor(_gameState.Player.Stats.Health));
        RenderStat("Stress", _gameState.Player.Stats.Stress, 100, GetStressColor(_gameState.Player.Stats.Stress));

        // Stats occupy rows (Surface.Height - 20) through (Surface.Height - 20 + 4); place household info after them
        y = Surface.Height - 20 + 6;
        Surface.Print(0, y++, $"Food Stock: {_gameState.Player.Household.FoodStockpile}", Color.White);
        Surface.Print(0, y++, $"Mother's Health: {_gameState.Player.Household.MotherHealth}%", 
            _gameState.Player.Household.MotherNeedsCare ? Color.Red : Color.Green);
        Surface.Print(0, y++, $"Skills: St {_gameState.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.StreetSmarts)} | Ph {_gameState.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.Physical)} | Pe {_gameState.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.Persuasion)} | Me {_gameState.Player.Skills.GetLevel(Slums.Core.Skills.SkillId.Medical)}", Color.Gray);
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
        var actions = GetActions();
        Surface.Print(0, 12, "--- Actions ---", Color.Cyan);
        Surface.Print(0, 13, "(Time flows automatically. Arrow keys to select, Enter to confirm)", Color.DarkGray);

        for (var i = 0; i < actions.Count; i++)
        {
            var prefix = i == _selectedAction ? "> " : "  ";
            var color = i == _selectedAction ? Color.Cyan : Color.White;
            Surface.Print(ActionListX, ActionListStartY + i, prefix + actions[i], color);
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
                IsFocused = false;
                GameHost.Instance.Screen = new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime);
            }
            return true;
        }

        var actions = GetActions();

        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedAction = (_selectedAction - 1 + actions.Count) % actions.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedAction = (_selectedAction + 1) % actions.Count;
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
            IsFocused = false;
            GameHost.Instance.Screen = new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime);
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.P))
        {
            ShowSaveMenu();
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
        var actions = GetActions();
        for (var i = 0; i < actions.Count; i++)
        {
            var endX = ActionListX + actions[i].Length + 2;
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
        var action = GetActions()[_selectedAction];
        switch (action)
        {
            case "Rest":
                _gameState.RestAtHome();
                break;
            case "Work":
                ShowWorkMenu();
                break;
            case "Crime":
                ShowCrimeMenu();
                break;
            case "Talk":
                ShowTalkMenu();
                break;
            case "Shop":
                ShowShopMenu();
                break;
            case "Travel":
                ShowTravelMenu();
                break;
            case "Save Game":
                ShowSaveMenu();
                break;
            case "End Day":
                _gameState.EndDay(_runtime.RandomSource.SharedRandom);
                break;
        }

        TryShowPendingNarrativeScene();
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

        IsFocused = false;
        GameHost.Instance.Screen = new WorkScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, jobs, this);
    }

    private void ShowCrimeMenu()
    {
        var crimes = _gameState.GetAvailableCrimes();
        if (crimes.Count == 0)
        {
            AddEventLogEntry("No crime opportunities here.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new CrimeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, crimes, this);
    }

    private void ShowTalkMenu()
    {
        var npcs = _gameState.GetReachableNpcs();
        if (npcs.Count == 0)
        {
            AddEventLogEntry("No one is available to talk.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new TalkScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, npcs, this);
    }

    private void ShowShopMenu()
    {
        IsFocused = false;
        GameHost.Instance.Screen = new ShopScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, this);
    }

    private void ShowTravelMenu()
    {
        var locations = _gameState.World.GetTravelableLocations().ToList();
        if (locations.Count == 0)
        {
            AddEventLogEntry("No travel destinations available");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new TravelScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, locations, this);
    }

    private void ShowSaveMenu()
    {
        IsFocused = false;
        GameHost.Instance.Screen = new SaveGameScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, this);
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

    private List<string> GetActions()
    {
        var actions = new List<string> { "Rest", "Work" };
        var location = _gameState.World.GetCurrentLocation();
        if (location?.HasCrimeOpportunities == true)
        {
            actions.Add("Crime");
        }

        if (_gameState.GetReachableNpcs().Count > 0)
        {
            actions.Add("Talk");
        }

        actions.Add("Shop");
        actions.Add("Travel");
        actions.Add("Save Game");
        actions.Add("End Day");

        if (_selectedAction >= actions.Count)
        {
            _selectedAction = actions.Count - 1;
        }

        return actions;
    }

    private bool TryShowPendingNarrativeScene()
    {
        if (!_gameState.TryDequeueNarrativeScene(out var knotName))
        {
            return false;
        }

        _runtime.NarrativeService.StartScene(knotName, _gameState);
        IsFocused = false;
        GameHost.Instance.Screen = new NarrativeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime.NarrativeService, _gameState, this);
        return true;
    }

    private bool TryShowEndingScene()
    {
        if (!_gameState.TryTakePendingEndingKnot(out var knotName))
        {
            return false;
        }

        _runtime.NarrativeService.StartScene(knotName, _gameState);
        IsFocused = false;
        GameHost.Instance.Screen = new NarrativeScreen(
            GameRuntime.ScreenWidth,
            GameRuntime.ScreenHeight,
            _runtime.NarrativeService,
            _gameState,
            new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime));

        return true;
    }
}
