using Slums.Core.Diagnostics;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Characters;

/// <summary>Applies household asset purchases, care, upgrades, and encounters.</summary>
internal static class HouseholdAssetsService
{
    internal static bool CanUse(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.World.CurrentLocationId == LocationId.FishMarket
            || session.World.CurrentLocationId == LocationId.PlantShop
            || session.World.CurrentLocationId == LocationId.Workshop
            || (session.World.CurrentLocationId == LocationId.Home
                && (session.Player.HouseholdAssets.HasAnyAssets
                    || session.Player.HouseholdAssets.HasStreetCatEncounter
                    || session.Player.Robotics.HasAnyRobots));
    }

    internal static bool AdoptStreetCat(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "AdoptStreetCat", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to be home to bring a street cat inside.");
            return false;
        }

        if (!session.Player.HouseholdAssets.AdoptCat(session.Clock.Day, session.CurrentWeek))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "AdoptStreetCat", before, session.CaptureStats(), "No cat encounter available");
            session.RaiseEvent("No stray cat is trusting you enough to come home right now.");
            return false;
        }

        session.RaiseEvent("The cat slips inside, claims a corner, and your mother smiles for the first time all day.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "AdoptStreetCat", before, session.CaptureStats(), "Adopted street cat");
        return true;
    }

    internal static bool BuyFishTank(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.FishMarket)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyFishTank", before, session.CaptureStats(), "Not at fish market");
            session.RaiseEvent("You need to be at the fish market to buy a tank.");
            return false;
        }

        if (!session.Player.HouseholdAssets.CanBuyFishTank)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyFishTank", before, session.CaptureStats(), "Already have a fish tank");
            session.RaiseEvent("There is already a fish tank at home.");
            return false;
        }

        var definition = PetRegistry.GetByType(PetType.Fish);
        if (session.Player.Stats.Money < definition.OneTimeCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyFishTank", before, session.CaptureStats(), $"Not enough money (need {definition.OneTimeCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. A fish tank costs {definition.OneTimeCost} LE.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-definition.OneTimeCost);
        session.Player.HouseholdAssets.BuyFishTank(session.Clock.Day, session.CurrentWeek);
        session.RaiseEvent($"You carry a modest fish tank home from the market for {definition.OneTimeCost} LE.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "BuyFishTank", before, session.CaptureStats(), $"Bought fish tank for {definition.OneTimeCost} LE");
        return true;
    }

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

        var definition = PlantRegistry.GetByType(plantType);
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

    internal static bool BuyRobot(GameSession session, RobotType robotType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Workshop)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, session.CaptureStats(), "Not at workshop");
            session.RaiseEvent("Abu Samir only sells machines from the workshop bench.");
            return false;
        }

        var definition = RobotRegistry.GetByType(robotType);
        if (!session.Player.Robotics.CanPurchaseRobot)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, session.CaptureStats(), "Robot limit reached");
            session.RaiseEvent($"The flat and the alley can only support {RobotRegistry.MaxOwnedRobots} machines at once.");
            return false;
        }

        if (session.Player.Robotics.Robots.Any(robot => robot.Type == robotType))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, session.CaptureStats(), "Already own this robot model");
            session.RaiseEvent($"You already own a {definition.Name}.");
            return false;
        }

        if (session.Player.Stats.Money < definition.PurchaseCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, session.CaptureStats(), $"Not enough money (need {definition.PurchaseCost} LE)");
            session.RaiseEvent($"You need {definition.PurchaseCost} LE for the {definition.Name}; the seller will not extend credit.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-definition.PurchaseCost);
        session.Player.Robotics.PurchaseRobot(robotType, session.Clock.Day);
        session.RaiseEvent($"You buy a {definition.Name} for {definition.PurchaseCost} LE. It works, but its warranty expired years ago.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "BuyRobot", before, session.CaptureStats(), $"Bought {definition.Name} for {definition.PurchaseCost} LE");
        return true;
    }

    internal static bool BuyRobotParts(GameSession session, int quantity)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Workshop)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, session.CaptureStats(), "Not at workshop");
            session.RaiseEvent("You need Abu Samir's workshop bench to buy robot parts.");
            return false;
        }

        if (quantity <= 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        }

        var cost = quantity * RobotRegistry.PartsPurchaseCost;
        if (!session.Player.Robotics.CanBuyParts(quantity))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, session.CaptureStats(), "Parts storage limit reached");
            session.RaiseEvent($"You can carry at most {RobotRegistry.MaxParts} spare robot parts in the flat.");
            return false;
        }

        if (session.Player.Stats.Money < cost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, session.CaptureStats(), $"Not enough money (need {cost} LE)");
            session.RaiseEvent($"You need {cost} LE for {quantity} robot part{(quantity == 1 ? string.Empty : "s")}.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-cost);
        session.Player.Robotics.AddParts(quantity);
        session.RaiseEvent($"You buy {quantity} robot part{(quantity == 1 ? string.Empty : "s")} for {cost} LE and wrap them against the dust.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "BuyRobotParts", before, session.CaptureStats(), $"Bought {quantity} robot parts for {cost} LE");
        return true;
    }

    internal static bool RepairRobot(GameSession session, Guid robotId)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Workshop)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, session.CaptureStats(), "Not at workshop");
            session.RaiseEvent("Repairs have to happen at Abu Samir's workshop bench.");
            return false;
        }

        var robot = session.Player.Robotics.GetRobot(robotId);
        if (robot is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, session.CaptureStats(), "Robot not found");
            session.RaiseEvent("You cannot repair a machine that is not yours.");
            return false;
        }

        if (robot.Condition >= 100)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, session.CaptureStats(), "Robot already fully repaired");
            session.RaiseEvent("That machine is already running as well as its old parts allow.");
            return false;
        }

        if (session.Player.Robotics.Parts <= 0)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, session.CaptureStats(), "No robot parts");
            session.RaiseEvent("You need at least one spare robot part before Abu Samir will open the casing.");
            return false;
        }

        var definition = RobotRegistry.GetByType(robot.Type);
        var repairCost = RobotRepairCostCalculator.GetRepairCost(
            session.Player.Skills.GetLevel(SkillId.RobotRepair),
            definition.RepairCost);
        if (session.Player.Stats.Money < repairCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "RepairRobot", before, session.CaptureStats(), $"Not enough money (need {repairCost} LE)");
            session.RaiseEvent($"Bench time and solder cost {repairCost} LE, even when you bring the part.");
            return false;
        }

        if (repairCost > 0)
        {
            session.Player.Stats.ModifyMoney(-repairCost);
        }

        session.Player.Robotics.TryRepairRobot(robotId);
        var repairMessage = repairCost == 0
            ? $"You open the {definition.Name}'s casing yourself and seat the part. It runs at {robot.Condition}% condition."
            : repairCost < definition.RepairCost
                ? $"You work the bench beside Abu Samir to bring your {definition.Name} up to {robot.Condition}% condition for {repairCost} LE."
                : $"Abu Samir uses one spare part to bring your {definition.Name} up to {robot.Condition}% condition.";
        session.RaiseEvent(repairMessage);
        session.RecordMutation(MutationCategories.HouseholdAsset, "RepairRobot", before, session.CaptureStats(), $"Repaired {definition.Name} for {repairCost} LE and one part");
        return true;
    }

    internal static bool PayPetCare(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "PayPetCare", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to be home to sort out pet care.");
            return false;
        }

        var cost = session.Player.HouseholdAssets.GetPetCareCostDue(session.CurrentWeek);
        if (cost <= 0)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "PayPetCare", before, session.CaptureStats(), "Pet care already covered");
            session.RaiseEvent("Pet care is already covered for this week.");
            return false;
        }

        if (session.Player.Stats.Money < cost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "PayPetCare", before, session.CaptureStats(), $"Not enough money (need {cost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. Pet food for the week costs {cost} LE.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-cost);
        session.Player.HouseholdAssets.PayPetCare(session.CurrentWeek);
        session.RaiseEvent($"You cover this week's pet food and care supplies for {cost} LE.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "PayPetCare", before, session.CaptureStats(), $"Paid pet care {cost} LE");
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
        var definition = PlantRegistry.GetByType(plant.Type);
        session.RaiseEvent($"{definition.Name}: {PlantUpgradeCatalog.GetName(upgradeType)} added for {cost} LE.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "UpgradePlant", before, session.CaptureStats(), $"Upgraded {definition.Name} with {PlantUpgradeCatalog.GetName(upgradeType)} for {cost} LE");
        return true;
    }

    internal static bool UpgradeFishTank(GameSession session, FishTankUpgradeType upgradeType)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to be home to work on the fish tank.");
            return false;
        }

        var fishTank = session.Player.HouseholdAssets.GetFishTank();
        if (fishTank is null)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, session.CaptureStats(), "No fish tank");
            session.RaiseEvent("You don't have a fish tank to upgrade.");
            return false;
        }

        var cost = FishTankUpgradeCatalog.GetCost(upgradeType);
        if (session.Player.Stats.Money < cost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, session.CaptureStats(), $"Not enough money (need {cost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. {FishTankUpgradeCatalog.GetName(upgradeType)} costs {cost} LE.");
            return false;
        }

        if (!session.Player.HouseholdAssets.TryUpgradeFishTank(upgradeType, session.CurrentWeek))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, session.CaptureStats(), $"{FishTankUpgradeCatalog.GetName(upgradeType)} already active");
            session.RaiseEvent($"{FishTankUpgradeCatalog.GetName(upgradeType)} is already active for the fish tank.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-cost);
        session.RaiseEvent($"Fish Tank: {FishTankUpgradeCatalog.GetName(upgradeType)} added for {cost} LE.");
        session.RecordMutation(MutationCategories.HouseholdAsset, "UpgradeFishTank", before, session.CaptureStats(), $"Upgraded fish tank with {FishTankUpgradeCatalog.GetName(upgradeType)} for {cost} LE");
        return true;
    }

    internal static void Restore(
        GameSession session,
        IEnumerable<OwnedPet> pets,
        IEnumerable<OwnedPlant> plants,
        bool hasStreetCatEncounter,
        int lastStreetCatEncounterDay,
        int totalHerbEarnings,
        IEnumerable<OwnedRobot>? robots,
        int robotParts)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(pets);
        ArgumentNullException.ThrowIfNull(plants);
        session.Player.HouseholdAssets.Restore(pets, plants, hasStreetCatEncounter, lastStreetCatEncounterDay, totalHerbEarnings);
        session.Player.Robotics.Restore(robots ?? [], robotParts);
    }

    internal static void ResolveWeekly(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var resolution = session.Player.HouseholdAssets.ResolveWeeklyNeglect(session.CurrentWeek);
        if (resolution.StressPenalty <= 0)
        {
            return;
        }

        session.Player.Stats.ModifyStress(resolution.StressPenalty);
        session.RaiseAutoTransaction($"Skipping household care all week weighs on your mother. Stress +{resolution.StressPenalty}.");
    }

    internal static void TryRollStreetCatEncounter(GameSession session, Random random)
    {
#pragma warning disable CA5394
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(random);

        if (session.World.CurrentLocationId != LocationId.Home || session.Clock.Day < 3)
        {
            return;
        }

        if (random.NextDouble() >= 0.15)
        {
            return;
        }

        if (session.Player.HouseholdAssets.TryTriggerStreetCatEncounter(session.Clock.Day))
        {
            session.RaiseEvent("A street cat starts waiting near your building door as if it has already chosen you.");
        }
#pragma warning restore CA5394
    }
}
