namespace Slums.Core.World;

public sealed record InfrastructureServiceState
{
    public DistrictId District { get; init; }
    public InfrastructureServiceType Service { get; init; }
    public InfrastructureSeverity Severity { get; init; }
    public int StartDay { get; init; }
    public int RemainingDays { get; init; }
    public string? SourceId { get; init; }

    public bool IsActive => Severity != InfrastructureSeverity.Normal && RemainingDays > 0;
}
