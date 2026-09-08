namespace Slums.Core.Jobs;

public sealed class JobShift
{
    public JobType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int BasePay { get; init; }
    public int EnergyCost { get; init; }
    public int StressCost { get; init; }
    public int DurationMinutes { get; init; }
    public int MinEnergyRequired { get; init; } = 20;
    public int PayVariance { get; init; } = 5;

    public int CalculatePay(Random? random = null)
    {
        if (random is null)
        {
            return Math.Max(0, BasePay);
        }

        var variance = random.Next(-PayVariance, PayVariance + 1);
        return Math.Max(0, BasePay + variance);
    }
}
