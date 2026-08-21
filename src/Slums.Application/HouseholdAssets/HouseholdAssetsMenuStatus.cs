using Slums.Core.Characters;
using Slums.Core.Robotics;

namespace Slums.Application.HouseholdAssets;

public sealed record HouseholdAssetsMenuStatus(
    HouseholdAssetActionType ActionType,
    string Title,
    string Summary,
    bool CanExecute,
    string Note,
    PetType? PetType = null,
    PlantType? PlantType = null,
    Guid? PlantId = null,
    RobotType? RobotType = null,
    Guid? RobotId = null);
