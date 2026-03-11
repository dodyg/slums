using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class WorkScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 5;
    private const int ListRowHeight = 2;
    private const int DetailX = 36;
    private readonly GameState _gameState;
    private readonly List<WorkMenuStatus> _jobs;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public WorkScreen(int width, int height, GameState gameState, List<WorkMenuStatus> jobs, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _jobs = jobs;
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

        Surface.Print(ListX, 2, "=== Work ===", Color.Cyan);
        Surface.Print(ListX, 3, "Select a shift to inspect or take.", Color.Gray);
        Surface.Print(DetailX, 2, "=== Shift Detail ===", Color.Cyan);

        for (var i = 0; i < _jobs.Count; i++)
        {
            var job = _jobs[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = ListY + (i * ListRowHeight);
            var color = job.CanPerform
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;

            Surface.Print(ListX, rowY, TrimToFit($"{prefix}{job.Job.Name}", DetailX - ListX - 2), color);
            Surface.Print(ListX + 2, rowY + 1, TrimToFit(GetStatusLine(job), DetailX - ListX - 4), job.CanPerform ? Color.Green : Color.Orange);
        }

        RenderSelectedJobDetails();

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to work, Escape to cancel", Color.DarkGray);
        Surface.Print(2, Surface.Height - 2, $"Your Energy: {_gameState.Player.Stats.Energy}%",
            _gameState.Player.Stats.Energy < 30 ? Color.Red : Color.Green);
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

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            WorkSelectedJob();
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
        for (var i = 0; i < _jobs.Count; i++)
        {
            var blockStartY = ListY + i * ListRowHeight;
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
        _gameState.WorkJob(job.Job);
        ReturnToParentScreen();
    }

    private static string GetStatusLine(WorkMenuStatus status)
    {
        if (status.LockoutUntilDay is int lockoutUntilDay)
        {
            return $"Locked out until day {lockoutUntilDay + 1}.";
        }

        return status.CanPerform
            ? "Ready to work."
            : status.AvailabilityReason ?? "Not available right now.";
    }

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
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
        foreach (var line in WrapText(selected.Job.Description, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        y++;
        Surface.Print(DetailX, y++, $"Pay ~{selected.Job.BasePay} LE", Color.Yellow);
        Surface.Print(DetailX, y++, $"Energy -{selected.Job.EnergyCost} | Stress +{selected.Job.StressCost}", Color.Yellow);
        Surface.Print(DetailX, y++, $"Duration {selected.Job.DurationMinutes / 60}h {selected.Job.DurationMinutes % 60}m", Color.Gray);
        Surface.Print(DetailX, y++, $"Reliability {selected.Reliability} | Shifts {selected.ShiftsCompleted}", Color.Gray);

        y++;
        Surface.Print(DetailX, y++, "Why this shift:", Color.Cyan);
        foreach (var line in WrapText(selected.VariantReason, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.White);
        }

        if (!string.IsNullOrWhiteSpace(selected.NextUnlockHint))
        {
            y++;
            Surface.Print(DetailX, y++, "Next unlock:", Color.Cyan);
            foreach (var line in WrapText(selected.NextUnlockHint, detailWidth))
            {
                Surface.Print(DetailX, y++, line, Color.Gray);
            }
        }

        if (!string.IsNullOrWhiteSpace(selected.RiskWarning))
        {
            y++;
            foreach (var line in WrapText(selected.RiskWarning, detailWidth))
            {
                Surface.Print(DetailX, y++, line, Color.Orange);
            }
        }

        if (selected.ActiveModifiers.Count > 0)
        {
            y++;
            Surface.Print(DetailX, y++, "Active modifiers:", Color.Cyan);
            foreach (var modifier in selected.ActiveModifiers)
            {
                foreach (var line in WrapText($"- {modifier}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.Gray);
                }
            }
        }

        if (selected.NarrativeSignals.Count > 0 && y < Surface.Height - 4)
        {
            y++;
            Surface.Print(DetailX, y++, "Narrative signals:", Color.Cyan);
            foreach (var signal in selected.NarrativeSignals)
            {
                foreach (var line in WrapText($"- {signal}", detailWidth))
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
