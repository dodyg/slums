using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class CrimeScreen : ScreenSurface
{
    private const int CrimeStartY = 6;
    private const int CrimeBlockHeight = 5;
    private readonly GameState _gameState;
    private readonly IReadOnlyList<CrimeMenuStatus> _crimeAttempts;
    private readonly GameScreen _parentScreen;
    private readonly GameRuntime _runtime;
    private int _selectedIndex;

    public CrimeScreen(int width, int height, GameRuntime runtime, GameState gameState, IReadOnlyList<CrimeMenuStatus> crimeAttempts, GameScreen parentScreen)
        : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
        _crimeAttempts = crimeAttempts;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(2, 2, "=== Crime ===", Color.Cyan);
        Surface.Print(2, 4, $"Police Pressure: {_gameState.PolicePressure}", GetPressureColor(_gameState.PolicePressure));

        var y = CrimeStartY;
        for (var i = 0; i < _crimeAttempts.Count; i++)
        {
            var attempt = _crimeAttempts[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = attempt.IsAvailable
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;
            Surface.Print(2, y++, $"{prefix}{attempt.Attempt.Name}", color);
            Surface.Print(4, y++, $"Reward: ~{attempt.Attempt.BaseReward} LE | Risk: {GetRiskLabel(attempt.Attempt.DetectionRisk)} | Energy: -{attempt.Attempt.EnergyCost}", Color.Yellow);
            Surface.Print(4, y++, $"Pressure: +{attempt.Attempt.PolicePressureIncrease} | Street Rep Required: {attempt.Attempt.StreetRepRequired}", Color.Gray);
            Surface.Print(4, y++, $"{TrimToFit(attempt.IsAvailable ? attempt.StatusText ?? "Ready to run." : attempt.BlockReason ?? "Blocked.", Surface.Width - 6)}", attempt.IsAvailable ? Color.Green : Color.Orange);
            y++;
        }

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to attempt, Escape to cancel", Color.DarkGray);
        if (_crimeAttempts.Count > 0)
        {
            var selected = _crimeAttempts[_selectedIndex];
            var footer = selected.IsAvailable
                ? selected.StatusText ?? "Route is open."
                : selected.BlockReason ?? "Route is blocked.";
            Surface.Print(2, Surface.Height - 2, TrimToFit(footer, Surface.Width - 4), selected.IsAvailable ? Color.DarkGray : Color.Orange);
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _crimeAttempts.Count) % _crimeAttempts.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _crimeAttempts.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            AttemptSelectedCrime();
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
        for (var i = 0; i < _crimeAttempts.Count; i++)
        {
            var blockStartY = CrimeStartY + (i * CrimeBlockHeight);
            if (cellPosition.Y < blockStartY || cellPosition.Y >= blockStartY + CrimeBlockHeight - 1)
            {
                continue;
            }

            _selectedIndex = i;
            if (_crimeAttempts[i].IsAvailable)
            {
                AttemptSelectedCrime();
            }

            return true;
        }

        return handled;
    }

    private void AttemptSelectedCrime()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _crimeAttempts.Count)
        {
            return;
        }

        var selected = _crimeAttempts[_selectedIndex];
        if (!selected.IsAvailable)
        {
            return;
        }

        _gameState.CommitCrime(selected.Attempt, _runtime.RandomSource.SharedRandom);
        ReturnToParentScreen();
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }

    private static string GetRiskLabel(int risk) => risk switch
    {
        < 25 => "Low",
        < 50 => "Medium",
        _ => "High"
    };

    private static Color GetPressureColor(int pressure) => pressure switch
    {
        >= 80 => Color.Red,
        >= 50 => Color.Orange,
        _ => Color.Green
    };

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
    }
}