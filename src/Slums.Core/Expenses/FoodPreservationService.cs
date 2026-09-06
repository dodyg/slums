using Slums.Core.Diagnostics;
using Slums.Core.Skills;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Expenses;

/// <summary>Applies Provisioning's time- and supply-limited food preservation action.</summary>
internal static class FoodPreservationService
{
    internal const int RequiredSkillLevel = SkillThresholds.HighLevel;
    internal const int InputFoodUnits = 2;
    internal const int OutputMealUnits = 1;
    internal const int TimeCostMinutes = 60;
    internal const int EnergyCost = 8;

    internal static FoodPreservationPreview Preview(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var atHome = session.World.CurrentLocationId == LocationId.Home;
        var hasSkill = session.Player.Skills.GetLevel(SkillId.Provisioning) >= RequiredSkillLevel;
        var hasFood = session.Player.Household.FoodStockpile >= InputFoodUnits;
        var hasTime = session.CanCompleteActivityToday(TimeCostMinutes);
        var hasEnergy = session.Player.Stats.Energy >= EnergyCost;
        var canPerform = atHome && hasSkill && hasFood && hasTime && hasEnergy;
        var reason = GetUnavailabilityReason(atHome, hasSkill, hasFood, hasTime, hasEnergy);

        return new FoodPreservationPreview(
            RequiredSkillLevel,
            InputFoodUnits,
            OutputMealUnits,
            TimeCostMinutes,
            EnergyCost,
            session.Player.Household.FoodStockpile,
            session.Player.Household.PreservedMealUnits,
            atHome,
            hasSkill,
            hasFood,
            hasTime,
            hasEnergy,
            canPerform,
            reason);
    }

    internal static bool Perform(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var before = session.CaptureStats();
        var preview = Preview(session);
        if (!preview.CanPerform)
        {
            var reason = preview.UnavailabilityReason ?? "Food preservation is not available.";
            session.RaiseEvent(reason);
            session.RecordMutation(MutationCategories.GuardRejected, "PreserveFood", before, session.CaptureStats(), reason);
            return false;
        }

        if (!session.Player.Household.PreserveFood(InputFoodUnits))
        {
            throw new InvalidOperationException("Food preservation preview became invalid before commitment.");
        }

        session.Player.Stats.ModifyEnergy(-EnergyCost);
        session.RaiseEvent($"You preserve {InputFoodUnits} staple units against Cairo's heat and price shocks. Preserved meals +{OutputMealUnits}; stockpile: {session.Player.Household.FoodStockpile}.");
        session.RecordMutation(MutationCategories.Food, "PreserveFood", before, session.CaptureStats(), $"Preserved {InputFoodUnits} food units with Provisioning {session.Player.Skills.GetLevel(SkillId.Provisioning)}");
        session.AdvanceTime(TimeCostMinutes);
        return true;
    }

    private static string? GetUnavailabilityReason(bool atHome, bool hasSkill, bool hasFood, bool hasTime, bool hasEnergy)
    {
        if (!atHome)
        {
            return "Return home to preserve food.";
        }

        if (!hasSkill)
        {
            return $"Reach Provisioning {RequiredSkillLevel}.";
        }

        if (!hasFood)
        {
            return $"Requires {InputFoodUnits} staple units.";
        }

        if (!hasTime)
        {
            return $"Requires {TimeCostMinutes} minutes before 22:00.";
        }

        if (!hasEnergy)
        {
            return $"Requires {EnergyCost} energy.";
        }

        return null;
    }
}
