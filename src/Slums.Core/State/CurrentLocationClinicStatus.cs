namespace Slums.Core.State;

public sealed record CurrentLocationClinicStatus(
    bool HasClinicServices,
    bool IsOpenToday,
    int VisitCost,
    string LocationName,
    string CurrentDayName,
    string OpenDaysSummary);
