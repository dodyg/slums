using Slums.Core.Characters;
using Slums.Core.Calendar;
using Slums.Core.Diagnostics;
using Slums.Core.Economy;
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
        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        var schedule = session.GetCurrentSchedule();
        var seasonModifiers = SeasonModifiersRegistry.GetModifiers(session.GetCurrentSeason());
        var baseModifier = (districtCondition?.Effect.FoodCostModifier ?? 0) + schedule.FoodCostModifier + seasonModifiers.FoodCostModifier + session.CurrentWeather.FoodCostModifier;
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && schedule.FoodCostModifier < 0)
        {
            baseModifier -= 1;
        }

        baseModifier += TerritoryDynamicsCalculator.GetFoodPriceModifier(session.Territory, session.World.CurrentDistrict);
        baseModifier += GetUmmKarimFoodDiscount(session);
        baseModifier += NewsImpactCalculator.GetFoodPriceModifier(session.News, session.World.CurrentDistrict);

        var modifiedCost = session.LocationPricing.GetFoodCost(session.World.CurrentDistrict) + baseModifier;
        return Math.Max(1, modifiedCost);
    }

    internal static int GetStreetFoodCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        var schedule = session.GetCurrentSchedule();
        var seasonModifiers = SeasonModifiersRegistry.GetModifiers(session.GetCurrentSeason());
        var baseModifier = (districtCondition?.Effect.StreetFoodCostModifier ?? 0) + schedule.FoodCostModifier + seasonModifiers.FoodCostModifier + session.CurrentWeather.FoodCostModifier;
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee && schedule.FoodCostModifier < 0)
        {
            baseModifier -= 1;
        }

        baseModifier += TerritoryDynamicsCalculator.GetFoodPriceModifier(session.Territory, session.World.CurrentDistrict);
        baseModifier += GetUmmKarimFoodDiscount(session);
        baseModifier += NewsImpactCalculator.GetFoodPriceModifier(session.News, session.World.CurrentDistrict);

        var modifiedCost = session.LocationPricing.GetStreetFoodCost(session.World.CurrentDistrict) + baseModifier;
        return Math.Max(1, modifiedCost);
    }

    internal static int GetMedicineCost(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var districtCondition = session.GetActiveDistrictConditionDefinition(session.World.CurrentDistrict);
        var modifiedCost = session.LocationPricing.GetMedicineCost(session.World.CurrentDistrict, session.World.CurrentLocationId, session.Relationships, session.Player.Skills)
            + (districtCondition?.Effect.MedicineCostModifier ?? 0)
            + InfrastructureImpactCalculator.GetMedicinePriceModifier(session.Infrastructure, session.World.CurrentDistrict);
        return Math.Max(1, modifiedCost);
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
        session.Player.Household.AddStaples(3);
        if (session.Player.BackgroundType == BackgroundType.SudaneseRefugee)
        {
            session.Player.Household.AddStaples(1);
            session.RaiseEvent("A Sudanese women-led kitchen stretches the bread run a little farther for you.");
        }

        session.RaiseEvent($"Bought food supplies for {foodCost} LE in {DistrictInfo.GetName(session.World.CurrentDistrict)}. Stockpile: {session.Player.Household.FoodStockpile}");
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

    private static int GetUmmKarimFoodDiscount(GameSession session)
    {
        var ummKarimEconomy = session.NpcEconomies.GetEconomy(NpcId.FixerUmmKarim);
        return ummKarimEconomy.WealthLevel == NpcWealthLevel.Comfortable ? -1 : 0;
    }
}
