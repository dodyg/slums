using Slums.Core.Characters;

namespace Slums.Application.HouseholdAssets;

public sealed record PlantUpgradeMenuStatus(
    PlantUpgradeType UpgradeType,
    string Name,
    int Cost,
    bool CanExecute,
    string Note);
