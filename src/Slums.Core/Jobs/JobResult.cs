namespace Slums.Core.Jobs;

public sealed class JobResult
{
    public bool Success { get; init; }
    public int MoneyEarned { get; init; }
    public int EnergyCost { get; init; }
    public int StressChange { get; init; }
    public string Message { get; init; } = string.Empty;

    public static JobResult Failed(string reason) => new()
    {
        Success = false,
        Message = reason
    };

    public static JobResult SuccessWork(int money, int energy, int stress, string message) => new()
    {
        Success = true,
        MoneyEarned = money,
        EnergyCost = energy,
        StressChange = stress,
        Message = message
    };
}
