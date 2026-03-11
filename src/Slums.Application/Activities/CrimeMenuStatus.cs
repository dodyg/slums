using Slums.Core.Crimes;

namespace Slums.Application.Activities;

public sealed record CrimeMenuStatus(
    CrimeAttempt Attempt,
    bool IsAvailable,
    string? StatusText,
    string? BlockReason);