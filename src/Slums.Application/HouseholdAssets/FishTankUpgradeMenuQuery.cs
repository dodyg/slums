using Slums.Core.Characters;

namespace Slums.Application.HouseholdAssets;

public sealed class FishTankUpgradeMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<FishTankUpgradeMenuStatus> GetStatuses(FishTankUpgradeMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        return Enum
            .GetValues<FishTankUpgradeType>()
            .Select(upgradeType => BuildStatus(context, upgradeType))
            .ToArray();
    }

    private static FishTankUpgradeMenuStatus BuildStatus(FishTankUpgradeMenuContext context, FishTankUpgradeType upgradeType)
    {
        var cost = FishTankUpgradeCatalog.GetCost(upgradeType);
        var canExecute = context.FishTank.CanPurchaseUpgrade(upgradeType, context.CurrentWeek) && context.Money >= cost;
        return new FishTankUpgradeMenuStatus(
            upgradeType,
            FishTankUpgradeCatalog.GetName(upgradeType),
            cost,
            canExecute,
            BuildNote(context, upgradeType));
    }

    private static string BuildNote(FishTankUpgradeMenuContext context, FishTankUpgradeType upgradeType)
    {
        var bonusSummary = $"Adds +{FishTankUpgradeCatalog.GetMotherHealthBonusPerUpgrade} to the mother's health bonus while active.";

        return upgradeType switch
        {
            FishTankUpgradeType.BetterFilter => context.FishTank.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already owned permanently."
                : $"Permanent upgrade. {bonusSummary}",
            FishTankUpgradeType.Heater => context.FishTank.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already owned permanently."
                : $"Permanent upgrade. {bonusSummary}",
            FishTankUpgradeType.Decorations => context.FishTank.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already paid for this week."
                : $"Recurring weekly upgrade. {bonusSummary}",
            FishTankUpgradeType.WaterConditioner => context.FishTank.HasActiveUpgrade(upgradeType, context.CurrentWeek)
                ? "Already paid for this week."
                : $"Recurring weekly upgrade. {bonusSummary}",
            _ => context.Definition.Description
        };
    }
}
