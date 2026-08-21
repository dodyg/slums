namespace Slums.Core.World.News;

public sealed record NewsEffectDefinition
{
    public NewsEffectType Type { get; init; }
    public int Amount { get; init; }
    public DistrictId? District { get; init; }
    public InfrastructureServiceType? Service { get; init; }
    public InfrastructureSeverity Severity { get; init; } = InfrastructureSeverity.Strained;
    public int DurationDays { get; init; }
}
