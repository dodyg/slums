using Slums.Core.Crimes;

namespace Slums.Application.Activities;

public sealed record CrimeMenuOptionContext(
    CrimeAttempt Attempt,
    CrimeRoutePreview Preview,
    bool IsAvailable,
    bool AvailableViaRegistry,
    string? BlockReason);
