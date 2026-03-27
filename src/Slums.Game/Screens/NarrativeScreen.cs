using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Narrative;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class NarrativeScreen : ScreenSurface
{
    private readonly INarrativeService _narrativeService;
    private readonly INarrativeOutcomeTarget _narrativeOutcomeTarget;
    private readonly ScreenSurface _nextScreen;
    private readonly List<string> _wrappedLines = [];
    private int _selectedChoiceIndex;
    private int _scrollOffset;

    public NarrativeScreen(int width, int height, INarrativeService narrativeService, INarrativeOutcomeTarget narrativeOutcomeTarget, ScreenSurface nextScreen)
        : base(width, height)
    {
        _narrativeService = narrativeService;
        _narrativeOutcomeTarget = narrativeOutcomeTarget;
        _nextScreen = nextScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        RefreshWrappedText();
    }

    public override void Update(TimeSpan delta)
    {
        base.Update(delta);
        RefreshWrappedText();
        HandleCompletion();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(2, 1, "=== Scene ===", Color.Cyan);

        const int textPanelTop = 3;
        var textPanelHeight = Surface.Height - 10;

        for (var row = 0; row < textPanelHeight; row++)
        {
            var lineIndex = _scrollOffset + row;
            if (lineIndex >= _wrappedLines.Count)
            {
                break;
            }

            Surface.Print(2, textPanelTop + row, _wrappedLines[lineIndex], Color.White);
        }

        if (_wrappedLines.Count > textPanelHeight)
        {
            Surface.Print(Surface.Width - 16, 1, $"Scroll {_scrollOffset + 1}/{Math.Max(1, _wrappedLines.Count - textPanelHeight + 1)}", Color.DarkGray);
        }

        var choiceStartY = Surface.Height - 5;
        if (_narrativeService.CurrentChoices.Count > 0)
        {
            for (var i = 0; i < _narrativeService.CurrentChoices.Count; i++)
            {
                var prefix = i == _selectedChoiceIndex ? "> " : "  ";
                var color = i == _selectedChoiceIndex ? Color.Yellow : Color.Gray;
                Surface.Print(2, choiceStartY + i, $"{prefix}{i + 1}. {_narrativeService.CurrentChoices[i]}", color);
            }
        }
        else if (!string.IsNullOrWhiteSpace(_narrativeService.CurrentText))
        {
            Surface.Print(2, choiceStartY + 1, "[Press Enter to continue]", Color.Yellow);
        }
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            if (_narrativeService.CurrentChoices.Count > 0)
            {
                _selectedChoiceIndex = (_selectedChoiceIndex - 1 + _narrativeService.CurrentChoices.Count) % _narrativeService.CurrentChoices.Count;
            }
            else
            {
                _scrollOffset = Math.Max(0, _scrollOffset - 1);
            }

            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            if (_narrativeService.CurrentChoices.Count > 0)
            {
                _selectedChoiceIndex = (_selectedChoiceIndex + 1) % _narrativeService.CurrentChoices.Count;
            }
            else
            {
                _scrollOffset = Math.Min(Math.Max(0, _wrappedLines.Count - (Surface.Height - 10)), _scrollOffset + 1);
            }

            return true;
        }

        for (var i = 0; i < Math.Min(9, _narrativeService.CurrentChoices.Count); i++)
        {
            if (keyboard.IsKeyPressed(Keys.D1 + i) || keyboard.IsKeyPressed(Keys.NumPad1 + i))
            {
                _selectedChoiceIndex = i;
                _narrativeService.SelectChoice(i);
                RefreshWrappedText();
                HandleCompletion();
                return true;
            }
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            if (_narrativeService.CurrentChoices.Count > 0)
            {
                _narrativeService.SelectChoice(_selectedChoiceIndex);
                RefreshWrappedText();
                HandleCompletion();
                return true;
            }

            _narrativeService.EndScene();
            HandleCompletion();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    private void RefreshWrappedText()
    {
        _wrappedLines.Clear();
        var text = _narrativeService.CurrentText;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var paragraphs = text.Replace("\r", string.Empty, StringComparison.Ordinal).Split("\n", StringSplitOptions.None);
        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                _wrappedLines.Add(string.Empty);
                continue;
            }

            _wrappedLines.AddRange(WrapLine(paragraph.Trim(), Surface.Width - 4));
            _wrappedLines.Add(string.Empty);
        }

        if (_wrappedLines.Count > 0 && string.IsNullOrWhiteSpace(_wrappedLines[^1]))
        {
            _wrappedLines.RemoveAt(_wrappedLines.Count - 1);
        }
    }

    private void HandleCompletion()
    {
        if (_narrativeService.IsSceneActive)
        {
            return;
        }

        var outcome = _narrativeService.GetPendingOutcome();
        if (outcome is not null)
        {
            _narrativeOutcomeTarget.ApplyOutcome(outcome);
            _narrativeService.ClearPendingOutcome();
        }

        IsFocused = false;
        _nextScreen.IsFocused = true;
        GameHost.Instance.Screen = _nextScreen;
    }

    private static IEnumerable<string> WrapLine(string text, int maxWidth)
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
