using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class CrimeScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 6;
    private const int ListRowHeight = 2;
    private const int DetailX = 36;
    private readonly CrimeMenuContext _context;
    private readonly GameSession _gameState;
    private readonly IReadOnlyList<CrimeMenuStatus> _crimeAttempts;
    private readonly GameScreen _parentScreen;
    private readonly GameRuntime _runtime;
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private readonly CrimeCommand _crimeCommand = new();
    private int _selectedIndex;

    public CrimeScreen(int width, int height, GameRuntime runtime, GameSession gameState, CrimeMenuContext context, IReadOnlyList<CrimeMenuStatus> crimeAttempts, GameScreen parentScreen)
        : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
        _context = context;
        _crimeAttempts = crimeAttempts;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Crime ===", Color.Cyan);
        Surface.Print(ListX, 4, $"Police Pressure: {_context.PolicePressure}", GetPressureColor(_context.PolicePressure));
        Surface.Print(DetailX, 2, "=== Route Detail ===", Color.Cyan);

        for (var i = 0; i < _crimeAttempts.Count; i++)
        {
            var attempt = _crimeAttempts[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = ListY + (i * ListRowHeight);
            var color = attempt.IsAvailable
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;
            Surface.Print(ListX, rowY, TrimToFit($"{prefix}{attempt.Attempt.Name}", DetailX - ListX - 2), color);
            var status = attempt.IsAvailable ? attempt.StatusText ?? "Ready to run." : attempt.BlockReason ?? "Blocked.";
            Surface.Print(ListX + 2, rowY + 1, TrimToFit(status, DetailX - ListX - 4), attempt.IsAvailable ? Color.Green : Color.Orange);
        }

        RenderSelectedCrimeDetails();

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

        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            AttemptSelectedCrime();
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
        for (var i = 0; i < _crimeAttempts.Count; i++)
        {
            var blockStartY = ListY + (i * ListRowHeight);
            if (cellPosition.Y < blockStartY || cellPosition.Y >= blockStartY + ListRowHeight)
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

        _crimeCommand.Execute(_gameState, selected.Attempt, _runtime.RandomSource.SharedRandom);
        ReturnToParentScreen();
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
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

    private void RenderSelectedCrimeDetails()
    {
        if (_crimeAttempts.Count == 0)
        {
            return;
        }

        var selected = _crimeAttempts[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Attempt.Name, Color.White);
        Surface.Print(DetailX, y++, $"Reward ~{selected.Attempt.BaseReward} LE", Color.Yellow);
        Surface.Print(DetailX, y++, $"Success {selected.EffectiveSuccessChance}% | Detection {selected.EffectiveDetectionRisk}%", Color.Yellow);
        Surface.Print(DetailX, y++, $"Pressure if seen +{selected.EffectivePressureIfDetected}", Color.Gray);
        Surface.Print(DetailX, y++, $"Pressure if clean +{selected.EffectivePressureIfUndetected}", Color.Gray);
        Surface.Print(DetailX, y++, $"Energy -{selected.Attempt.EnergyCost} | Street Rep {selected.Attempt.StreetRepRequired}", Color.Gray);

        y++;
        var summary = selected.IsAvailable ? selected.StatusText ?? "Route is open." : selected.BlockReason ?? "Route is blocked.";
        foreach (var line in WrapText(summary, detailWidth))
        {
            Surface.Print(DetailX, y++, line, selected.IsAvailable ? Color.Green : Color.Orange);
        }

        if (selected.AccessSignals.Count > 0 && y < Surface.Height - 4)
        {
            y++;
            Surface.Print(DetailX, y++, "Access signals:", Color.Cyan);
            foreach (var signal in selected.AccessSignals)
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

        if (selected.RiskNotes.Count > 0 && y < Surface.Height - 4)
        {
            y++;
            Surface.Print(DetailX, y++, "Risk notes:", Color.Cyan);
            foreach (var note in selected.RiskNotes)
            {
                foreach (var line in WrapText($"- {note}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.Gray);
                    if (y >= Surface.Height - 3)
                    {
                        return;
                    }
                }
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
}
