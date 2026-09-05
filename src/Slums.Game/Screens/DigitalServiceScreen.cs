using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Technology;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class DigitalServiceScreen : ScreenSurface
{
    private readonly GameSession _gameState;
    private readonly DigitalServiceMenuContext _context;
    private readonly List<DigitalServiceMenuStatus> _actions;
    private readonly GameScreen _parentScreen;
    private readonly DigitalServiceCommand _command = new();

    public DigitalServiceScreen(int width, int height, GameSession gameState, DigitalServiceMenuContext context, List<DigitalServiceMenuStatus> actions, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _actions = actions;
        _parentScreen = parentScreen;
        IsFocused = true;
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();
        Surface.Print(2, 2, "=== Digital Services ===", Color.Cyan);
        Surface.Print(2, 3, $"Digital Literacy: {_context.DigitalLiteracySkillLevel} | Handset exposure: {_context.HandsetExposure} | Review pending: {_context.BiometricAppealPending}", Color.Gray);

        for (var i = 0; i < _actions.Count; i++)
        {
            var status = _actions[i];
            var color = status.CanPerform ? Color.White : Color.Gray;
            Surface.Print(2, 5 + i * 3, status.Preview.Action.Name, color);
            Surface.Print(4, 6 + i * 3, status.CanPerform
                ? $"{status.Preview.SuccessChance}% success | {status.Preview.Action.MoneyCost} LE | {status.Preview.Action.TimeCostMinutes}m | review remains pending"
                : status.UnavailabilityReason ?? "Unavailable", status.CanPerform ? Color.Green : Color.Orange);
            Surface.Print(4, 7 + i * 3, status.Preview.Action.Description, Color.DarkGray);
        }

        Surface.Print(2, Surface.Height - 2, "Enter to submit selected service, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            var selected = _actions[0];
            if (selected.CanPerform)
            {
                _command.Execute(_gameState, selected.Preview.Action.Type);
                ReturnToParentScreen();
            }

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
        _parentScreen.SuppressActionKeysUntilRelease();
        ScreenTransition.ReturnTo(_parentScreen);
    }
}
