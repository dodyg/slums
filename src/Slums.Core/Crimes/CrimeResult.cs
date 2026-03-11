namespace Slums.Core.Crimes;

public sealed class CrimeResult
{
    public bool Success { get; init; }
    public bool Detected { get; init; }
    public bool ArrestWarning { get; init; }
    public int MoneyEarned { get; init; }
    public int EnergyCost { get; init; }
    public int StressCost { get; init; }
    public int PolicePressureDelta { get; init; }
    public string Message { get; init; } = string.Empty;
}