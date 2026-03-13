namespace Slums.Core.State;

public readonly record struct MotherClinicVisitResult(bool Success, int TotalCost, int HealthChange);
