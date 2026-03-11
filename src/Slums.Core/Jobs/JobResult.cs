namespace Slums.Core.Jobs;

public sealed class JobResult
{
    public bool Success { get; init; }
    public bool MistakeMade { get; init; }
    public int MoneyEarned { get; init; }
    public int EnergyCost { get; init; }
    public int StressChange { get; init; }
    public int ReliabilityChange { get; init; }
    public int LockoutUntilDay { get; init; }
    public string Message { get; init; } = string.Empty;

    public static JobResult Failed(string reason) => new()
    {
        Success = false,
        Message = reason
    };

    public static JobResult SuccessWork(int money, int energy, int stress, string message, int reliabilityChange = 0, int lockoutUntilDay = 0, bool mistakeMade = false) => new()
    {
        Success = true,
        MistakeMade = mistakeMade,
        MoneyEarned = money,
        EnergyCost = energy,
        StressChange = stress,
        ReliabilityChange = reliabilityChange,
        LockoutUntilDay = lockoutUntilDay,
        Message = message
    };
}
