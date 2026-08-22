using Slums.Core.Characters;
using Slums.Core.Clock;
using Slums.Core.Territory;

namespace Slums.Core.Narrative;

/// <summary>
/// Immutable signals used to schedule the authored community and debt scenes.
/// </summary>
public sealed record NarrativeCommunityDebtContext(
    int Day,
    GameDayOfWeek DayOfWeek,
    BackgroundType Background,
    int CommunityAttendance,
    int ConsecutiveCommunitySkips,
    bool HasTeaCircleInvitation,
    int PolicePressure,
    int CrimesCommitted,
    int HonestShiftsCompleted,
    int MonaTrust,
    int YoussefTrust,
    int NadiaTrust,
    int MariamTrust,
    bool MonaWasHelped,
    bool YoussefWasHelped,
    bool HasLoanSharkDebt,
    int LoanSharkDaysOverdue,
    int LoanSharkDaysUntilDue,
    bool HasNeighborDebt,
    int ImbabaTension,
    TensionLevel ImbabaTensionLevel,
    bool ImbabaControlledByDokkiThugs,
    bool ImbabaControlledByExPrisonerNetwork);
