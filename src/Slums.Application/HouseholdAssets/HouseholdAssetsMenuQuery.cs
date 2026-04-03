using Slums.Core.Characters;
using Slums.Core.World;

namespace Slums.Application.HouseholdAssets;

public sealed class HouseholdAssetsMenuQuery
{
#pragma warning disable CA1822
    public IReadOnlyList<HouseholdAssetsMenuStatus> GetStatuses(HouseholdAssetsMenuContext context)
#pragma warning restore CA1822
    {
        ArgumentNullException.ThrowIfNull(context);

        var statuses = new List<HouseholdAssetsMenuStatus>();

        if (context.CurrentLocationId == LocationId.Home)
        {
            AddHomeStatuses(context, statuses);
        }

        if (context.CurrentLocationId == LocationId.FishMarket)
        {
            var fishDefinition = PetRegistry.GetByType(PetType.Fish);
            statuses.Add(new HouseholdAssetsMenuStatus(
                HouseholdAssetActionType.BuyFishTank,
                fishDefinition.Name,
                $"One-time cost {fishDefinition.OneTimeCost} LE | weekly food {fishDefinition.WeeklyCareCost} LE",
                context.Assets.CanBuyFishTank && context.Money >= fishDefinition.OneTimeCost,
                context.Assets.CanBuyFishTank
                    ? "Buy a single tank for the flat. It passively helps your mother and gets a bigger benefit when weekly care is covered."
                    : "You already have a fish tank at home.",
                PetType.Fish));
        }

        if (context.CurrentLocationId == LocationId.PlantShop)
        {
            foreach (var definition in PlantRegistry.AllDefinitions.OrderBy(static plant => plant.Name, StringComparer.Ordinal))
            {
                statuses.Add(new HouseholdAssetsMenuStatus(
                    HouseholdAssetActionType.BuyPlant,
                    definition.Name,
                    $"Buy {definition.OneTimeCost} LE | weekly care {definition.WeeklyCareCost} LE",
                    context.Assets.CanBuyPlant && context.Money >= definition.OneTimeCost,
                    BuildPlantPurchaseNote(definition, context.Assets.CanBuyPlant),
                    PlantType: definition.Type));
            }
        }

        return statuses;
    }

    private static void AddHomeStatuses(HouseholdAssetsMenuContext context, List<HouseholdAssetsMenuStatus> statuses)
    {
        var catDefinition = PetRegistry.GetByType(PetType.Cat);
        var catCount = context.Assets.Pets.Count(static pet => pet.Type == PetType.Cat);
        statuses.Add(new HouseholdAssetsMenuStatus(
            HouseholdAssetActionType.AdoptCat,
            catDefinition.Name,
            $"Free adoption | weekly food {catDefinition.WeeklyCareCost} LE | cats {catCount}/{catDefinition.MaxOwned}",
            context.Assets.CanAdoptCat,
            context.Assets.HasStreetCatEncounter
                ? "A stray has followed you home. If you adopt it now, this week is already covered."
                : "Cats require a street encounter before you can bring one home.",
            PetType.Cat));

        if (context.Assets.Pets.Count > 0)
        {
            var petCareDue = context.Assets.GetPetCareCostDue(context.CurrentWeek);
            statuses.Add(new HouseholdAssetsMenuStatus(
                HouseholdAssetActionType.PayPetCare,
                "Cover Pet Care",
                petCareDue > 0 ? $"Due now: {petCareDue} LE" : "Pet care already covered",
                petCareDue > 0 && context.Money >= petCareDue,
                "Weekly cat food and fish food keep the active mother-health bonus online and avoid neglect stress."));
        }

        if (context.Assets.Plants.Count > 0)
        {
            var plantCareDue = context.Assets.GetPlantCareCostDue(context.CurrentWeek);
            statuses.Add(new HouseholdAssetsMenuStatus(
                HouseholdAssetActionType.PayPlantCare,
                "Cover Plant Care",
                plantCareDue > 0 ? $"Due now: {plantCareDue} LE" : "Plant care already covered",
                plantCareDue > 0 && context.Money >= plantCareDue,
                "Weekly plant-care supplies keep their active household bonuses online and avoid neglect stress."));
        }

        var plantCounts = new Dictionary<PlantType, int>();
        foreach (var plant in context.Assets.Plants)
        {
            plantCounts.TryGetValue(plant.Type, out var existingCount);
            plantCounts[plant.Type] = existingCount + 1;
            var definition = PlantRegistry.GetByType(plant.Type);
            var label = plantCounts[plant.Type] == 1 ? definition.Name : $"{definition.Name} #{plantCounts[plant.Type]}";
            var upgradeCount = plant.GetActiveUpgradeCount(context.CurrentWeek);
            var careState = plant.IsBaseCarePaidForWeek(context.CurrentWeek) ? "care covered" : "care due";
            statuses.Add(new HouseholdAssetsMenuStatus(
                HouseholdAssetActionType.ManagePlant,
                $"Manage {label}",
                $"{careState} | upgrades active {upgradeCount}",
                true,
                BuildPlantManageNote(definition, plant, context.CurrentWeek),
                PlantType: plant.Type,
                PlantId: plant.Id));
        }

        var fishTank = context.Assets.GetFishTank();
        if (fishTank is not null)
        {
            var fishDefinition = PetRegistry.GetByType(PetType.Fish);
            var fishCareState = fishTank.IsUpkeepPaidForWeek(context.CurrentWeek) ? "care covered" : "care due";
            var fishUpgradeCount = fishTank.GetActiveUpgradeCount(context.CurrentWeek);
            statuses.Add(new HouseholdAssetsMenuStatus(
                HouseholdAssetActionType.ManageFishTank,
                "Manage Fish Tank",
                $"{fishCareState} | upgrades active {fishUpgradeCount}",
                true,
                BuildFishTankManageNote(fishDefinition, fishTank, context.CurrentWeek),
                PetType.Fish));
        }
    }

    private static string BuildPlantPurchaseNote(PlantDefinition definition, bool hasRoom)
    {
        if (!hasRoom)
        {
            return "You already have the maximum number of plants at home.";
        }

        return definition.Category switch
        {
            PlantCategory.CulinaryHerb => "Improves simple meals at home. Upgrades make the kitchen bonus stronger.",
            PlantCategory.SellableHerb => $"Auto-sells every {definition.HarvestCycleDays} days for {definition.HarvestSalePrice} LE before upgrade boosts.",
            PlantCategory.Flower => "Decorative flowers raise your mother's health bonus. Upgrades improve the effect.",
            PlantCategory.Medicinal => "Medicinal plant with a stronger direct mother-health bonus.",
            _ => definition.Description
        };
    }

    private static string BuildPlantManageNote(PlantDefinition definition, OwnedPlant plant, int currentWeek)
    {
        var activeUpgrades = new List<string>();
        foreach (var upgradeType in Enum.GetValues<PlantUpgradeType>())
        {
            if (plant.HasActiveUpgrade(upgradeType, currentWeek))
            {
                activeUpgrades.Add(PlantUpgradeCatalog.GetName(upgradeType));
            }
        }

        var activeSummary = activeUpgrades.Count == 0 ? "No upgrades active yet." : $"Active upgrades: {string.Join(", ", activeUpgrades)}.";
        return $"{definition.Description} {activeSummary}";
    }

    private static string BuildFishTankManageNote(PetDefinition definition, OwnedPet fishTank, int currentWeek)
    {
        var activeUpgrades = new List<string>();
        foreach (var upgradeType in Enum.GetValues<FishTankUpgradeType>())
        {
            if (fishTank.HasActiveUpgrade(upgradeType, currentWeek))
            {
                activeUpgrades.Add(FishTankUpgradeCatalog.GetName(upgradeType));
            }
        }

        var activeSummary = activeUpgrades.Count == 0 ? "No upgrades active yet." : $"Active upgrades: {string.Join(", ", activeUpgrades)}.";
        return $"{definition.Description} {activeSummary}";
    }
}
