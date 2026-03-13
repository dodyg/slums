namespace Slums.Core.State;

public readonly record struct TravelAndClinicVisitResult(
    bool Success,
    int TravelCost,
    int ClinicCost,
    int TotalCost,
    int HealthChange);
