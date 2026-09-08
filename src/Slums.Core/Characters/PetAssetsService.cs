using Slums.Core.Diagnostics;
using Slums.Core.Randomness;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Characters;

/// <summary>Applies pet and fish-tank purchases, care, upgrades, and street-cat encounters.</summary>
internal static class PetAssetsService
{
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

        var definition = session.Player.HouseholdAssets.GetPetDefinition(PetType.Fish);
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

    internal static void TryRollStreetCatEncounter(GameSession session, Random random)
    {
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
    }
}
