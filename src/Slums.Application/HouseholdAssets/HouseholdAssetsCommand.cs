using Slums.Core.Characters;
using Slums.Core.State;
using Slums.Core.Robotics;

namespace Slums.Application.HouseholdAssets;

public sealed class HouseholdAssetsCommand
{
#pragma warning disable CA1822
    public bool Execute(GameSession gameSession, HouseholdAssetActionType actionType, PetType? petType = null, PlantType? plantType = null, RobotType? robotType = null, Guid? robotId = null)
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
            HouseholdAssetActionType.BuyRobot when robotType is RobotType concreteRobotType => gameSession.BuyRobot(concreteRobotType),
            HouseholdAssetActionType.BuyRobotParts => gameSession.BuyRobotParts(),
            HouseholdAssetActionType.RepairRobot when robotId is Guid concreteRobotId => gameSession.RepairRobot(concreteRobotId),
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
        };
    }
}
