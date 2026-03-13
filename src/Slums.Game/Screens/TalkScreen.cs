using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class TalkScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 6;
    private const int ListRowHeight = 2;
    private const int DetailX = 34;
    private readonly GameRuntime _runtime;
    private readonly GameState _gameState;
    private readonly IReadOnlyList<TalkNpcStatus> _npcs;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public TalkScreen(int width, int height, GameRuntime runtime, GameState gameState, IReadOnlyList<TalkNpcStatus> npcs, GameScreen parentScreen)
        : base(width, height)
    {
        _runtime = runtime;
        _gameState = gameState;
        _npcs = npcs;
        _parentScreen = parentScreen;
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Talk ===", Color.Cyan);
        Surface.Print(ListX, 4, "Choose who to speak with:", Color.Gray);
        Surface.Print(DetailX, 2, "=== Relationship Detail ===", Color.Cyan);

        for (var i = 0; i < _npcs.Count; i++)
        {
            var npc = _npcs[i];
            var rowY = ListY + (i * ListRowHeight);
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            Surface.Print(ListX, rowY, TrimToFit($"{prefix}{npc.Name}", DetailX - ListX - 2), color);
            Surface.Print(ListX + 2, rowY + 1, $"Trust: {npc.Trust}", npc.Trust < 0 ? Color.Orange : Color.Gray);
        }

        RenderSelectedNpcDetails();

        Surface.Print(2, Surface.Height - 2, "Arrow keys to select, Enter to talk, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _npcs.Count) % _npcs.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _npcs.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            var npcId = _npcs[_selectedIndex].NpcId;
            _gameState.Relationships.RecordContact(npcId, _gameState.Clock.Day);
            var knotName = NpcRegistry.GetConversationKnot(
                npcId,
                _gameState.Relationships,
                _gameState.PolicePressure,
                _gameState.Clock.Day,
                _gameState.HonestShiftsCompleted,
                _gameState.CrimesCommitted,
                _gameState.Player.Stats.Money,
                _gameState.Player.Household.MotherHealth);
            _gameState.Relationships.RecordSeenConversation(npcId, knotName);
            _runtime.NarrativeService.StartScene(knotName, _gameState);
            IsFocused = false;
            GameHost.Instance.Screen = new NarrativeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime.NarrativeService, _gameState, _parentScreen);
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

    private void RenderSelectedNpcDetails()
    {
        if (_npcs.Count == 0)
        {
            return;
        }

        var selected = _npcs[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, selected.Name, Color.White);
        Surface.Print(DetailX, y++, $"Trust: {selected.Trust}", selected.Trust < 0 ? Color.Orange : Color.Gray);
        y++;

        foreach (var line in WrapText(selected.Summary, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.White);
        }

        if (!string.IsNullOrWhiteSpace(selected.FactionLink))
        {
            y++;
            Surface.Print(DetailX, y++, selected.FactionLink, Color.Yellow);
        }

        if (selected.TriggerSignals.Count > 0)
        {
            y++;
            Surface.Print(DetailX, y++, "Conversation triggers:", Color.Cyan);
            foreach (var signal in selected.TriggerSignals)
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

        if (selected.MemoryFlags.Count > 0)
        {
            y++;
            Surface.Print(DetailX, y++, "Memory flags:", Color.Cyan);
            foreach (var flag in selected.MemoryFlags)
            {
                foreach (var line in WrapText($"- {flag}", detailWidth))
                {
                    Surface.Print(DetailX, y++, line, Color.Gray);
                }

                if (y >= Surface.Height - 3)
                {
                    break;
                }
            }
        }
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
}