using Slums.Core.Calendar;
using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Economy;
using Slums.Core.Home;
using Slums.Core.Relationships;
using Slums.Core.State;
using Slums.Core.Territory;
using Slums.Core.World;

namespace Slums.Core.Narrative;

/// <summary>Builds immutable signals used by end-of-day narrative planners.</summary>
internal static class NarrativeFollowUpContextBuilder
{
    internal static NarrativeReachabilityContext BuildReachabilityContext(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var holiday = session.GetActiveHolidayState();
        return new NarrativeReachabilityContext(
            session.Clock.Day,
            session.CurrentWeather.Type,
            session.GetCurrentSeason(),
            holiday.Id,
            holiday.CurrentDay,
            session.Player.BackgroundType,
            session.GetCurrentDayOfWeek(),
            session.World.CurrentLocationId == LocationId.Home,
            session.HomeUpgrades.HasUpgrade(HomeUpgrade.Curtain),
            session.Player.Household.MotherHealth);
    }

    internal static NarrativeCommunityDebtContext BuildCommunityDebtContext(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var loanSharkDebt = session.PlayerDebts.Debts.FirstOrDefault(static debt => debt.Source == DebtSource.LoanShark);
        var mona = session.Relationships.GetNpcRelationship(NpcId.NeighborMona);
        var youssef = session.Relationships.GetNpcRelationship(NpcId.RunnerYoussef);
        var nadia = session.Relationships.GetNpcRelationship(NpcId.CafeOwnerNadia);
        var mariam = session.Relationships.GetNpcRelationship(NpcId.PharmacistMariam);
        var imbaba = session.Territory.GetControl(DistrictId.Imbaba);

        return new NarrativeCommunityDebtContext(
            session.Clock.Day,
            session.GetCurrentDayOfWeek(),
            session.Player.BackgroundType,
            session.EventAttendance.TotalAttended,
            session.EventAttendance.ConsecutiveSkips,
            session.EventAttendance.HasTeaCircleInvitation,
            session.PolicePressure,
            session.CrimesCommitted,
            session.HonestShiftsCompleted,
            mona.Trust,
            youssef.Trust,
            nadia.Trust,
            mariam.Trust,
            mona.WasHelped,
            youssef.WasHelped,
            loanSharkDebt is not null,
            loanSharkDebt?.DaysOverdue(session.Clock.Day) ?? 0,
            loanSharkDebt is null ? 0 : Math.Max(0, loanSharkDebt.DueDay - session.Clock.Day),
            session.PlayerDebts.Debts.Any(static debt => debt.Source is DebtSource.NeighborLoan or DebtSource.CommunityMutualAid),
            imbaba.Tension,
            imbaba.TensionLevel,
            imbaba.ControllingFaction == FactionId.DokkiThugs,
            imbaba.ControllingFaction == FactionId.ExPrisonerNetwork);
    }
}
