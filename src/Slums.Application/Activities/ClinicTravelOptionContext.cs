using Slums.Core.World;

namespace Slums.Application.Activities;

public sealed record ClinicTravelOptionContext(
    LocationId LocationId,
    string LocationName,
    string DistrictName,
    int TravelCost,
    int ClinicCost,
    int TotalCost,
    bool IsOpenToday,
    string OpenDaysSummary,
    int TravelTimeMinutes,
    bool CanAfford,
    bool IsValidOption);
