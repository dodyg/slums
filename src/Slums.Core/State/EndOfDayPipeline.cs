using Slums.Core.Diagnostics;
using Slums.Core.State.DailyResolution;

namespace Slums.Core.State;

/// <summary>
/// Coordinates the ordered daily resolution steps for a game session. The step order and
/// the sequence of random draws are observable behavior (persisted RNG state) and must not
/// be reordered.
/// </summary>
internal static class EndOfDayPipeline
{
    internal static void Run(GameSession session, Random? random)
    {
        ArgumentNullException.ThrowIfNull(session);
        var resolvedRandom = random ?? session.SharedRandom;
        var beforeStats = session.CaptureStats();
        var currentWeek = session.CurrentWeek;

        DailyStatResolution.ApplyDecayAndRecovery(session, currentWeek);
        DailyEconomyResolution.ProcessRent(session);
        DailyStatResolution.RaiseDailyRecapEvents(session);
        DailyWorldResolution.DecayPressures(session);
        DailyStatResolution.ApplyBackgroundAndGenderStress(session);
        DailyEconomyResolution.ResolveHerbIncome(session, currentWeek);
        DailyWorldResolution.AdvanceToNextMorning(session, resolvedRandom, beforeStats);
        DailyEconomyResolution.ResolveWeeklyCycle(session, resolvedRandom);
        session.QueueCityCrisisBeat();
        DailyStatResolution.BeginNewDay(session);
        DailyInformationResolution.ResolveAttendance(session);
        DailyWorldResolution.RollDailyEvents(session, resolvedRandom);
        DailyInformationResolution.ResolveRumors(session);
        DailyEconomyResolution.ProcessDailyDebt(session);
        DailyInformationResolution.ResolvePhoneAndTips(session, resolvedRandom);
        session.CheckGameOverConditions();
        session.RecordMutation(MutationCategories.DayTransition, "EndDay", beforeStats, session.CaptureStats(), $"Day {session.CurrentDay} completed");
    }
}
