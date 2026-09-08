using Slums.Core.Diagnostics;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Characters;

/// <summary>Applies plant purchases, weekly care, and upgrades.</summary>
internal static class PlantAssetsService
{
    internal static bool BuyPlant(GameSession session, PlantType plantType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.PlantShop)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyPlant", before, session.CaptureStats(), "Not at plant shop");
            session.RaiseEvent("You need to be at the plant shop to buy plants.");
            return false;
        }

        if (!session.Player.HouseholdAssets.CanBuyPlant)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyPlant", before, session.CaptureStats(), "No room for more plants");
            session.RaiseEvent("There is no room left for more plants at home.");
            return false;
        }

        var definition = session.Player.HouseholdAssets.GetPlantDefinition(plantType);
        if (session.Player.Stats.Money < definition.OneTimeCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyPlant", before, session.CaptureStats(), $"Not enough money (need {definition.OneTimeCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. {definition.Name} costs {definition.OneTimeCost} LE.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-definition.OneTimeCost);
        session.Player.HouseholdAssets.BuyPlant(plantType, session.Clock.Day, session.CurrentWeek);
        session.RaiseEvent($"You buy {definition.Name} for {definition.OneTimeCost} LE and carry it back home.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "BuyPlant", before, session.CaptureStats(), $"Bought {definition.Name} for {definition.OneTimeCost} LE");
        return true;
    }

    internal static bool PayPlantCare(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "PayPlantCare", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to be home to water and supply the plants.");
            return false;
        }

        var cost = session.Player.HouseholdAssets.GetPlantCareCostDue(session.CurrentWeek);
        if (cost <= 0)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "PayPlantCare", before, session.CaptureStats(), "Plant care already covered");
            session.RaiseEvent("Plant care is already covered for this week.");
            return false;
        }

        if (session.Player.Stats.Money < cost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "PayPlantCare", before, session.CaptureStats(), $"Not enough money (need {cost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. Plant care supplies cost {cost} LE this week.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-cost);
        session.Player.HouseholdAssets.PayPlantCare(session.CurrentWeek);
        session.RaiseEvent($"You pay {cost} LE to keep the plants watered and supplied this week.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "PayPlantCare", before, session.CaptureStats(), $"Paid plant care {cost} LE");
        return true;
    }

    internal static bool UpgradePlant(GameSession session, Guid plantId, PlantUpgradeType upgradeType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to be home to work on the plants.");
            return false;
        }

        var plant = session.Player.HouseholdAssets.GetPlant(plantId);
        if (plant is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, session.CaptureStats(), "Plant not found");
            session.RaiseEvent("That plant is not in your flat anymore.");
            return false;
        }

        var cost = PlantUpgradeCatalog.GetCost(upgradeType);
        if (session.Player.Stats.Money < cost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, session.CaptureStats(), $"Not enough money (need {cost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. {PlantUpgradeCatalog.GetName(upgradeType)} costs {cost} LE.");
            return false;
        }

        if (!session.Player.HouseholdAssets.TryUpgradePlant(plantId, upgradeType, session.CurrentWeek))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradePlant", before, session.CaptureStats(), $"{PlantUpgradeCatalog.GetName(upgradeType)} already active");
            session.RaiseEvent($"{PlantUpgradeCatalog.GetName(upgradeType)} is already active for that plant.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-cost);
        var definition = session.Player.HouseholdAssets.GetPlantDefinition(plant.Type);
        session.RaiseEvent($"{definition.Name}: {PlantUpgradeCatalog.GetName(upgradeType)} added for {cost} LE.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "UpgradePlant", before, session.CaptureStats(), $"Upgraded {definition.Name} with {PlantUpgradeCatalog.GetName(upgradeType)} for {cost} LE");
        return true;
    }
}
