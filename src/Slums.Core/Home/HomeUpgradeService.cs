using Slums.Core.Diagnostics;
using Slums.Core.State;
using Slums.Core.World;

namespace Slums.Core.Home;

/// <summary>Applies home upgrade purchases, restoration, and rest actions to a session.</summary>
internal static class HomeUpgradeService
{
    internal static bool RestAtHome(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "RestAtHome", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to go home to rest.");
            return false;
        }

        var seasonRestBonus = session.GetCurrentSeasonModifiers().RestRecoveryBonus;
        var recovery = SleepQualityCalculator.CalculateRecovery(
            session.Player.Stats, session.Player.Nutrition, session.Player.Household,
            session.UnpaidRentDays, session.HomeUpgrades, seasonRestBonus);

        session.Player.Stats.ModifyEnergy(recovery);
        session.Player.Stats.ModifyHunger(-10);
        session.Player.Stats.ModifyStress(-15);
        session.AdvanceTime(8 * 60);

        var breakdown = SleepQualityCalculator.BuildRecoveryBreakdown(
            recovery, session.Player.Stats, session.Player.Nutrition, session.Player.Household,
            session.UnpaidRentDays, session.HomeUpgrades, seasonRestBonus);
        session.RaiseEvent($"You rest at home. Energy +{recovery}. ({breakdown})");
        session.RecordMutation(MutationCategories.Rest, "RestAtHome", before, session.CaptureStats(), "Rested at home");
        return true;
    }

    internal static bool Purchase(GameSession session, HomeUpgrade upgrade)
    {
        ArgumentNullException.ThrowIfNull(session);
        var before = session.CaptureStats();
        if (session.World.CurrentLocationId != LocationId.Home)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPurchaseHomeUpgrade", before, session.CaptureStats(), "Not at home");
            session.RaiseEvent("You need to be at home to improve it.");
            return false;
        }

        if (session.HomeUpgrades.HasUpgrade(upgrade))
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPurchaseHomeUpgrade", before, session.CaptureStats(), $"{upgrade} already purchased");
            session.RaiseEvent($"You already have {HomeUpgradeDefinitions.GetDescription(upgrade)}.");
            return false;
        }

        var cost = HomeUpgradeDefinitions.GetCost(upgrade);
        if (session.Player.Stats.Money < cost)
        {
            session.RecordMutation(MutationCategories.GuardRejected, "TryPurchaseHomeUpgrade", before, session.CaptureStats(), $"Not enough money ({cost} LE)");
            session.RaiseEvent($"You can't afford that. You need {cost} LE but only have {session.Player.Stats.Money} LE.");
            return false;
        }

        session.Player.Stats.ModifyMoney(-cost);
        session.HomeUpgrades.Purchase(upgrade);
        session.RaiseEvent($"You bought {HomeUpgradeDefinitions.GetDescription(upgrade)} for {cost} LE.");
        session.RecordMutation(MutationCategories.Shop, "TryPurchaseHomeUpgrade", before, session.CaptureStats(), $"Purchased {upgrade} for {cost} LE");
        return true;
    }

    internal static IReadOnlyList<HomeUpgrade> GetAvailable(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return HomeUpgradeDefinitions.AllUpgrades
            .Where(upgrade => !session.HomeUpgrades.HasUpgrade(upgrade))
            .ToList();
    }

    internal static void Restore(GameSession session, IEnumerable<HomeUpgrade> upgrades)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(upgrades);
        session.HomeUpgrades.Restore(upgrades);
    }
}
