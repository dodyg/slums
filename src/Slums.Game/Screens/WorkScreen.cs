using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class WorkScreen : ScreenSurface
{
    private const int JobStartX = 2;
    private const int JobStartY = 4;
    private const int JobBlockHeight = 6;
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

        var y = 2;
        Surface.Print(JobStartX, y++, "=== Work ===", Color.Cyan);
        y = JobStartY;

        for (var i = 0; i < _jobs.Count; i++)
        {
            var job = _jobs[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = job.CanPerform
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;

            Surface.Print(JobStartX, y++, $"{prefix}{job.Job.Name}", color);
            Surface.Print(4, y++, $"  {TrimToFit(job.Job.Description, Surface.Width - 6)}", Color.Gray);
            Surface.Print(4, y++, $"  Pay: ~{job.Job.BasePay} LE | Energy: -{job.Job.EnergyCost} | Stress: +{job.Job.StressCost}",
                Color.Yellow);
            Surface.Print(4, y++, $"  Duration: {job.Job.DurationMinutes / 60}h {job.Job.DurationMinutes % 60}m | Reliability: {job.Reliability} | Shifts: {job.ShiftsCompleted}", Color.Gray);
            Surface.Print(4, y++, $"  {GetStatusLine(job)}", job.CanPerform ? Color.Green : Color.Orange);
            y++;
        }

        y++;
        Surface.Print(2, y++, "Arrow keys to select, Enter to work, Escape to cancel", Color.DarkGray);
        Surface.Print(2, y++, $"Your Energy: {_gameState.Player.Stats.Energy}%",
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
            var blockStartY = JobStartY + i * JobBlockHeight;
            if (cellPosition.Y >= blockStartY &&
                cellPosition.Y < blockStartY + JobBlockHeight - 1 &&
                cellPosition.X >= JobStartX &&
                cellPosition.X < Surface.Width - 2)
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

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
