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

    public int CalculatePay(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
#pragma warning disable CA5394 // Random is sufficient for gameplay mechanics
        var variance = random.Next(-PayVariance, PayVariance + 1);
#pragma warning restore CA5394
        return Math.Max(0, BasePay + variance);
    }
}
