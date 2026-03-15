using Slums.Core.Characters;

namespace Slums.Application.HouseholdAssets;

public sealed record HouseholdAssetsMenuStatus(
    HouseholdAssetActionType ActionType,
    string Title,
    string Summary,
    bool CanExecute,
    string Note,
    PetType? PetType = null,
    PlantType? PlantType = null,
    Guid? PlantId = null);
