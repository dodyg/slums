namespace Slums.Application.Activities;

public sealed record ClinicTravelMenuStatus(
    string LocationName,
    string DistrictName,
    int TravelCost,
    int ClinicCost,
    int TotalCost,
    bool IsOpenToday,
    string OpenDaysSummary,
    int TravelTimeMinutes,
    bool CanAfford,
    string? UnavailableReason);
