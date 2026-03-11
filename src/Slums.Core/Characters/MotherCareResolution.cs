namespace Slums.Core.Characters;

public sealed record MotherCareResolution(
    int HealthDelta,
    int StressDelta,
    MotherCondition ConditionAfterResolution,
    bool MotherAlive);