using Slums.Core.Characters;

namespace Slums.Application.HouseholdAssets;

public sealed record FishTankUpgradeMenuStatus(
    FishTankUpgradeType UpgradeType,
    string Name,
    int Cost,
    bool CanExecute,
    string Note);
