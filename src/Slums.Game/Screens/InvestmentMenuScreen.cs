using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Investments;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class InvestmentMenuScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 5;
    private const int ListRowHeight = 2;
    private const int DetailX = 40;
    private readonly InvestmentMenuContext _context;
    private readonly GameSession _gameState;
    private readonly List<InvestmentMenuStatus> _opportunities;
    private readonly GameScreen _parentScreen;
    private readonly MakeInvestmentCommand _makeInvestmentCommand = new();
    private int _selectedIndex;

    public InvestmentMenuScreen(int width, int height, GameSession gameState, InvestmentMenuContext context, List<InvestmentMenuStatus> opportunities, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _opportunities = opportunities;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Investments ===", Color.Cyan);
        Surface.Print(ListX, 3, $"Location: {_context.LocationName ?? "Unknown"}", Color.Gray);
        Surface.Print(DetailX, 2, "=== Opportunity Details ===", Color.Cyan);

        for (var i = 0; i < _opportunities.Count; i++)
        {
            var status = _opportunities[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = ListY + (i * ListRowHeight);
            var color = status.CanInvest
                ? i == _selectedIndex ? Color.Cyan : Color.White
                : i == _selectedIndex ? Color.Orange : Color.Gray;

            Surface.Print(ListX, rowY, TrimToFit($"{prefix}{status.Definition.Name}", DetailX - ListX - 2), color);
            Surface.Print(ListX + 2, rowY + 1, TrimToFit(GetStatusLine(status), DetailX - ListX - 4), status.CanInvest ? Color.Green : Color.Orange);
        }

        RenderSelectedOpportunityDetails();

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter to invest, Escape to cancel", Color.DarkGray);
        Surface.Print(2, Surface.Height - 2, $"Money: {_gameState.Player.Stats.Money} LE | Active: {_gameState.ActiveInvestments.Count} | Earnings: {_gameState.TotalInvestmentEarnings} LE", Color.Gold);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _opportunities.Count) % _opportunities.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _opportunities.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            BuySelectedInvestment();
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
        for (var i = 0; i < _opportunities.Count; i++)
        {
            var blockStartY = ListY + i * ListRowHeight;
            if (cellPosition.Y >= blockStartY &&
                cellPosition.Y < blockStartY + ListRowHeight &&
                cellPosition.X >= ListX &&
                cellPosition.X < DetailX - 1)
            {
                _selectedIndex = i;
                BuySelectedInvestment();
                return true;
            }
        }

        return handled;
    }

    private void BuySelectedInvestment()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _opportunities.Count)
        {
            return;
        }

        var status = _opportunities[_selectedIndex];
        if (!status.CanInvest)
        {
            return;
        }

        _makeInvestmentCommand.Execute(_gameState, status.Definition.Type);
        ReturnToParentScreen();
    }

    private static string GetStatusLine(InvestmentMenuStatus status)
    {
        return status.CanInvest
            ? $"Cost: {status.Definition.Cost} LE | Return: {status.WeeklyReturnSummary}"
            : status.BlockingReasons.Count > 0 ? status.BlockingReasons[0] : "Not available";
    }

    private void RenderSelectedOpportunityDetails()
    {
        if (_opportunities.Count == 0)
        {
            return;
        }

        var selected = _opportunities[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Definition.Name, Color.White);
        foreach (var line in WrapText(selected.Definition.Description, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        y++;
        Surface.Print(DetailX, y++, $"Cost: {selected.Definition.Cost} LE", Color.Yellow);
        Surface.Print(DetailX, y++, $"Weekly return: {selected.WeeklyReturnSummary}", Color.Green);
        Surface.Print(DetailX, y++, $"Risk: {selected.Definition.RiskLabel}", GetRiskColor(selected.Definition.RiskLabel));

        foreach (var entry in selected.RiskBreakdown)
        {
            foreach (var line in WrapText(entry, detailWidth))
            {
                Surface.Print(DetailX, y++, line, Color.Gray);
            }
        }

        y++;
        foreach (var line in WrapText(selected.OpportunitySource, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.White);
        }

        y++;
        Surface.Print(DetailX, y++, "Requirements:", Color.Cyan);
        foreach (var line in WrapText(selected.UnlockSummary, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.Gray);
        }

        if (!string.IsNullOrWhiteSpace(selected.OwnedStateSummary))
        {
            y++;
            Surface.Print(DetailX, y++, "Current stake:", Color.Cyan);
            foreach (var line in WrapText(selected.OwnedStateSummary, detailWidth))
            {
                Surface.Print(DetailX, y++, line, selected.CanInvest ? Color.LightGray : Color.Orange);
            }
        }

        if (selected.CurrentStateNotes.Count > 0)
        {
            y++;
            Surface.Print(DetailX, y++, "Status notes:", Color.Cyan);
            foreach (var note in selected.CurrentStateNotes)
            {
                foreach (var line in WrapText($"- {note}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.LightGray);
                    if (y >= Surface.Height - 3)
                    {
                        return;
                    }
                }
            }
        }

        if (!selected.CanInvest)
        {
            y++;
            Surface.Print(DetailX, y++, "Blocked by:", Color.Cyan);
            foreach (var reason in selected.BlockingReasons)
            {
                foreach (var line in WrapText($"- {reason}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.Orange);
                    if (y >= Surface.Height - 3)
                    {
                        return;
                    }
                }
            }
        }
    }

    private static Color GetRiskColor(string riskLabel)
    {
        return riskLabel switch
        {
            "Low" => Color.Green,
            "Medium" => Color.Yellow,
            "Medium-High" => Color.Orange,
            _ => Color.Red
        };
    }

    private static string TrimToFit(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : $"{text[..Math.Max(0, maxLength - 3)]}...";
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
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }
}
