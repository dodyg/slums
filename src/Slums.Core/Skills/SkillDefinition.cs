namespace Slums.Core.Skills;

/// <summary>Player-facing metadata for a persisted skill identifier.</summary>
public sealed record SkillDefinition(
    SkillId Id,
    string DisplayName,
    string Description,
    IReadOnlyDictionary<int, string> Thresholds);
