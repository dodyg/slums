using Slums.Core.Characters;
using Slums.Core.Calendar;
using Slums.Core.Diagnostics;
using Slums.Core.Economy;
using Slums.Core.Home;
using Slums.Core.Investments;
using Slums.Core.Relationships;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;
using Slums.Core.World.News;

namespace Slums.Core.Expenses;

/// <summary>Calculates food and medicine prices and applies shop purchases.</summary>
internal static class FoodShopService
{
    internal static int GetFoodCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return PriceModifierPipeline.GetFoodCost(session);
    }

    internal static int GetStreetFoodCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return PriceModifierPipeline.GetStreetFoodCost(session);
    }

    internal static int GetMedicineCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return PriceModifierPipeline.GetMedicineCost(session);
    }

    internal static bool BuyFood(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var foodCost = GetFoodCost(session);
        if (session.Player.Stats.Money < foodCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyFood", before, session.CaptureStats(), $"Not enough money (need {foodCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. Food costs {foodCost} LE.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-foodCost);
        var provisioningLevel = session.Player.Skills.GetLevel(SkillId.Provisioning);
        var bundleUnits = ProvisioningCalculator.GetFoodBundleUnits(provisioningLevel);
        session.Player.Household.AddStaples(bundleUnits);
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee)
        {
            session.Player.Household.AddStaples(1);
            session.RaiseEvent("A Sudanese women-led kitchen stretches the bread run a little farther for you.");
        }

        session.RaiseEvent($"Bought food supplies for {foodCost} LE in {DistrictInfo.GetName(session.World.CurrentDistrict)}. Stockpile +{bundleUnits}: {session.Player.Household.FoodStockpile}");
        session.RecordMutation(MutationCategories.Food, "BuyFood", before, session.CaptureStats(), $"Bought food for {foodCost} LE");
        return true;
    }

    internal static bool BuyMedicine(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        var medicineCost = GetMedicineCost(session);
        if (session.Player.Stats.Money < medicineCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "BuyMedicine", before, session.CaptureStats(), $"Not enough money (need {medicineCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"Not enough money. Medicine costs {medicineCost} LE.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-medicineCost);
        session.Player.Household.AddMedicine(2);
        session.ApplySkillGain(SkillId.Medical);
        session.RaiseEvent($"Bought medicine for {medicineCost} LE. Medicine stock: {session.Player.Household.MedicineStock}");
        session.RecordMutation(MutationCategories.Shop, "BuyMedicine", before, session.CaptureStats(), $"Bought medicine for {medicineCost} LE");
        return true;
    }
}
