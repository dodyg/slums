using Slums.Core.Characters;
using Slums.Core.Diagnostics;
using Slums.Core.Economy;
using Slums.Core.Skills;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Core.Home;

/// <summary>Applies home and street meal activities to a game session.</summary>
internal static class MealService
{
    internal static bool EatAtHome(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var before = session.CaptureStats();
        if (!session.Player.Household.FeedMother())
        {
            session.RecordMutation(MutationCategories.GuardRejected, "EatAtHome", before, session.CaptureStats(), "Not enough food at home");
            session.RaiseEvent("There is not enough food at home.");
            return false;
        }

        var provisioningLevel = session.Player.Skills.GetLevel(SkillId.Provisioning);
        var cookingBonus = session.Player.HouseholdAssets.GetHomeCookingBonus(session.CurrentWeek);
        var mealPlan = ProvisioningCalculator.GetMealPlan(provisioningLevel, cookingBonus);
        session.Player.Nutrition.Eat(mealPlan.Quality);
        session.SyncLegacyHunger();
        if (mealPlan.StressReduction > 0)
        {
            session.Player.Stats.ModifyStress(-mealPlan.StressReduction);
        }

        var mealDescription = mealPlan.Quality == MealQuality.HotMeal
            ? "You turn the staples and herbs into a hot meal and make sure your mother eats too."
            : "You eat a simple meal at home and make sure your mother eats too.";
        session.RaiseEvent(mealDescription);
        if (mealPlan.StressReduction > 0)
        {
            session.RaiseEvent($"Fresh herbs soften the meal a little. Stress -{mealPlan.StressReduction}.");
        }

        var motherBonus = ProvisioningCalculator.GetMotherCareMealBonus(provisioningLevel, mealPlan.Quality);
        if (motherBonus > 0)
        {
            session.Player.Household.UpdateMotherHealth(motherBonus);
            session.RaiseEvent($"Your mother manages a little more of the meal. Mother's health +{motherBonus}.");
        }

        session.RecordMutation(MutationCategories.Food, "EatAtHome", before, session.CaptureStats(), "Ate at home");
        return true;
    }

    internal static bool EatStreetFood(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var before = session.CaptureStats();
        var streetFoodCost = session.GetStreetFoodCost();
        if (session.Player.Stats.Money < streetFoodCost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "EatStreetFood", before, session.CaptureStats(), $"Not enough money (need {streetFoodCost} LE, have {session.Player.Stats.Money} LE)");
            session.RaiseEvent($"You do not have enough money for street food. It costs {streetFoodCost} LE here.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-streetFoodCost);
        session.Player.Nutrition.Eat(MealQuality.Basic);
        session.SyncLegacyHunger();
        session.RaiseEvent($"You grab a cheap meal from the street for {streetFoodCost} LE.");
        session.RecordMutation(MutationCategories.Food, "EatStreetFood", before, session.CaptureStats(), $"Ate street food for {streetFoodCost} LE");
        return true;
    }

    internal static int GetUmmKarimFoodDiscount(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var ummKarimEconomy = session.NpcEconomies.GetEconomy(NpcId.FixerUmmKarim);
        return ummKarimEconomy.WealthLevel == NpcWealthLevel.Comfortable ? -1 : 0;
    }
}
