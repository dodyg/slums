namespace Slums.Core.Crimes;

public sealed record CrimeResolutionPreview(
    int DetectionChance,
    int SuccessChance,
    int PolicePressureIfDetected,
    int PolicePressureIfUndetected);