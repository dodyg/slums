using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed record FishTankUpgradeMenuContext(
    OwnedPet FishTank,
    PetDefinition Definition,
    int CurrentWeek,
    int Money)
{
    public static FishTankUpgradeMenuContext Create(GameSession gameSession)
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        var fishTank = gameSession.Player.HouseholdAssets.GetFishTank()
            ?? throw new InvalidOperationException("No fish tank owned.");
        var definition = PetRegistry.GetByType(PetType.Fish);
        return new FishTankUpgradeMenuContext(fishTank, definition, gameSession.CurrentWeek, gameSession.Player.Stats.Money);
    }
}
