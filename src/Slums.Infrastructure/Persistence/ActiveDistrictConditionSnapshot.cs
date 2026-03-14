using Slums.Core.World;

namespace Slums.Infrastructure.Persistence;

public sealed record ActiveDistrictConditionSnapshot
{
    public string District { get; init; } = DistrictId.Imbaba.ToString();

    public string ConditionId { get; init; } = string.Empty;

    public static ActiveDistrictConditionSnapshot Capture(ActiveDistrictCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        return new ActiveDistrictConditionSnapshot
        {
            District = condition.District.ToString(),
            ConditionId = condition.ConditionId
        };
    }

    public ActiveDistrictCondition Restore()
    {
        if (!Enum.TryParse<DistrictId>(District, ignoreCase: false, out var districtId))
        {
            throw new InvalidOperationException($"Unknown district '{District}' in active district condition snapshot.");
        }

        return new ActiveDistrictCondition
        {
            District = districtId,
            ConditionId = ConditionId
        };
    }
}
