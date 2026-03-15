using Slums.Core.Characters;

namespace Slums.Application.HouseholdAssets;

public sealed class PlantUpgradeMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<PlantUpgradeMenuStatus> GetStatuses(PlantUpgradeMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return Enum
            .GetValues<PlantUpgradeType>()
            .Select(upgradeType => BuildStatus(context, upgradeType))
            .ToArray();
    }

    private static PlantUpgradeMenuStatus BuildStatus(PlantUpgradeMenuContext context, PlantUpgradeType upgradeType)
    {
        var cost = PlantUpgradeCatalog.GetCost(upgradeType);
        var canExecute = context.Plant.CanPurchaseUpgrade(upgradeType, context.CurrentWeek) && context.Money >= cost;
        return new PlantUpgradeMenuStatus(
            upgradeType,
            PlantUpgradeCatalog.GetName(upgradeType),
            cost,
            canExecute,
            BuildNote(context, upgradeType));
    }

    private static string BuildNote(PlantUpgradeMenuContext context, PlantUpgradeType upgradeType)
    {
        var typeSummary = context.Definition.Category switch
        {
            PlantCategory.CulinaryHerb => "Boosts the home cooking bonus.",
            PlantCategory.SellableHerb => $"Adds +{context.Definition.HarvestPriceBonusPerUpgrade} LE to each harvest while active.",
            _ => $"Adds +{context.Definition.MotherHealthBonusPerUpgrade} to the mother's health bonus while active."
        };

        return upgradeType switch
        {
            PlantUpgradeType.BiggerPot => context.Plant.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already owned permanently."
                : $"Permanent upgrade. {typeSummary}",
            PlantUpgradeType.WindowPlacement => context.Plant.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already owned permanently."
                : $"Permanent upgrade. {typeSummary}",
            PlantUpgradeType.Fertilizer => context.Plant.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already paid for this week."
                : $"Recurring weekly upgrade. {typeSummary}",
            PlantUpgradeType.Irrigation => context.Plant.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already paid for this week."
                : $"Recurring weekly upgrade. {typeSummary}",
            _ => context.Definition.Description
        };
    }
}
