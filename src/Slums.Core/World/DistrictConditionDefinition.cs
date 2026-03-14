namespace Slums.Core.World;

public sealed record DistrictConditionDefinition
{
    public string Id { get; init; } = string.Empty;

    public DistrictId District { get; init; }

    public string Title { get; init; } = string.Empty;

    public string BulletinText { get; init; } = string.Empty;

    public string GameplaySummary { get; init; } = string.Empty;

    public int MinDay { get; init; } = 1;

    public int Weight { get; init; } = 1;

    public int? MinPolicePressure { get; init; }

    public int? MaxPolicePressure { get; init; }

    public DistrictConditionEffect Effect { get; init; } = new();

    public bool IsEligible(int currentDay, int policePressure)
    {
        if (currentDay < MinDay)
        {
            return false;
        }

        if (MinPolicePressure is int minPolicePressure && policePressure < minPolicePressure)
        {
            return false;
        }

        if (MaxPolicePressure is int maxPolicePressure && policePressure > maxPolicePressure)
        {
            return false;
        }

        return true;
    }
}
