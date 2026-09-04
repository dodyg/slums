using Slums.Core.Clock;
using Slums.Core.Expenses;
using Slums.Core.Narrative;

namespace Slums.Core.State.DailyResolution;

using NarrativeStoryFlags = Slums.Core.Narrative.StoryFlags;

/// <summary>
/// Resolves the money-facing blocks of the daily pipeline: rent, herb income, the Monday
/// weekly cycle, and daily debt processing.
/// </summary>
internal static class DailyEconomyResolution
{
    internal static void ProcessRent(GameSession session)
    {
        var rentResult = session.ProcessRentDay();
        if (rentResult.GraceApplied)
        {
            session.RaiseAutoTransaction($"Rent grace used. {rentResult.GraceDaysRemaining} grace day{(rentResult.GraceDaysRemaining == 1 ? string.Empty : "s")} remain.");
        }
        else if (rentResult.Paid)
        {
            session.Player.Stats.ModifyMoney(-RecurringExpenses.DailyRentCost);
            session.RaiseAutoTransaction($"Paid rent: {RecurringExpenses.DailyRentCost} LE");
        }
        else
        {
            session.RaiseAutoTransaction($"Could not pay rent! Debt: {rentResult.AccumulatedDebt} LE. Unpaid days: {rentResult.CurrentUnpaidDays}.");

            if (rentResult.WarningType == RentWarningType.First)
            {
                session.RaiseEvent("The landlord's son knocks hard. \"Three days now. My father is patient, but not forever.\"");
            }
            else if (rentResult.WarningType == RentWarningType.Final)
            {
                session.RaiseEvent("The landlord himself appears. \"Five days. Two more and we put your things on the street.\"");
                session.TryQueueNarrativeTrigger(new NarrativeSceneTrigger(NarrativeStoryFlags.EventRentFinalWarningSeen, NarrativeKnots.EventRentFinalWarning));
            }
        }
    }

    internal static void ResolveHerbIncome(GameSession session, int currentWeek)
    {
        var herbIncome = session.Player.HouseholdAssets.ResolveSellablePlantIncome(session.Clock.Day, currentWeek);
        if (herbIncome > 0)
        {
            session.Player.Stats.ModifyMoney(herbIncome);
            session.RaiseAutoTransaction($"The street vendor moves your herbs quietly. +{herbIncome} LE reaches home.");
        }
    }

    internal static void ResolveWeeklyCycle(GameSession session, Random random)
    {
        if (session.GetCurrentDayOfWeek() != GameDayOfWeek.Monday)
        {
            return;
        }

        session.ResolveWeeklyHouseholdAssets();
        if (session.ActiveInvestments.Count > 0)
        {
            session.ResolveWeeklyInvestments(random);
        }

        session.ResolveWeeklyEconomy(random);
    }

    internal static void ProcessDailyDebt(GameSession session)
    {
        session.BeginDailyActivityLedger();
        session.ProcessDailyDebt();
    }
}
