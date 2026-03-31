using System.Diagnostics.CodeAnalysis;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Slums.Application.Activities;
using Slums.Core.State;
using Slums.Game.Input;

namespace Slums.Game.Screens;

internal sealed class PhoneScreen : ScreenSurface
{
    private const int ListX = 2;
    private const int ListY = 7;
    private const int ListRowHeight = 2;
    private const int DetailX = 38;
    private readonly PhoneMenuContext _context;
    private readonly GameSession _gameState;
    private readonly PhoneMenuQuery _phoneMenuQuery = new();
    private readonly GameScreen _parentScreen;
    private readonly ScreenActionKeyGate _actionKeyGate = new();
    private PhoneMenuStatus _status;
    private int _selectedIndex;

    public PhoneScreen(int width, int height, GameSession gameState, PhoneMenuContext context, GameScreen parentScreen)
        : base(width, height)
    {
        _gameState = gameState;
        _context = context;
        _parentScreen = parentScreen;
        _status = _phoneMenuQuery.GetStatus(context);
        IsFocused = true;
        UseMouse = true;
        FocusOnMouseClick = true;
        _actionKeyGate.SuppressActionKeysUntilRelease();
    }

    public override void Render(TimeSpan delta)
    {
        base.Render(delta);
        Surface.Clear();

        Surface.Print(ListX, 2, "=== Phone ===", Color.Cyan);

        if (_context.PhoneLost)
        {
            Surface.Print(ListX, 4, "Your phone is lost. You need to replace it.", Color.Orange);
            Surface.Print(2, Surface.Height - 3, "R = Replace phone (25 LE) | Escape to cancel", Color.DarkGray);
            return;
        }

        if (!_context.PhoneOperational)
        {
            Surface.Print(ListX, 4, $"No credit. Refill for {_context.CreditWeekCost} LE to receive messages.", Color.Orange);
            Surface.Print(2, Surface.Height - 3, "R = Refill credit | Escape to cancel", Color.DarkGray);
            return;
        }

        Surface.Print(ListX, 4, $"Credit: {_context.CreditRemaining} days | Messages: {_status.Entries.Count}", Color.Gray);
        Surface.Print(DetailX, 2, "=== Message Detail ===", Color.Cyan);

        if (_status.Entries.Count == 0)
        {
            Surface.Print(ListX, ListY, "No messages or tips.", Color.DarkGray);
        }

        for (var i = 0; i < _status.Entries.Count; i++)
        {
            var entry = _status.Entries[i];
            var prefix = i == _selectedIndex ? "> " : "  ";
            var rowY = ListY + (i * ListRowHeight);
            var baseColor = i == _selectedIndex ? Color.Cyan : Color.White;

            if (entry.IsEmergency)
            {
                baseColor = i == _selectedIndex ? Color.Red : Color.Orange;
            }

            var label = $"{prefix}{entry.TypeIcon} {entry.Label}";
            Surface.Print(ListX, rowY, TrimToFit(label, DetailX - ListX - 2), baseColor);

            var metaColor = entry.IsEmergency ? Color.Orange : Color.Gray;
            var expiry = entry.DaysUntilExpiry.HasValue && entry.DaysUntilExpiry.Value <= 1
                ? " [expiring!]"
                : entry.DaysUntilExpiry.HasValue
                    ? $" [{entry.DaysUntilExpiry.Value}d]"
                    : "";
            Surface.Print(ListX + 2, rowY + 1, TrimToFit($"{entry.SourceName}{expiry}", DetailX - ListX - 4), metaColor);

            if (rowY + ListRowHeight >= Surface.Height - 4)
            {
                break;
            }
        }

        RenderSelectedEntryDetails();

        Surface.Print(2, Surface.Height - 3, "Arrow keys to select, Enter=acknowledge/respond, I=ignore, Escape to cancel", Color.DarkGray);
    }

    public override bool ProcessKeyboard([NotNull] Keyboard keyboard)
    {
        if (_status.Entries.Count == 0 && !_context.PhoneOperational)
        {
            if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.R))
                || keyboard.IsKeyPressed(Keys.R))
            {
                HandleRefillOrReplace();
                return true;
            }

            if (_actionKeyGate.TryConsumeCancel(keyboard.IsKeyPressed(Keys.Escape)))
            {
                ReturnToParentScreen();
                return true;
            }

            return base.ProcessKeyboard(keyboard);
        }

        if (keyboard.IsKeyPressed(Keys.Up) && _status.Entries.Count > 0)
        {
            _selectedIndex = (_selectedIndex - 1 + _status.Entries.Count) % _status.Entries.Count;
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Down) && _status.Entries.Count > 0)
        {
            _selectedIndex = (_selectedIndex + 1) % _status.Entries.Count;
            return true;
        }

        if (_actionKeyGate.TryConsumeConfirm(keyboard.IsKeyPressed(Keys.Enter)))
        {
            AcknowledgeOrRespond();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.I))
        {
            IgnoreSelected();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.R))
        {
            HandleRefillOrReplace();
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
        for (var i = 0; i < _status.Entries.Count; i++)
        {
            var blockStartY = ListY + (i * ListRowHeight);
            if (cellPosition.Y < blockStartY || cellPosition.Y >= blockStartY + ListRowHeight)
            {
                continue;
            }

            _selectedIndex = i;
            AcknowledgeOrRespond();
            return true;
        }

        return handled;
    }

    private void AcknowledgeOrRespond()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _status.Entries.Count)
        {
            return;
        }

        var entry = _status.Entries[_selectedIndex];

        if (entry.IsTip)
        {
            var (success, message) = _gameState.AcknowledgeTip(entry.Id);
            if (success)
            {
                _parentScreen.AddEventLogEntry($"Tip acknowledged: {message}");
            }
        }
        else
        {
            var (success, message) = _gameState.RespondToMessage(entry.Id);
            if (success)
            {
                _parentScreen.AddEventLogEntry($"Responded to message: {message}");
            }
        }

        RefreshStatus();
    }

    private void IgnoreSelected()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _status.Entries.Count)
        {
            return;
        }

        var entry = _status.Entries[_selectedIndex];

        if (entry.IsTip)
        {
            var (success, message, trustLoss) = _gameState.IgnoreTipAction(entry.Id);
            if (success)
            {
                var logMsg = trustLoss > 0
                    ? $"Ignored tip from {entry.SourceName}. Trust -{trustLoss}."
                    : $"Ignored tip from {entry.SourceName}.";
                _parentScreen.AddEventLogEntry(logMsg);
            }
        }
        else
        {
            var (success, message, trustLoss) = _gameState.IgnoreMessage(entry.Id);
            if (success)
            {
                _parentScreen.AddEventLogEntry($"Ignored message from {entry.SourceName}.");
            }
        }

        RefreshStatus();
    }

    private void HandleRefillOrReplace()
    {
        if (_context.PhoneLost)
        {
            var (success, message) = _gameState.ReplacePhone();
            if (success)
            {
                _parentScreen.AddEventLogEntry("Phone replaced for 25 LE.");
            }
            else
            {
                _parentScreen.AddEventLogEntry(message);
            }
        }
        else if (!_context.PhoneOperational)
        {
            var (success, message) = _gameState.RefillPhoneCredit();
            if (success)
            {
                _parentScreen.AddEventLogEntry($"Phone credit refilled: {message}");
            }
            else
            {
                _parentScreen.AddEventLogEntry(message);
            }
        }

        ReturnToParentScreen();
    }

    private void RefreshStatus()
    {
        var newContext = PhoneMenuContext.Create(_gameState);
        _status = _phoneMenuQuery.GetStatus(newContext);

        if (_selectedIndex >= _status.Entries.Count)
        {
            _selectedIndex = Math.Max(0, _status.Entries.Count - 1);
        }
    }

    private void ReturnToParentScreen()
    {
        IsFocused = false;
        _parentScreen.SuppressActionKeysUntilRelease();
        _parentScreen.IsFocused = true;
        GameHost.Instance.Screen = _parentScreen;
    }

    private void RenderSelectedEntryDetails()
    {
        if (_status.Entries.Count == 0)
        {
            return;
        }

        var entry = _status.Entries[_selectedIndex];
        var y = 4;
        var detailWidth = Surface.Width - DetailX - 2;

        Surface.Print(DetailX, y++, TrimToFit($"{entry.TypeIcon} {entry.Label}", detailWidth), entry.IsEmergency ? Color.Orange : Color.White);

        y++;
        Surface.Print(DetailX, y++, "Content:", Color.Cyan);
        foreach (var line in WrapText(entry.Content, detailWidth))
        {
            Surface.Print(DetailX, y++, line, Color.White);
            if (y >= Surface.Height - 5)
            {
                return;
            }
        }

        y++;
        Surface.Print(DetailX, y++, $"From: {entry.SourceName}", Color.Gray);

        if (entry.DaysUntilExpiry.HasValue)
        {
            var expiryColor = entry.DaysUntilExpiry.Value <= 1 ? Color.Red : Color.Gray;
            Surface.Print(DetailX, y++, $"Expires in: {entry.DaysUntilExpiry.Value} day(s)", expiryColor);
        }

        if (entry.IsEmergency)
        {
            y++;
            Surface.Print(DetailX, y++, "[URGENT - act immediately]", Color.Red);
        }

        y++;
        if (entry.IsTip)
        {
            Surface.Print(DetailX, y++, "Enter = Acknowledge | I = Ignore", Color.DarkGray);
        }
        else if (entry.RequiresResponse)
        {
            Surface.Print(DetailX, y++, "Enter = Respond | I = Ignore", Color.DarkGray);
        }
        else
        {
            Surface.Print(DetailX, y++, "Enter = Read/Dismiss | I = Ignore", Color.DarkGray);
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
