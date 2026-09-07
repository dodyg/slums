using Slums.Core.State;

namespace Slums.Application.Activities;

/// <summary>
/// Records a UI-produced system message in the persistent event journal and raises it as a
/// session game event so the running screen log picks it up.
/// </summary>
public sealed class RecordSystemEventCommand
{
    public void Execute(GameSession gameSession, string message)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        gameSession.AddEventMessage(message);
        gameSession.EventJournal.Add(gameSession.Clock.Day, EventSource.System, message);
    }
}
