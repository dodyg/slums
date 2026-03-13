using Slums.Core.State;

namespace Slums.Application.Persistence;

public sealed record SaveGameRequest(GameSession GameSession, string CheckpointName, string? LastKnot)
{
    public static SaveGameRequest Create(GameSession gameSession, string? lastKnot)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var backgroundName = gameSession.Player.Background?.Name ?? gameSession.Player.BackgroundType.ToString();
        return new SaveGameRequest(gameSession, $"{backgroundName} - Day {gameSession.Clock.Day}", lastKnot);
    }
}
