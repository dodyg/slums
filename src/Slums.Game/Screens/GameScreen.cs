using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Application.Narrative;
using Slums.Core.Clock;
using Slums.Core.State;
using Slums.Core.World;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class GameScreen : ScreenSurface
{
    private static readonly TimeSpan RealTimePerGameMinute = TimeSpan.FromSeconds(1);
    private readonly GameRuntime _runtime;
    private readonly GameSession _gameState;
    private readonly GameStatusPageQuery _statusPageQuery = new();
    private readonly GameActionMenuQuery _gameActionMenuQuery = new();
    private readonly GameActionCommand _gameActionCommand = new();
    private readonly AdvanceTimeCommand _advanceTimeCommand = new();
    private readonly AutomaticTimeAdvancer _automaticTimeAdvancer;
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private readonly List<string> _eventLog = new(GameScreenLayout.MaxEventLogEntries);
    private readonly GameScreenNavigator _navigator;
    private int _selectedAction;
    private int _selectedStatusPage;
    private bool _hasLoggedGameOver;

    public GameScreen(int width, int height, GameRuntime runtime, GameSession gameState) : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
        _gameState.GameEvent += OnGameEvent;
        _automaticTimeAdvancer = new AutomaticTimeAdvancer(RealTimePerGameMinute);
        _navigator = new GameScreenNavigator(runtime, gameState, this);
        Children.Add(new OverviewWindow(this));
        Children.Add(new StatusPageWindow(this));
        Children.Add(new EventLogWindow(this));
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

        var statusContext = GameStatusContext.Create(_gameState);
        GameScreenHudRenderer.RenderHud(this, statusContext);
        GameScreenHudRenderer.RenderActions(this, GetActions(), _selectedAction);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (_gameState.IsGameOver)
        {
            if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
            {
                ScreenTransition.SwitchTo(new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime));
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
            _navigator.ShowTravelMenu();
            return true;
        }

        if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
        {
            ScreenTransition.SwitchTo(new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime));
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.P))
        {
            _navigator.ShowSaveMenu();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.L))
        {
            _navigator.ShowEventLogViewer();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    internal void SuppressActionKeysUntilRelease()
    {
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    internal void AddEventLogEntry(string message)
    {
        _eventLog.Add(message);
        while (_eventLog.Count > GameScreenLayout.MaxEventLogEntries)
        {
            _eventLog.RemoveAt(0);
        }
    }

    internal IReadOnlyList<string> GetEventLogEntries() => _eventLog;

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
            default:
                _navigator.ShowMenu(action.Id);
                break;
        }

        TryShowPendingNarrativeScene();
        AppendGameOverMessagesIfNeeded();
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
        ScreenTransition.FadeTo(new NarrativeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime.NarrativeService, _gameState, this));
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
        ScreenTransition.FadeTo(new NarrativeScreen(
            GameRuntime.ScreenWidth,
            GameRuntime.ScreenHeight,
            _runtime.NarrativeService,
            _gameState,
            new MainMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime)));

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

    private sealed class EventLogWindow : Window
    {
        private const int VisibleEntries = 6;
        private readonly GameScreen _owner;

        public EventLogWindow(GameScreen owner)
            : base(GameScreenLayout.RightPanelWidth, VisibleEntries + 2)
        {
            _owner = owner;
            Position = new Point(GameScreenLayout.EventLogX, GameScreenLayout.EventLogY);
            Title = " Event Log (L=full) ";
            CanDrag = true;
            IsExclusiveMouse = true;
            IsFocused = true;
            UseMouse = true;
            CloseOnEscKey = false;
            IsVisible = true;
        }

        public override void Render(TimeSpan delta)
        {
            base.Render(delta);
            Surface.Clear();
            DrawBorder();
            RenderEventLog();
        }

        public void RenderEventLog()
        {
            var entries = _owner._eventLog;
            var start = Math.Max(0, entries.Count - VisibleEntries);
            var row = 1;

            for (var i = entries.Count - 1; i >= start; i--)
            {
                var text = GameScreenHudRenderer.TrimToWidth(entries[i], Surface.Width - 4);
                Surface.Print(2, row, text, Color.Gray);
                row++;
            }
        }
    }

    private sealed class OverviewWindow : Window
    {
        private readonly GameScreen _owner;

        public OverviewWindow(GameScreen owner)
            : base(GameScreenLayout.RightPanelWidth, 12)
        {
            _owner = owner;
            Position = new Point(GameScreenLayout.OverviewX, GameScreenLayout.OverviewY);
            Title = " Overview ";
            IsVisible = true;
        }

        public override void Render(TimeSpan delta)
        {
            base.Render(delta);
            Surface.Clear();
            DrawBorder();
            RenderOverview();
        }

        private void RenderOverview()
        {
            var statusContext = GameStatusContext.Create(_owner._gameState);
            var household = statusContext.Player.Household;
            var location = statusContext.World.GetCurrentLocation()?.Name ?? "Unknown";
            var districtName = DistrictInfo.GetName(statusContext.World.CurrentDistrict);
            var width = Surface.Width - 4;
            var y = 1;

            var dayOfWeek = statusContext.Clock.DayOfWeek;
            var daySchedule = DayScheduleRegistry.GetModifiers(dayOfWeek);
            Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth($"Day {statusContext.Clock.Day} ({daySchedule.DayName}) - {statusContext.Clock.TimeOfDay} | {statusContext.Clock.Hour:D2}:{statusContext.Clock.Minute:D2} | {statusContext.SeasonName} | {statusContext.WeatherName}", width), Color.White);
            Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth($"Location: {location}", width), Color.White);
            Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth($"District: {districtName}", width), Color.White);
            var policeColor = statusContext.PolicePressure >= 80 ? Color.Red
                : statusContext.PolicePressure >= 50 ? Color.Orange
                : Color.Green;
            var moneyPrefix = $"Money: {statusContext.Player.Stats.Money} LE | Police: ";
            Surface.Print(2, y, GameScreenHudRenderer.TrimToWidth($"{moneyPrefix}{statusContext.PolicePressure}", width), Color.Gold);
            Surface.Print(2 + moneyPrefix.Length, y, $"{statusContext.PolicePressure}", policeColor);
            y++;

            var rentColor = statusContext.UnpaidRentDays >= 5
                ? Color.Red
                : statusContext.UnpaidRentDays > 0 || statusContext.Player.Stats.Money < statusContext.RentCost
                    ? Color.Orange
                    : Color.Gray;
            Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth(GameScreenHudRenderer.BuildRentOverviewText(statusContext), width), rentColor);
            Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth($"Food: {household.FoodStockpile} | Med: {household.MedicineStock} | Buy food {statusContext.FoodCost} | Street {statusContext.StreetFoodCost} | Meds {statusContext.MedicineCost}", width), Color.White);

            var clinicText = statusContext.HasClinicServices
                ? $"Clinic: {(statusContext.ClinicOpenToday ? "open" : "closed")} | visit {statusContext.ClinicVisitCost} LE"
                : "Clinic: none here";
            Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth(clinicText, width), statusContext.HasClinicServices ? Color.LightGreen : Color.Gray);

            var undeliveredTips = _owner._gameState.Tips.GetUndeliveredTips(statusContext.Clock.Day);
            if (undeliveredTips.Count > 0)
            {
                var tipText = undeliveredTips.Any(t => t.IsEmergency)
                    ? $"!! URGENT TIP ({undeliveredTips.Count} new)"
                    : $"New tip ({undeliveredTips.Count})";
                Surface.Print(2, y++, GameScreenHudRenderer.TrimToWidth(tipText, width),
                    undeliveredTips.Any(t => t.IsEmergency) ? Color.Red : Color.Yellow);
            }

            var districtBulletin = statusContext.CurrentDistrictCondition is null
                ? "Today: no major district pressure."
                : $"Today: {statusContext.CurrentDistrictCondition.Title} - {statusContext.CurrentDistrictCondition.GameplaySummary}";
            Surface.Print(2, y, GameScreenHudRenderer.TrimToWidth(districtBulletin, width), Color.LightGray);
        }
    }

    private sealed class StatusPageWindow : Window
    {
        private readonly GameScreen _owner;

        public StatusPageWindow(GameScreen owner)
            : base(GameScreenLayout.RightPanelWidth, 8)
        {
            _owner = owner;
            Position = new Point(GameScreenLayout.StatusPageX, GameScreenLayout.StatusPageY);
            Title = " Status ";
            CanDrag = true;
            IsExclusiveMouse = true;
            IsFocused = true;
            UseMouse = true;
            IsVisible = true;
        }

        public override void Render(TimeSpan delta)
        {
            base.Render(delta);
            Surface.Clear();
            DrawBorder();
            RenderStatusPage();
        }

        private void RenderStatusPage()
        {
            var statusContext = GameStatusContext.Create(_owner._gameState);
            var pages = _owner._statusPageQuery.GetPages(statusContext);
            if (pages.Count == 0)
            {
                return;
            }

            if (_owner._selectedStatusPage >= pages.Count)
            {
                _owner._selectedStatusPage = 0;
            }

            var page = pages[_owner._selectedStatusPage];
            const int x = 2;
            var y = 1;
            var width = Surface.Width - 4;

            Surface.Print(x, y++, $"{page.Title} [{_owner._selectedStatusPage + 1}/{pages.Count}]", Color.Cyan);

            foreach (var line in page.Lines)
            {
                foreach (var wrappedLine in GameScreenHudRenderer.WrapText(line, width).Take(3))
                {
                    if (y >= Surface.Height - 1)
                    {
                        return;
                    }

                    Surface.Print(x, y++, wrappedLine, Color.White);
                }
            }
        }
    }
}
