namespace Slums.Core.Crimes;

public sealed record CrimeOpportunityStatus(
    CrimeAttempt Attempt,
    bool IsAvailable,
    string? BlockReason);