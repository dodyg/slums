namespace Slums.Core.Heat;

/// <summary>Shared police-pressure thresholds used by simulation, narrative, and presentation.</summary>
public static class PolicePressureThresholds
{
    public const int Elevated = 50;
    public const int MaterialRisk = 60;
    public const int Hot = 70;
    public const int ArrestWarning = 80;
    public const int LongRunArrest = 85;
    public const int CloseCall = 90;
    public const int Arrest = 100;
}
