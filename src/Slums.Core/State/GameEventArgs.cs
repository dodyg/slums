namespace Slums.Core.State;

public sealed class GameEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}
