using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed record PlantUpgradeMenuContext(
    OwnedPlant Plant,
    PlantDefinition Definition,
    int CurrentWeek,
    int Money)
{
    public static PlantUpgradeMenuContext Create(GameSession gameSession, Guid plantId)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var plant = gameSession.Player.HouseholdAssets.GetPlant(plantId)
            ?? throw new InvalidOperationException("Plant not found.");
        var definition = PlantRegistry.GetByType(plant.Type);
        return new PlantUpgradeMenuContext(plant, definition, gameSession.CurrentWeek, gameSession.Player.Stats.Money);
    }
}
