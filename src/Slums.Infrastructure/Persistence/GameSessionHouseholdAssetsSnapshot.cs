namespace Slums.Infrastructure.Persistence;

public sealed record GameSessionHouseholdAssetsSnapshot
{
    public IReadOnlyList<OwnedPetSnapshot> Pets { get; init; } = [];

    public IReadOnlyList<OwnedPlantSnapshot> Plants { get; init; } = [];

    public IReadOnlyList<OwnedRobotSnapshot> Robots { get; init; } = [];

    public int RobotParts { get; init; }

    public bool HasStreetCatEncounter { get; init; }

    public int LastStreetCatEncounterDay { get; init; }

    public int TotalHerbEarnings { get; init; }

    public static GameSessionHouseholdAssetsSnapshot Capture(Slums.Core.State.GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return new GameSessionHouseholdAssetsSnapshot
        {
            Pets = gameSession.Player.HouseholdAssets.Pets.Select(OwnedPetSnapshot.Capture).ToArray(),
            Plants = gameSession.Player.HouseholdAssets.Plants.Select(OwnedPlantSnapshot.Capture).ToArray(),
            Robots = gameSession.Player.Robotics.Robots.Select(OwnedRobotSnapshot.Capture).ToArray(),
            RobotParts = gameSession.Player.Robotics.Parts,
            HasStreetCatEncounter = gameSession.Player.HouseholdAssets.HasStreetCatEncounter,
            LastStreetCatEncounterDay = gameSession.Player.HouseholdAssets.LastStreetCatEncounterDay,
            TotalHerbEarnings = gameSession.Player.HouseholdAssets.TotalHerbEarnings
        };
    }
}
