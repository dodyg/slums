using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionInfrastructureSnapshot
{
    public IReadOnlyList<InfrastructureServiceState> Services { get; init; } = [];

    public static GameSessionInfrastructureSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        return new GameSessionInfrastructureSnapshot { Services = gameSession.Infrastructure.Services.ToArray() };
    }

    public void Restore(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);
        gameSession.Infrastructure.Restore(Services);
    }
}
