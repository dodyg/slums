using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Crimes;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class CrimeScreen : ScreenSurface
{
    private readonly GameState _gameState;
    private readonly IReadOnlyList<CrimeAttempt> _crimeAttempts;
    private readonly GameScreen _parentScreen;
    private readonly GameRuntime _runtime;
    private int _selectedIndex;

    public CrimeScreen(int width, int height, GameRuntime runtime, GameState gameState, IReadOnlyList<CrimeAttempt> crimeAttempts, GameScreen parentScreen)
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

        var y = 6;
        for (var i = 0; i < _crimeAttempts.Count; i++)
        {
            var attempt = _crimeAttempts[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            Surface.Print(2, y++, $"{prefix}{attempt.Name}", color);
            Surface.Print(4, y++, $"Reward: ~{attempt.BaseReward} LE | Risk: {GetRiskLabel(attempt.DetectionRisk)} | Energy: -{attempt.EnergyCost}", Color.Yellow);
            Surface.Print(4, y++, $"Pressure: +{attempt.PolicePressureIncrease} | Street Rep Required: {attempt.StreetRepRequired}", Color.Gray);
            y++;
        }

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to attempt, Escape to cancel", Color.DarkGray);
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
            _gameState.CommitCrime(_crimeAttempts[_selectedIndex], _runtime.RandomSource.SharedRandom);
            ReturnToParentScreen();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            ReturnToParentScreen();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
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
}