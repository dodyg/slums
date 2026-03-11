using Slums.Core.Crimes;

namespace Slums.Application.Activities;

public sealed record CrimeMenuStatus(
    CrimeAttempt Attempt,
    bool IsAvailable,
    string? StatusText,
    string? BlockReason,
    int EffectiveDetectionRisk,
    int EffectiveSuccessChance,
    int EffectivePressureIfDetected,
    int EffectivePressureIfUndetected,
    IReadOnlyList<string> ActiveModifiers);