using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Application.Investments;
using Slums.Application.Narrative;
using Slums.Core.Clock;
using Slums.Core.State;
using Slums.Core.World;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class GameScreen : ScreenSurface
{
    private const int ActionListX = 2;
    private const int ActionHeaderY = 10;
    private const int ActionListStartY = 12;
    private const int MaxEventLogEntries = 8;
    private static readonly TimeSpan RealTimePerGameMinute = TimeSpan.FromSeconds(1);
    private readonly GameRuntime _runtime;
    private readonly GameSession _gameState;
    private readonly WorkMenuStatusQuery _workMenuStatusQuery = new();
    private readonly CrimeMenuStatusQuery _crimeMenuStatusQuery = new();
    private readonly GameStatusPageQuery _statusPageQuery = new();
    private readonly TalkNpcStatusQuery _talkNpcStatusQuery = new();
    private readonly ClinicTravelMenuQuery _clinicTravelMenuQuery = new();
    private readonly EntertainmentMenuQuery _entertainmentMenuQuery = new();
    private readonly InvestmentMenuQuery _investmentMenuQuery = new();
    private readonly GameActionMenuQuery _gameActionMenuQuery = new();
    private readonly GameActionCommand _gameActionCommand = new();
    private readonly AdvanceTimeCommand _advanceTimeCommand = new();
    private readonly AutomaticTimeAdvancer _automaticTimeAdvancer;
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private readonly List<string> _eventLog = new(10);
    private int _selectedAction;
    private int _selectedStatusPage;
    private bool _hasLoggedGameOver;

    public GameScreen(int width, int height, GameRuntime runtime, GameSession gameState) : base(width, height)
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

        _advanceTimeCommand.Execute(_gameState, elapsedMinutes);
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
        RenderStatusPage();
    }

    private void RenderHud()
    {
        var statusContext = GameStatusContext.Create(_gameState);

        Surface.Print(0, 0, "=== SLUMS - Cairo Survival ===", Color.Yellow);
        Surface.Print(0, 2, $"Day {statusContext.Clock.Day} - {statusContext.Clock.TimeOfDay}", Color.White);
        Surface.Print(0, 3, $"Time: {statusContext.Clock.Hour:D2}:{statusContext.Clock.Minute:D2}", Color.Gray);
        Surface.Print(0, 4, $"Police Pressure: {statusContext.PolicePressure}", statusContext.PolicePressure >= 80 ? Color.Red : Color.Orange);

        // Money is displayed as a plain figure; the bar is only meaningful for 0-100 bounded stats
        var rentColor = statusContext.Player.Stats.Money < statusContext.RentCost ? Color.Red : Color.Gray;
        Surface.Print(0, Surface.Height - 20 + GetStatLine("Rent"), $"Rent: {statusContext.RentCost} LE (due end of day)", rentColor);
        Surface.Print(0, Surface.Height - 20 + GetStatLine("Money"), $"Money: {statusContext.Player.Stats.Money} LE", Color.Gold);
        RenderStat("Hunger", statusContext.Player.Stats.Hunger, 100, GetStatColor(statusContext.Player.Stats.Hunger));
        RenderStat("Energy", statusContext.Player.Stats.Energy, 100, GetStatColor(statusContext.Player.Stats.Energy));
        RenderStat("Health", statusContext.Player.Stats.Health, 100, GetStatColor(statusContext.Player.Stats.Health));
        RenderStat("Stress", statusContext.Player.Stats.Stress, 100, GetStressColor(statusContext.Player.Stats.Stress));
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
        "Rent" => 0,
        "Money" => 1,
        "Hunger" => 2,
        "Energy" => 3,
        "Health" => 4,
        "Stress" => 5,
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
        Surface.Print(0, ActionHeaderY, "--- Actions ---", Color.Cyan);

        for (var i = 0; i < actions.Count; i++)
        {
            var prefix = i == _selectedAction ? "> " : "  ";
            var color = i == _selectedAction ? Color.Cyan : Color.White;
            Surface.Print(ActionListX, ActionListStartY + i, prefix + actions[i].Label, color);
        }
    }

    private void RenderEventLog()
    {
        var y = 18;
        Surface.Print(45, y++, "--- Event Log ---", Color.Cyan);

        for (var i = _eventLog.Count - 1; i >= 0; i--)
        {
            var text = _eventLog[i];
            if (text.Length > 32)
            {
                text = text[..32] + "...";
            }
            Surface.Print(45, y + (_eventLog.Count - 1 - i), text, Color.Gray);
        }
    }

    private void RenderStatusPage()
    {
        var x = 45;
        var y = 0;
        var pages = _statusPageQuery.GetPages(GameStatusContext.Create(_gameState));
        if (pages.Count == 0)
        {
            return;
        }

        if (_selectedStatusPage >= pages.Count)
        {
            _selectedStatusPage = 0;
        }

        var page = pages[_selectedStatusPage];
        Surface.Print(x, y++, $"--- {page.Title} [{_selectedStatusPage + 1}/{pages.Count}] ---", Color.Cyan);

        foreach (var line in page.Lines.Take(10))
        {
            foreach (var wrappedLine in WrapText(line, Surface.Width - x - 2).Take(2))
            {
                Surface.Print(x, y++, wrappedLine, Color.White);
            }
            if (y >= 14)
            {
                break;
            }
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (_gameState.IsGameOver)
        {
            if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
            {
                IsFocused = false;
                GameHost.Instance.Screen = new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime);
            }

            _actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape));
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

        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            ExecuteAction();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Tab))
        {
            CycleStatusPage();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.T))
        {
            ShowTravelMenu();
            return true;
        }

        if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
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

    internal void SuppressActionKeysUntilRelease()
    {
        _actionKeyGate.SuppressActionKeysUntilRelease();
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
            var endX = ActionListX + actions[i].Label.Length + 2;
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
        switch (action.Id)
        {
            case GameActionId.Rest:
            case GameActionId.EatAtHome:
            case GameActionId.EatStreetFood:
            case GameActionId.CheckOnMother:
            case GameActionId.GiveMotherMedicine:
            case GameActionId.EndDay:
                _gameActionCommand.Execute(_gameState, action.Id, _runtime.RandomSource.SharedRandom);
                break;
            case GameActionId.Work:
                ShowWorkMenu();
                break;
            case GameActionId.Crime:
                ShowCrimeMenu();
                break;
            case GameActionId.Talk:
                ShowTalkMenu();
                break;
            case GameActionId.Entertainment:
                ShowEntertainmentMenu();
                break;
            case GameActionId.Invest:
                ShowInvestmentMenu();
                break;
            case GameActionId.Shop:
                ShowShopMenu();
                break;
            case GameActionId.TakeMotherToClinic:
                ShowClinicTravelMenu();
                break;
            case GameActionId.Travel:
                ShowTravelMenu();
                break;
            case GameActionId.SaveGame:
                ShowSaveMenu();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }

        TryShowPendingNarrativeScene();
        AppendGameOverMessagesIfNeeded();
    }

    private void ShowWorkMenu()
    {
        var workContext = WorkMenuContext.Create(_gameState);
        var jobs = _workMenuStatusQuery.GetStatuses(workContext).ToList();
        if (jobs.Count == 0)
        {
            AddEventLogEntry("No work available here.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new WorkScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, workContext, jobs, this);
    }

    private void ShowCrimeMenu()
    {
        var crimeContext = CrimeMenuContext.Create(_gameState);
        var crimes = _crimeMenuStatusQuery.GetStatuses(crimeContext).ToList();
        if (crimes.Count == 0)
        {
            AddEventLogEntry("No crime opportunities here.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new CrimeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, crimeContext, crimes, this);
    }

    private void ShowTalkMenu()
    {
        var talkContext = TalkNpcContext.Create(_gameState);
        var npcStatuses = _talkNpcStatusQuery.GetStatuses(talkContext);
        if (npcStatuses.Count == 0)
        {
            AddEventLogEntry("No one is available to talk.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new TalkScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, talkContext, npcStatuses, this);
    }

    private void ShowShopMenu()
    {
        var shopContext = ShopMenuContext.Create(_gameState);
        IsFocused = false;
        GameHost.Instance.Screen = new ShopScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, shopContext, this);
    }

    private void ShowTravelMenu()
    {
        var locations = WorldState.AllLocations.ToList();
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

    private void ShowClinicTravelMenu()
    {
        var clinicContext = ClinicTravelMenuContext.Create(_gameState);
        var clinics = _clinicTravelMenuQuery.GetStatuses(clinicContext).ToList();
        if (clinics.Count == 0)
        {
            AddEventLogEntry("No clinics available.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new ClinicTravelScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, clinicContext, clinics, this);
    }

    private void ShowEntertainmentMenu()
    {
        var entertainmentContext = EntertainmentMenuContext.Create(_gameState);
        var activities = _entertainmentMenuQuery.GetStatuses(entertainmentContext).ToList();
        if (activities.Count == 0)
        {
            AddEventLogEntry("No entertainment available here.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new EntertainmentScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, entertainmentContext, activities, this);
    }

    private void ShowInvestmentMenu()
    {
        var investmentContext = InvestmentMenuContext.Create(_gameState);
        var investments = _investmentMenuQuery.GetStatuses(investmentContext).ToList();
        if (investments.Count == 0)
        {
            AddEventLogEntry("No investment opportunities are being offered here.");
            return;
        }

        IsFocused = false;
        GameHost.Instance.Screen = new InvestmentMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, investmentContext, investments, this);
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

    private List<GameAction> GetActions()
    {
        var actions = _gameActionMenuQuery.GetActions(GameActionMenuContext.Create(_gameState)).ToList();
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

        _runtime.NarrativeService.StartScene(knotName, NarrativeSceneState.Create(_gameState));
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

        _runtime.NarrativeService.StartScene(knotName, NarrativeSceneState.Create(_gameState));
        IsFocused = false;
        GameHost.Instance.Screen = new NarrativeScreen(
            GameRuntime.ScreenWidth,
            GameRuntime.ScreenHeight,
            _runtime.NarrativeService,
            _gameState,
            new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime));

        return true;
    }

    private void CycleStatusPage()
    {
        var pages = _statusPageQuery.GetPages(GameStatusContext.Create(_gameState));
        if (pages.Count == 0)
        {
            return;
        }

        _selectedStatusPage = (_selectedStatusPage + 1) % pages.Count;
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
}
