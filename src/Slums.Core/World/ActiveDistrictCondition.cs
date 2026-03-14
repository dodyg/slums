namespace Slums.Core.World;

public sealed record ActiveDistrictCondition
{
    public DistrictId District { get; init; }

    public string ConditionId { get; init; } = string.Empty;
}
