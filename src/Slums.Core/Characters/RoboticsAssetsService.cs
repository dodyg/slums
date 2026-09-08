using Slums.Core.Diagnostics;
using Slums.Core.Robotics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Characters;

/// <summary>Applies robot purchases, spare-part purchases, and repairs.</summary>
internal static class RoboticsAssetsService
{
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

        var definition = session.Player.Robotics.GetDefinition(robotType);
        if (!session.Player.Robotics.CanPurchaseRobot)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobot", before, session.CaptureStats(), "Robot limit reached");
            session.RaiseEvent($"The flat and the alley can only support {RoboticsState.MaxOwnedRobots} machines at once.");
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

        var cost = quantity * RoboticsState.PartsPurchaseCost;
        if (!session.Player.Robotics.CanBuyParts(quantity))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyRobotParts", before, session.CaptureStats(), "Parts storage limit reached");
            session.RaiseEvent($"You can carry at most {RoboticsState.MaxParts} spare robot parts in the flat.");
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

        var definition = session.Player.Robotics.GetDefinition(robot.Type);
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
}
