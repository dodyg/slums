using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.News;
using Slums.Core.State;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class NewsScreen : ScreenSurface
{
    private readonly GameSession _gameState;
    private readonly GameScreen _parentScreen;
    private readonly NewsMenuQuery _query = new();
    private readonly NewsResponseCommand _responseCommand = new();
    private readonly AcknowledgeNewsCommand _acknowledgeCommand = new();
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private NewsMenuStatus _status;
    private int _selectedIndex;

    public NewsScreen(int width, int height, GameSession gameState, GameScreen parentScreen) : base(width, height)
    {
        _gameState = gameState;
        _parentScreen = parentScreen;
        _status = GetStatus();
        IsFocused = true;
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();
        Surface.Print(2, 1, "=== City News ===", Color.Cyan);
        Surface.Print(2, 2, "Information creates choices; sources and uncertainty matter.", Color.Gray);

        if (_status.Flashes.Count == 0)
        {
            Surface.Print(2, 5, "No active city-wide news flashes.", Color.DarkGray);
        }

        for (var index = 0; index < _status.Flashes.Count; index++)
        {
            var flash = _status.Flashes[index];
            var color = index == _selectedIndex ? Color.Cyan : Color.White;
            Surface.Print(2, 4 + (index * 2), $"{(index == _selectedIndex ? ">" : " ")} {flash.Headline}", color);
            Surface.Print(4, 5 + (index * 2), $"{flash.Source} | {flash.Reliability} | {flash.DaysRemaining}d", Color.Gray);
        }

        RenderSelectedFlash();
        Surface.Print(2, Surface.Height - 2, "Arrows select | 1-3 respond | Enter mark read | Escape return", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up) && _status.Flashes.Count > 0)
        {
            _selectedIndex = (_selectedIndex - 1 + _status.Flashes.Count) % _status.Flashes.Count;
            return true;
        }
        if (keyboard.IsKeyPressed(Keys.Down) && _status.Flashes.Count > 0)
        {
            _selectedIndex = (_selectedIndex + 1) % _status.Flashes.Count;
            return true;
        }
        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            AcknowledgeSelected();
            return true;
        }
        var responseIndex = NumberKeyMapper.GetPressedNumberIndex(keyboard, 3);
        if (responseIndex.HasValue)
        {
            RespondSelected(responseIndex.Value);
            return true;
        }
        if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
        {
            ReturnToParentScreen();
            return true;
        }
        return base.ProcessKeyboard(keyboard);
    }

    private void RenderSelectedFlash()
    {
        if (_status.Flashes.Count == 0 || _selectedIndex >= _status.Flashes.Count)
        {
            return;
        }

        var flash = _status.Flashes[_selectedIndex];
        var y = 6 + (_status.Flashes.Count * 2);
        Surface.Print(48, 4, "Details", Color.Cyan);
        foreach (var line in Wrap(flash.Body, 48))
        {
            Surface.Print(48, y++, line, Color.White);
            if (y >= Surface.Height - 8)
            {
                break;
            }
        }
        y++;
        Surface.Print(48, y++, $"Areas: {string.Join(", ", flash.AffectedAreas)}", Color.Gray);
        for (var index = 0; index < flash.Responses.Count && index < 3; index++)
        {
            var response = flash.Responses[index];
            Surface.Print(48, y++, $"{index + 1}. {response.Label} [{response.CostSummary}]", response.IsAvailable ? Color.LightGreen : Color.DarkGray);
        }
    }

    private void AcknowledgeSelected()
    {
        if (_status.Flashes.Count == 0)
        {
            return;
        }
        var result = _acknowledgeCommand.Execute(_gameState, _status.Flashes[_selectedIndex].Id);
        if (result.Success)
        {
            _parentScreen.AddEventLogEntry(result.Message);
        }
        Refresh();
    }

    private void RespondSelected(int responseIndex)
    {
        if (_status.Flashes.Count == 0 || responseIndex >= _status.Flashes[_selectedIndex].Responses.Count)
        {
            return;
        }
        var flash = _status.Flashes[_selectedIndex];
        var result = _responseCommand.Execute(_gameState, flash.Id, flash.Responses[responseIndex].Id);
        _parentScreen.AddEventLogEntry(result.Message);
        Refresh();
    }

    private void Refresh()
    {
        _status = GetStatus();
        if (_selectedIndex >= _status.Flashes.Count)
        {
            _selectedIndex = Math.Max(0, _status.Flashes.Count - 1);
        }
    }

    private NewsMenuStatus GetStatus() => _query.GetStatus(NewsMenuContext.Create(_gameState), _gameState.Inventory.Quantities, _gameState.Player.Stats.Money);

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        ScreenTransition.SwitchTo(_parentScreen);
    }

    private static IEnumerable<string> Wrap(string text, int width)
    {
        var current = string.Empty;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = string.IsNullOrEmpty(current) ? word : $"{current} {word}";
            if (candidate.Length > width && current.Length > 0)
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
