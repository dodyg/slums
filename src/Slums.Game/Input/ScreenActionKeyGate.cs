namespace Slums.Game.Input;

/// <summary>
/// Prevents held confirm/cancel keys from retriggering actions across screen transitions.
/// </summary>
internal sealed class ScreenActionKeyGate
{
    private bool _confirmSuppressed;
    private bool _cancelSuppressed;

    public void SuppressConfirmUntilRelease()
    {
        _confirmSuppressed = true;
    }

    public void SuppressCancelUntilRelease()
    {
        _cancelSuppressed = true;
    }

    public void SuppressActionKeysUntilRelease()
    {
        SuppressConfirmUntilRelease();
        SuppressCancelUntilRelease();
    }

    public bool TryConsumeConfirm(bool isPressed)
    {
        if (!isPressed)
        {
            _confirmSuppressed = false;
            return false;
        }

        if (_confirmSuppressed)
        {
            return false;
        }

        _confirmSuppressed = true;
        return true;
    }

    public bool TryConsumeCancel(bool isPressed)
    {
        if (!isPressed)
        {
            _cancelSuppressed = false;
            return false;
        }

        if (_cancelSuppressed)
        {
            return false;
        }

        _cancelSuppressed = true;
        return true;
    }
}
