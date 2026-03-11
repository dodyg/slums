namespace Slums.Core.Crimes;

public sealed record CrimeRoutePreview(
    CrimeAttempt Attempt,
    CrimeResolutionPreview Resolution,
    IReadOnlyList<string> ActiveModifiers);