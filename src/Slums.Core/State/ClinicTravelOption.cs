using Slums.Core.World;

namespace Slums.Core.State;

public sealed record ClinicTravelOption(
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
