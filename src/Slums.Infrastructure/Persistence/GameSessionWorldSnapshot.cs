using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionWorldSnapshot
{
    public string CurrentLocationId { get; init; } = LocationId.Home.Value;

    public IReadOnlyList<ActiveDistrictConditionSnapshot> ActiveDistrictConditions { get; init; } = [];

    public static GameSessionWorldSnapshot Capture(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionWorldSnapshot
        {
            CurrentLocationId = gameSession.World.CurrentLocationId.Value,
            ActiveDistrictConditions = gameSession.World.ActiveDistrictConditions
                .Select(ActiveDistrictConditionSnapshot.Capture)
                .ToArray()
        };
    }
}
