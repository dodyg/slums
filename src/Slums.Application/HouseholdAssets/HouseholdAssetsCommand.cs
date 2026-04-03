using Slums.Core.Characters;
using Slums.Core.State;

namespace Slums.Application.HouseholdAssets;

public sealed class HouseholdAssetsCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, HouseholdAssetActionType actionType, PetType? petType = null, PlantType? plantType = null)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(gameSession);

        return actionType switch
        {
            HouseholdAssetActionType.AdoptCat => gameSession.AdoptStreetCat(),
            HouseholdAssetActionType.BuyFishTank => gameSession.BuyFishTank(),
            HouseholdAssetActionType.BuyPlant when plantType is PlantType concretePlantType => gameSession.BuyPlant(concretePlantType),
            HouseholdAssetActionType.PayPetCare => gameSession.PayPetCare(),
            HouseholdAssetActionType.PayPlantCare => gameSession.PayPlantCare(),
            HouseholdAssetActionType.ManagePlant => true,
            HouseholdAssetActionType.ManageFishTank => true,
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
        };
    }
}
