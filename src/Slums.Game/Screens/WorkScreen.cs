using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;
using Slums.Game.Rendering;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class WorkScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 5;
    private const int ListRowHeight = 2;
    private const int DetailX = 36;
    private readonly WorkMenuContext _context;
    private readonly GameSession _gameState;
    private readonly List<WorkMenuStatus> _jobs;
    private readonly GameScreen _parentScreen;
    private readonly WorkCommand _workCommand = new();
    private readonly TipContextQuery _tipContextQuery = new();
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private int _selectedIndex;

    public WorkScreen(int width, int height, GameSession gameState, WorkMenuContext context, List<WorkMenuStatus> jobs, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _jobs = jobs;
        _parentScreen = parentScreen;
        _selectedIndex = 0;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Work ===", Color.Cyan);
        Surface.Print(ListX, 3, "Select a shift to inspect or take.", Color.Gray);
        Surface.Print(DetailX, 2, "=== Shift Detail ===", Color.Cyan);

        var tipHints = _tipContextQuery.GetWorkHints(_gameState);
        var tipStartY = 4;
        for (var t = 0; t < Math.Min(tipHints.Count, 2); t++)
        {
            Surface.Print(ListX, tipStartY + t, UiText.TrimToFit($"* {tipHints[t].Content}", DetailX - ListX - 2), Color.Yellow);
        }

        var effectiveListY = ListY + Math.Min(tipHints.Count, 2);

        for (var i = 0; i < _jobs.Count; i++)
        {
            var job = _jobs[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = effectiveListY + (i * ListRowHeight);
            var color = job.CanPerform
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;

            Surface.Print(ListX, rowY, UiText.TrimToFit($"{prefix}{job.Job.Name}", DetailX - ListX - 2), color);
            Surface.Print(ListX + 2, rowY + 1, UiText.TrimToFit(GetStatusLine(job), DetailX - ListX - 4), job.CanPerform ? Color.Green : Color.Orange);
        }

        RenderSelectedJobDetails();

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to work, Escape to cancel", Color.DarkGray);
        Surface.Print(2, Surface.Height - 2, $"Your Energy: {_context.Player.Stats.Energy}%",
            _context.Player.Stats.Energy < 30 ? Color.Red : Color.Green);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _jobs.Count) % _jobs.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _jobs.Count;
            return true;
        }

        var numberIndex = NumberKeyMapper.GetPressedNumberIndex(keyboard, _jobs.Count);
        if (numberIndex is int selectedIndex)
        {
            _selectedIndex = selectedIndex;
            return true;
        }

        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            WorkSelectedJob();
            return true;
        }

        if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
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
        var effectiveListY = ListY + Math.Min(_tipContextQuery.GetWorkHints(_gameState).Count, 2);
        for (var i = 0; i < _jobs.Count; i++)
        {
            var blockStartY = effectiveListY + i * ListRowHeight;
            if (cellPosition.Y >= blockStartY &&
                cellPosition.Y < blockStartY + ListRowHeight &&
                cellPosition.X >= ListX &&
                cellPosition.X < DetailX - 1)
            {
                _selectedIndex = i;
                WorkSelectedJob();
                return true;
            }
        }

        return handled;
    }

    private void WorkSelectedJob()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _jobs.Count)
        {
            return;
        }

        var job = _jobs[_selectedIndex];
        if (!job.CanPerform)
        {
            return;
        }

        _workCommand.Execute(_gameState, job.Job);
        ReturnToParentScreen();
    }

    private static string GetStatusLine(WorkMenuStatus status)
    {
        if (status.LockoutUntilDay is int lockoutUntilDay)
        {
            return $"Locked to day {lockoutUntilDay + 1} | Reliability {status.Reliability}";
        }

        return status.CanPerform
            ? $"Ready | Reliability {status.Reliability}"
            : status.AvailabilityReason ?? $"Blocked | Reliability {status.Reliability}";
    }

    private void RenderSelectedJobDetails()
    {
        if (_jobs.Count == 0)
        {
            return;
        }

        var selected = _jobs[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Job.Name, Color.White);
        foreach (var line in TextWrap.WrapText(selected.Job.Description, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        y++;
        Surface.Print(DetailX, y++, $"Pay ~{selected.Job.BasePay} LE", Color.Yellow);
        Surface.Print(DetailX, y++, $"Energy -{selected.Job.EnergyCost} | Stress +{selected.Job.StressCost}", Color.Yellow);
        Surface.Print(DetailX, y++, $"Duration {selected.Job.DurationMinutes / 60}h {selected.Job.DurationMinutes % 60}m", Color.Gray);
        Surface.Print(DetailX, y++, $"Reliability {selected.Reliability} | Shifts {selected.ShiftsCompleted}", Color.Gray);

        y++;
        Surface.Print(DetailX, y++, "Availability:", Color.Cyan);
        foreach (var signal in selected.AvailabilitySignals)
        {
            foreach (var line in TextWrap.WrapText($"- {signal}", detailWidth))
            {
                Surface.Print(DetailX, y++, line, selected.CanPerform ? Color.Gray : Color.Orange);
            }
        }

        y++;
        Surface.Print(DetailX, y++, "Why this variant:", Color.Cyan);
        foreach (var line in TextWrap.WrapText(selected.VariantReason, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.White);
        }

        y++;
        Surface.Print(DetailX, y++, "Reliability outlook:", Color.Cyan);
        foreach (var line in TextWrap.WrapText(selected.ReliabilitySummary, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.LightGray);
        }

        if (!string.IsNullOrWhiteSpace(selected.NextUnlockHint))
        {
            y++;
            Surface.Print(DetailX, y++, "Next unlock:", Color.Cyan);
            foreach (var line in TextWrap.WrapText(selected.NextUnlockHint, detailWidth))
            {
                Surface.Print(DetailX, y++, line, Color.Gray);
            }
        }

        if (!string.IsNullOrWhiteSpace(selected.RiskWarning))
        {
            y++;
            foreach (var line in TextWrap.WrapText(selected.RiskWarning, detailWidth))
            {
                Surface.Print(DetailX, y++, line, Color.Orange);
            }
        }

        if (selected.ActiveModifiers.Count > 0)
        {
            y++;
            Surface.Print(DetailX, y++, "Active effects:", Color.Cyan);
            foreach (var modifier in selected.ActiveModifiers)
            {
                foreach (var line in TextWrap.WrapText($"- {modifier}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.Gray);
                }
            }
        }

        if (selected.NarrativeSignals.Count > 0 && y < Surface.Height - 4)
        {
            y++;
            Surface.Print(DetailX, y++, "Story triggers:", Color.Cyan);
            foreach (var signal in selected.NarrativeSignals)
            {
                foreach (var line in TextWrap.WrapText($"- {signal}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.LightGray);
                    if (y >= Surface.Height - 3)
                    {
                        return;
                    }
                }
            }
        }
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        ScreenTransition.ReturnTo(_parentScreen);
    }
}
