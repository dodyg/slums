using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class TalkScreen : ScreenSurface
{
    private readonly GameRuntime _runtime;
    private readonly GameState _gameState;
    private readonly IReadOnlyList<NpcId> _npcs;
    private readonly GameScreen _parentScreen;
    private int _selectedIndex;

    public TalkScreen(int width, int height, GameRuntime runtime, GameState gameState, IReadOnlyList<NpcId> npcs, GameScreen parentScreen)
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

        Surface.Print(2, 2, "=== Talk ===", Color.Cyan);
        Surface.Print(2, 4, "Choose who to speak with:", Color.Gray);

        var y = 6;
        for (var i = 0; i < _npcs.Count; i++)
        {
            var npcId = _npcs[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var color = i == _selectedIndex ? Color.Cyan : Color.White;
            var trust = _gameState.Relationships.GetNpcRelationship(npcId).Trust;
            Surface.Print(2, y++, $"{prefix}{NpcRegistry.GetName(npcId)}", color);
            Surface.Print(4, y++, $"Trust: {trust}", trust < 0 ? Color.Orange : Color.Gray);
        }

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
            var npcId = _npcs[_selectedIndex];
            var knotName = NpcRegistry.GetConversationKnot(npcId, _gameState.Relationships, _gameState.PolicePressure);
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
}